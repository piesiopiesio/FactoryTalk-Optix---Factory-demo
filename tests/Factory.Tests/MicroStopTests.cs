// @summary: Tests for micro stops (< 30 s): no alarm/state change, Performance loss of 2-5 pp on the real line, validation.
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Tests;

public static class MicroStopTests
{
    [Test] public static void MicroStopPausesWithoutFaultOrEvent()
    {
        var m = Fixtures.Tiny();
        m.Lines[0].Stations[1].MicroStop = new MicroStopSpec { MtbsS = 20, MeanS = 4 };
        var e = new SimEngine(FactoryLoader.Build(m));
        var b = Fixtures.St(e, "T/B");
        var sawActive = false;
        e.Run(600, afterTick: _ =>
        {
            if (!b.MicroStopActive) return;
            sawActive = true;
            Assert.Equal(MachineState.Running, b.State, "state during a micro stop");
        });
        Assert.True(sawActive && b.MicroStops >= 10, $"micro stops happen (got {b.MicroStops})");
        Assert.InRange(b.MicroStopS / b.MicroStops, 1, MicroStopModel.MaxS, "mean stop length");
        Assert.Equal(0, e.Events.Count(x => x.NodeId == "T/B"), "no events/alarms for micro stops");
        Assert.Equal(0.0, b.TimeIn(MachineState.Faulted), "no fault time");
    }

    [Test] public static void MicroStopsCostTwoToFivePointsOfPerformance()
    {
        // Faults removed: with them, one long fault elsewhere moves Performance by ±15 pp between seeds and
        // hides the effect. Without faults the only difference between the two runs is the micro stops.
        double withMs = 0, without = 0;
        int[] seeds = { 42, 7, 1234 };
        foreach (var seed in seeds)
        {
            withMs += Performance(seed, keepMicroStops: true);
            without += Performance(seed, keepMicroStops: false);
        }
        var drop = (without - withMs) / seeds.Length;
        Assert.InRange(drop, 2, 5, "Performance drop [pp]");
    }

    [Test] public static void MicroStopSpecIsValidated()
    {
        var m = Fixtures.Tiny();
        m.Lines[0].Stations[1].MicroStop = new MicroStopSpec { MtbsS = 60, MeanS = 45 };
        Assert.True(FactoryLoader.Validate(m).Any(x => x.Contains("microStop")), "meanS >= 30 s is a fault, not a micro stop");
    }

    static double Performance(int seed, bool keepMicroStops)
    {
        var m = Fixtures.Manifest();
        m.Hall.Seed = seed;
        foreach (var l in m.Lines) { l.Stations.ForEach(s => s.Fault = null); l.Conveyors.ForEach(c => c.Fault = null); }
        if (!keepMicroStops) foreach (var s in m.Lines.SelectMany(l => l.Stations)) s.MicroStop = null;
        var e = new SimEngine(FactoryLoader.Build(m), seed);
        e.Run(30 * 60);
        return e.Hall.Lines[0].Performance;
    }
}
