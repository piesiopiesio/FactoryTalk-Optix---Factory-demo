// @summary: Tests for manifest validation, conveyor/flow behavior, determinism, KPIs, signals and palette.
using System.Text.Json;
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;
using Sim.Cli;

namespace Factory.Tests;

public static class ManifestTests
{
    [Test] public static void RealManifestIsValid() =>
        Assert.Equal(0, FactoryLoader.Validate(Fixtures.Manifest()).Count, "validation errors: " + string.Join("; ", FactoryLoader.Validate(Fixtures.Manifest())));

    [Test] public static void UnknownTypeIsReported()
    {
        var m = Fixtures.Tiny();
        m.Lines[0].Stations[1].Type = "teleporter";
        Assert.True(FactoryLoader.Validate(m).Any(e => e.Contains("teleporter")), "unknown type not reported");
    }

    [Test] public static void BrokenChainIsReported()
    {
        var m = Fixtures.Tiny();
        m.Lines[0].Conveyors.RemoveAt(1);
        Assert.True(FactoryLoader.Validate(m).Count > 0, "missing conveyor not reported");
        Assert.Throws<ManifestException>(() => FactoryLoader.Build(m), "Build on invalid manifest");
    }

    [Test] public static void RegistryKnowsAllManifestTypes()
    {
        foreach (var s in Fixtures.Manifest().Lines.SelectMany(l => l.Stations))
            Assert.True(StationRegistry.Has(s.Type), $"type '{s.Type}' not registered");
    }
}

public static class FlowTests
{
    [Test] public static void StoppedStationBlocksUpstreamAndFillsBelt()
    {
        var e = new SimEngine(FactoryLoader.Build(Fixtures.Tiny()));
        e.Command("T/B", Cmd.Stop);
        e.Run(60);
        var belt = e.Hall.Lines[0].Conveyors[0];
        Assert.Equal(belt.Capacity, belt.ItemsOnBelt, "items on full belt");
        Assert.Equal(MachineState.Blocked, Fixtures.St(e, "T/A").State, "feeder state");
    }

    [Test] public static void BeltPositionsAreHeadFirstAndPitched()
    {
        // The preview interpolates belt dots between frames assuming head-first order and pitch spacing.
        var e = Fixtures.Engine();
        for (var k = 0; k < 60; k++)
        {
            e.Run(10);
            foreach (var c in e.Hall.Lines.SelectMany(l => l.Conveyors))
            {
                var q = c.Positions().ToArray();
                var gap = c.PitchM / c.LengthM - 0.002;
                for (var i = 0; i < q.Length; i++)
                {
                    Assert.True(q[i] >= 0 && q[i] <= 1, $"{c.Path}: position {q[i]} outside 0..1");
                    if (i > 0) Assert.True(q[i - 1] - q[i] >= gap, $"{c.Path}: gap {q[i - 1] - q[i]:0.000} < pitch {gap:0.000}");
                }
            }
        }
    }

    [Test] public static void StoppedFeederStarvesDownstream()
    {
        var e = new SimEngine(FactoryLoader.Build(Fixtures.Tiny()));
        e.Command("T/A", Cmd.Stop);
        e.Run(30);
        Assert.Equal(MachineState.Starved, Fixtures.St(e, "T/B").State, "capper state");
    }

    [Test] public static void BottleBalanceHolds()
    {
        var e = Fixtures.Engine();
        e.Run(20 * 60);
        var (fed, accounted) = Checks.Balance(e.Hall.Lines[0]);
        Assert.Equal(fed, accounted, "fed vs good + reject + wip");
    }

    [Test] public static void SameSeedSameRun() => Assert.Equal(Fingerprint(7), Fingerprint(7), "fingerprint");

    [Test] public static void DifferentSeedDifferentRun() => Assert.True(Fingerprint(7) != Fingerprint(8), "seeds 7 and 8 gave identical runs");

    static string Fingerprint(int seed)
    {
        var e = Fixtures.Engine(seed);
        e.Run(10 * 60);
        return JsonSerializer.Serialize(e.Nodes().Select(n => Signals.Snapshot(n.Node))) + e.Events.Count;
    }
}

public static class KpiTests
{
    [Test] public static void OeeFactorsAreBoundedAndConsistent()
    {
        var e = Fixtures.Engine();
        e.Run(15 * 60);
        var l = e.Hall.Lines[0];
        foreach (var (v, n) in new[] { (l.Availability, "A"), (l.Performance, "P"), (l.Quality, "Q"), (l.Oee, "OEE") })
            Assert.InRange(v, 0, 100, n);
        Assert.InRange(l.Oee - l.Availability * l.Performance * l.Quality / 10000, -1e-9, 1e-9, "OEE = A*P*Q");
        Assert.True(l.ThroughputPerMin > 0, "throughput is zero");
    }

    [Test] public static void FaultIsRaisedAndCleared()
    {
        var e = Fixtures.Engine();
        e.Run(30 * 60);
        Assert.True(e.Events.Any(x => x.Kind == SimEventKind.FaultRaised), "no fault raised in 30 min");
        Assert.True(e.Events.Any(x => x.Kind == SimEventKind.FaultCleared), "no fault cleared in 30 min");
    }
}

