// @summary: Test helpers: repo paths, loading the real factory.json, building small ad-hoc lines.
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Tests;

public static class Fixtures
{
    public static string RepoRoot
    {
        get
        {
            var d = new DirectoryInfo(AppContext.BaseDirectory);
            while (d != null && !File.Exists(Path.Combine(d.FullName, "factory.json"))) d = d.Parent;
            return d?.FullName ?? throw new Exception("factory.json not found above " + AppContext.BaseDirectory);
        }
    }

    public static FactoryManifest Manifest() => FactoryLoader.Parse(File.ReadAllText(Path.Combine(RepoRoot, "factory.json")));

    public static SimEngine Engine(int? seed = null) => new(FactoryLoader.Build(Manifest()), seed);

    /// <summary>Tiny 3-station line without faults: feeder -> filler -> palletizer-like sink (vision).</summary>
    public static FactoryManifest Tiny() => new()
    {
        Hall = new HallSpec { Zones = [new ZoneSpec { Id = "Z" }] },
        Lines =
        [
            new LineSpec
            {
                Id = "T", Zone = "Z", OeeStation = "B",
                Stations =
                [
                    new StationSpec { Id = "A", Type = "feeder", CycleS = 0.5 },
                    new StationSpec { Id = "B", Type = "capper", CycleS = 0.6, Params = new() { ["torqueSigma"] = 0 } },
                    new StationSpec { Id = "C", Type = "casepacker", CycleS = 0.4 },
                ],
                Conveyors =
                [
                    new ConveyorSpec { Id = "C1", From = "A", To = "B", LengthM = 1, PitchM = 0.1, SpeedMps = 0.5 },
                    new ConveyorSpec { Id = "C2", From = "B", To = "C", LengthM = 1, PitchM = 0.1, SpeedMps = 0.5 },
                ],
            },
        ],
    };

    public static Station St(SimEngine e, string path) => (Station)e.Hall.Lines.SelectMany(l => l.Equipment).Single(x => x.Path == path);
}
