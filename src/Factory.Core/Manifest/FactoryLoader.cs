// @summary: Parses + validates factory.json and builds the Hall object graph (stations, conveyors, links).
#nullable enable
using System.Text.Json;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Manifest;

public static class FactoryLoader
{
    public static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = false,
    };

    public static FactoryManifest Parse(string json) =>
        JsonSerializer.Deserialize<FactoryManifest>(json, Json) ?? throw new ManifestException("Empty manifest");

    public static Hall Load(string path) => Build(Parse(File.ReadAllText(path)));

    /// <summary>All problems found in the manifest; empty list = valid.</summary>
    public static List<string> Validate(FactoryManifest m)
    {
        var errors = new List<string>();
        var zones = m.Hall.Zones.Select(z => z.Id).ToHashSet();
        foreach (var dup in m.Lines.GroupBy(l => l.Id).Where(g => g.Count() > 1)) errors.Add($"Duplicate line id '{dup.Key}'");

        foreach (var line in m.Lines)
        {
            var p = $"line {line.Id}";
            if (!zones.Contains(line.Zone)) errors.Add($"{p}: unknown zone '{line.Zone}'");
            var ids = line.Stations.Select(s => s.Id).Concat(line.Conveyors.Select(c => c.Id)).ToList();
            foreach (var dup in ids.GroupBy(x => x).Where(g => g.Count() > 1)) errors.Add($"{p}: duplicate id '{dup.Key}'");
            if (line.Stations.Count == 0) { errors.Add($"{p}: no stations"); continue; }

            foreach (var s in line.Stations)
            {
                if (!StationRegistry.Has(s.Type)) errors.Add($"{p}/{s.Id}: unknown type '{s.Type}'");
                if (s.CycleS <= 0) errors.Add($"{p}/{s.Id}: cycleS must be > 0");
                if (s.Pos.Length != 2) errors.Add($"{p}/{s.Id}: pos must be [x, y]");
                if (s.MicroStop is { } ms && (ms.MtbsS <= 0 || ms.MeanS <= 0 || ms.MeanS > MicroStopModel.MaxS))
                    errors.Add($"{p}/{s.Id}: microStop needs mtbsS > 0 and 0 < meanS <= {MicroStopModel.MaxS}");
            }

            var stationIds = line.Stations.Select(s => s.Id).ToHashSet();
            foreach (var c in line.Conveyors)
            {
                if (!stationIds.Contains(c.From)) errors.Add($"{p}/{c.Id}: unknown from '{c.From}'");
                if (!stationIds.Contains(c.To)) errors.Add($"{p}/{c.Id}: unknown to '{c.To}'");
                if (c.LengthM <= 0 || c.PitchM <= 0 || c.SpeedMps <= 0) errors.Add($"{p}/{c.Id}: lengthM, pitchM, speedMps must be > 0");
            }
            foreach (var g in line.Conveyors.GroupBy(c => c.From).Where(g => g.Count() > 1)) errors.Add($"{p}: station '{g.Key}' has >1 output (branching not supported yet)");
            foreach (var g in line.Conveyors.GroupBy(c => c.To).Where(g => g.Count() > 1)) errors.Add($"{p}: station '{g.Key}' has >1 input (merging not supported yet)");
            if (line.Conveyors.Count != line.Stations.Count - 1) errors.Add($"{p}: a line must be one chain (stations - 1 conveyors)");
            if (!string.IsNullOrEmpty(line.OeeStation) && !stationIds.Contains(line.OeeStation)) errors.Add($"{p}: unknown oeeStation '{line.OeeStation}'");
        }
        return errors;
    }

    public static Hall Build(FactoryManifest m)
    {
        var errors = Validate(m);
        if (errors.Count > 0) throw new ManifestException("Invalid factory.json:\n- " + string.Join("\n- ", errors));

        return new Hall
        {
            Id = m.Hall.Id,
            Name = m.Hall.Name,
            Seed = m.Hall.Seed,
            Zones = m.Hall.Zones.Select(z => new Zone(z.Id, z.Name, z.Rect, z.Planned)).ToList(),
            Lines = m.Lines.Select(l => BuildLine(l, m.Hall.Seed)).ToList(),
        };
    }

    static Line BuildLine(LineSpec spec, int seed)
    {
        var stations = spec.Stations.ToDictionary(s => s.Id, s => CreateStation(spec, s, seed));
        var conveyors = spec.Conveyors.Select(c => CreateConveyor(spec, c)).ToList();
        foreach (var c in conveyors)
        {
            stations[c.FromId].Output = c;
            stations[c.ToId].Input = c;
        }

        var ordered = new List<Station>();
        var next = stations.Values.SingleOrDefault(s => s.Input == null)
                   ?? throw new ManifestException($"line {spec.Id}: no single source station");
        while (next != null)
        {
            ordered.Add(next);
            next = next.Output == null ? null : stations[next.Output.ToId];
        }
        if (ordered.Count != stations.Count) throw new ManifestException($"line {spec.Id}: stations not connected in one chain");

        var oee = string.IsNullOrEmpty(spec.OeeStation) ? ordered.MaxBy(s => s.CycleS)! : stations[spec.OeeStation];
        return new Line(spec.Id, spec.Name, ordered, conveyors, oee) { AutoStart = spec.AutoStart };
    }

    static Station CreateStation(LineSpec line, StationSpec s, int seed)
    {
        var st = StationRegistry.Create(s.Type);
        st.Id = s.Id;
        st.Name = string.IsNullOrEmpty(s.Name) ? s.Id : s.Name;
        st.TypeKey = s.Type;
        st.LineId = line.Id;
        st.CycleS = s.CycleS;
        st.Fault = ToFault(s.Fault);
        if (s.MicroStop != null)
            st.MicroStop = new MicroStopModel { MtbsS = s.MicroStop.MtbsS, MeanS = s.MicroStop.MeanS, Rng = new SimRandom(MicroStopModel.SeedFor(seed, st.Path)) };
        var prms = new Dictionary<string, double>(s.Params)
        {
            ["unitsPerCase"] = line.Product.UnitsPerCase,
            ["casesPerPallet"] = line.Product.CasesPerPallet,
        };
        st.Params = prms;
        return st;
    }

    static Conveyor CreateConveyor(LineSpec line, ConveyorSpec c) => new()
    {
        Id = c.Id,
        Name = string.IsNullOrEmpty(c.Name) ? $"Taśma {c.From}→{c.To}" : c.Name,
        TypeKey = "conveyor",
        LineId = line.Id,
        FromId = c.From,
        ToId = c.To,
        LengthM = c.LengthM,
        SpeedMps = c.SpeedMps,
        PitchM = c.PitchM,
        Fault = ToFault(c.Fault),
    };

    static FaultModel ToFault(FaultSpec? f) =>
        f == null ? FaultModel.None : new FaultModel { MtbfS = f.MtbfS, MttrS = f.MttrS, AutoRecover = f.AutoRecover };
}