public static class SignalTests
{
    [Test] public static void EveryStationTypeExposesCoreSignals()
    {
        foreach (var t in StationRegistry.Types.Values)
        {
            var names = Signals.Of(t).Select(s => s.Name).ToList();
            foreach (var must in new[] { "State", "FaultActive", "Good", "Reject", "CycleTimeS" })
                Assert.True(names.Contains(must), $"{t.Name} misses signal {must}");
            Assert.Equal(names.Count, names.Distinct().Count(), $"{t.Name} duplicate signal names");
        }
    }

    [Test] public static void PaletteMatchesDesignTheme()
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Fixtures.RepoRoot, "design", "theme.json")));
        var states = doc.RootElement.GetProperty("states");
        foreach (var s in Enum.GetValues<MachineState>())
            Assert.Equal(StatePalette.Hex(s), states.GetProperty(s.ToString()).GetString(), $"theme color for {s}");
    }
}

public static class AlarmTests
{
    [Test] public static void AlarmPrioritiesFollowIsa182()
    {
        var eq = Fixtures.Engine().Hall.Lines.SelectMany(l => l.Equipment).ToList();
        AlarmPriority P(string type) => eq.First(e => e.TypeKey == type).AlarmPriority;
        Assert.Equal(AlarmPriority.Urgent, P("palletizer"), "light curtain (safety) must be Urgent");
        Assert.True(eq.OfType<Conveyor>().All(c => c.AlarmPriority < P("labeler")), "belt drive fault must outrank missing labels");
        var sev = Enum.GetValues<AlarmPriority>().Select(AlarmPriorities.Severity).ToList();
        Assert.True(sev.Zip(sev.Skip(1)).All(p => p.First > p.Second) && sev.All(s => s is >= 1 and <= 1000), "Severity must fall 1->4 within 1..1000");
    }
}

public static class TrendTests
{
    [Test] public static void TrendPensKeepColumnsAndReadLikeOperatorText()
    {
        var pens = TrendPens.Of(Fixtures.Engine().Hall).ToList();
        var cols = pens.Select(p => p.Column).ToList();
        foreach (var must in new[] { "L1_oee", "L1_throughputPerMin", "L1_FILL_tankLevel", "L1_C1_occupancy" })
            Assert.True(cols.Contains(must), $"DB column {must} missing (renaming a column orphans logged history)");
        Assert.Equal(pens.Count, cols.Distinct().Count(), "duplicate DB columns");
        Assert.Equal(pens.Count, pens.Select(p => p.Label).Distinct().Count(), "duplicate pen names");
        Assert.Equal("L1 OEE [%]", pens.First(p => p.Column == "L1_oee").Label, "line OEE pen");
        Assert.Equal("L1 Napełniarka: Zbiornik [%]", pens.First(p => p.Column == "L1_FILL_tankLevel").Label, "filler tank pen");
        Assert.True(pens.All(p => !p.Label.Contains('_') && p.Label.Length <= 40), "pen names must be readable (no '_', <= 40 chars)");
    }

    [Test] public static void HallTileOpensItsLineTab()
    {
        var m = Fixtures.Manifest();
        var tabs = NavTabs.Of(m);
        Assert.Equal("Hala", tabs[0].Title, "hall is the first tab (CurrentTabIndex 0 at start)");
        Assert.Equal(1, NavTabs.IndexOf(m, "L1"), "L1 tab index");
        Assert.Equal(m.Lines[0].Name, tabs[NavTabs.IndexOf(m, "L1")].Title, "tile target shows the line's own screen");
        Assert.Equal(m.Lines.Count + 4, tabs.Count, "hall + lines + Alarmy/Trendy/Logowanie");
        Assert.Equal(tabs.Count, tabs.Select(t => t.Key).Distinct().Count(), "unique tab keys");
        Assert.Throws<ArgumentException>(() => NavTabs.IndexOf(m, "L9"), "unknown line has no tab");
    }
}

public static class MaintenanceTests
{
    [Test] public static void FillerTankRefillsInBatchesWithLowTankEvent()
    {
        var e = Fixtures.Engine();
        var fill = (Factory.Core.Stations.Filler)Fixtures.St(e, "L1/FILL");
        double minLevel = 100;
        for (int i = 0; i < 18000; i++)
        {
            e.Tick(0.1);
            minLevel = Math.Min(minLevel, fill.TankLevel);
        }
        var ev = e.Events.Where(x => x.NodeId == "L1/FILL").ToList();
        var low = ev.Count(x => x.Kind == SimEventKind.LowTank);
        Assert.True(low >= 1, "no LowTank event in 30 min");
        Assert.Equal(low, ev.Count(x => x.Kind == SimEventKind.MaintenanceStarted), "every LowTank starts one refill");
        Assert.True(ev.Count(x => x.Kind == SimEventKind.MaintenanceEnded) >= low - 1, "refills finish");
        Assert.True(fill.TimeIn(MachineState.Maintenance) > 20, "refill must show as Maintenance time (Availability loss)");
        Assert.True(minLevel >= 15 && minLevel < 20, $"tank never drains below the low mark (min {minLevel:0.0} %)");
    }
}

