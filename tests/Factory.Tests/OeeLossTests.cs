// @summary: Tests for the OEE loss Pareto: losses + OEE = 100 %, starved/blocked blamed on the stopped neighbour, rejects on the defect's origin.
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Tests;

public static class OeeLossTests
{
    [Test] public static void LossesAndOeeAddUpToHundred()
    {
        var e = Fixtures.Engine();
        e.Run(30 * 60);
        var line = e.Hall.Lines[0];
        var pareto = line.Losses.Pareto();
        Assert.InRange(line.Oee + pareto.Sum(line.Losses.Pp), 99.99, 100.01, "OEE + Σ losses [%]");
        Assert.True(pareto.Zip(pareto.Skip(1)).All(p => p.First.Id == OeeLosses.OtherId || p.Second.Id == OeeLosses.OtherId || p.First.TotalS >= p.Second.TotalS), "Pareto order");
        var other = pareto.SingleOrDefault(x => x.Id == OeeLosses.OtherId);
        Assert.InRange(other == null ? 0 : line.Losses.Pp(other), 0, 1, "unexplained remainder [pp]");
        var s = line.OeeStation;
        Assert.InRange(pareto.Single(x => x.Id == s.Id).AvailabilityS, s.TimeIn(MachineState.Faulted) + s.TimeIn(MachineState.Maintenance) - 0.01,
            s.TimeIn(MachineState.Faulted) + s.TimeIn(MachineState.Maintenance) + 0.01, "availability loss = own fault + maintenance time");
        Assert.True(pareto.Count(x => x.TotalS > 0 && x.Id != s.Id && x.Id != OeeLosses.OtherId) >= 3, "losses spread over other equipment");
    }

    [Test] public static void BlockedAndStarvedAreBlamedOnTheStoppedStation()
    {
        foreach (var (target, waiting) in new[] { ("T/C", MachineState.Blocked), ("T/A", MachineState.Starved) })
        {
            var e = new SimEngine(Factory.Core.Manifest.FactoryLoader.Build(Fixtures.Tiny()));
            var waited = 0.0;
            void Count(SimEngine x) { if (Fixtures.St(x, "T/B").TickState == waiting) waited += 0.1; }   // incl. start-up starving
            e.Run(60, afterTick: Count);
            Assert.True(e.Command(target, Cmd.InjectFault), "fault injected");
            e.Run(120, afterTick: Count);
            var blamed = e.Hall.Lines[0].Losses.Pareto().Single(x => x.Id == target[2..]);
            Assert.True(waited > 10, $"B waits while {target} is down ({waited:0} s)");
            Assert.InRange(blamed.PerformanceS, waited - 0.5, waited + 0.5, $"{waiting} time blamed on {target}");
        }
    }

    [Test] public static void RejectsAreBlamedOnTheStationThatMadeTheDefect()
    {
        var e = Fixtures.Engine();
        e.Run(30 * 60);
        var line = e.Hall.Lines[0];
        var quality = line.Losses.Pareto().Where(x => x.QualityS > 0).ToList();
        Assert.True(line.RejectUnits > 0, "rejects happen");
        Assert.True(quality.All(x => x.Id is "FILL" or "CAP" or "LAB"), "quality losses only on defect makers: " + string.Join(",", quality.Select(x => x.Id)));
        Assert.InRange(quality.Sum(x => x.QualityS), line.RejectUnits * line.OeeStation.CycleS - 0.01, line.RejectUnits * line.OeeStation.CycleS + 0.01, "one ideal cycle per reject");
    }
}
