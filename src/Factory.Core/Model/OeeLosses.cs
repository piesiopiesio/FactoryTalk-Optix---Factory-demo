// @summary: OEE loss Pareto: every lost second at the OEE station blamed on the equipment that caused it (fault, starved/blocked root, micro stop, reject origin).
#nullable enable
namespace Factory.Core.Model;

/// <summary>Loss of one piece of equipment, split by OEE factor. Seconds of the OEE station's planned time.</summary>
public sealed record OeeLoss(string Id, string Name, double AvailabilityS, double PerformanceS, double QualityS)
{
    public double TotalS => AvailabilityS + PerformanceS + QualityS;
}

/// <summary>
/// Exact decomposition: planned = good × cycle + down + (run − processed × cycle) + rejects × cycle, so
/// OEE + Σ losses = 100 %. Starved walks upstream and Blocked walks downstream to the first piece of equipment
/// that is not itself waiting; a reject is blamed on the station that marked the defect.
/// Whatever is left (partial cycle at the end, ramp-up) is <see cref="OtherId"/>.
/// </summary>
public sealed class OeeLosses
{
    public const string OtherId = "INNE";
    readonly Line line;
    readonly Dictionary<string, double[]> lost = new();   // id -> [availability, performance, quality] seconds
    readonly Dictionary<string, Equipment> byId;

    public OeeLosses(Line line)
    {
        this.line = line;
        byId = line.Equipment.ToDictionary(e => e.Id);
        foreach (var st in line.Stations) st.OnReject = item => Add(item.DefectBy ?? st.Id, 2, line.OeeStation.CycleS);
    }

    public double PlannedS { get; private set; }

    public void Tick(double dt)
    {
        var s = line.OeeStation;
        switch (s.TickState)
        {
            case MachineState.Stopped: return;
            case MachineState.Faulted:
            case MachineState.Maintenance: Add(s.Id, 0, dt); break;
            case MachineState.Running when s.MicroStopActive: Add(s.Id, 1, dt); break;
            case MachineState.Starved: Add(RootUpstream(s).Id, 1, dt); break;
            case MachineState.Blocked: Add(RootDownstream(s).Id, 1, dt); break;
        }
        PlannedS += dt;
    }

    /// <summary>Losses sorted largest first (Pareto order) plus the unexplained remainder; empty before any planned time.</summary>
    public IReadOnlyList<OeeLoss> Pareto()
    {
        var list = lost.Select(kv => new OeeLoss(kv.Key, byId.TryGetValue(kv.Key, out var e) ? e.Name : kv.Key, kv.Value[0], kv.Value[1], kv.Value[2]))
                       .Where(l => l.TotalS > 0).OrderByDescending(l => l.TotalS).ThenBy(l => l.Id).ToList();
        var other = PlannedS * (1 - line.Oee / 100) - list.Sum(l => l.TotalS);
        if (other > 1e-6) list.Add(new OeeLoss(OtherId, "Inne (niepełny cykl, rozruch)", 0, other, 0));
        return list;
    }

    /// <summary>Share of planned time in percentage points of OEE.</summary>
    public double Pp(OeeLoss loss) => PlannedS <= 0 ? 0 : 100 * loss.TotalS / PlannedS;

    void Add(string id, int factor, double s)
    {
        if (!lost.TryGetValue(id, out var v)) lost[id] = v = new double[3];
        v[factor] += s;
        if (byId.TryGetValue(id, out var e)) e.OeeLossS += s;
    }

    /// <summary>A belt is a root cause only when it is not moving (a full belt is waiting, not the cause).</summary>
    static bool Down(Equipment e) => e.TickState is MachineState.Faulted or MachineState.Maintenance or MachineState.Stopped;

    Equipment RootUpstream(Station s)
    {
        for (var st = s; ;)
        {
            var c = st.Input;
            if (c == null) return st;
            if (Down(c)) return c;
            var up = line.Stations.FirstOrDefault(x => x.Output == c);
            if (up == null || up.TickState != MachineState.Starved) return (Equipment?)up ?? c;
            st = up;
        }
    }

    Equipment RootDownstream(Station s)
    {
        for (var st = s; ;)
        {
            var c = st.Output;
            if (c == null) return st;
            if (Down(c)) return c;
            var down = line.Stations.FirstOrDefault(x => x.Input == c);
            if (down == null || down.TickState != MachineState.Blocked) return (Equipment?)down ?? c;
            st = down;
        }
    }
}
