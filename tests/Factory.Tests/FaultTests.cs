// @summary: Tests for injected (operator/test) faults: device-only, event pair, repair time, no effect on the RNG stream.
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Tests;

public static class FaultTests
{
    [Test] public static void InjectedFaultStopsDeviceAndClearsAfterRepair()
    {
        var e = new SimEngine(FactoryLoader.Build(Fixtures.Tiny()));   // Tiny has no fault model (MTTR 0)
        e.Run(5);
        Assert.True(e.Command("T/B", Cmd.InjectFault), "command accepted");
        var b = Fixtures.St(e, "T/B");
        Assert.Equal(MachineState.Faulted, b.State, "state right after injection");
        Assert.Equal(1, b.FaultCode, "fault code");
        e.Run(10);
        Assert.Equal(MachineState.Faulted, b.State, "still faulted during repair");
        Assert.Equal(MachineState.Blocked, Fixtures.St(e, "T/A").State, "feeder blocked behind the fault");
        e.Command("T/B", Cmd.InjectFault);                              // second press while faulted = no-op
        e.Run(Equipment.InjectedRepairS);
        Assert.True(b.State != MachineState.Faulted, "fault cleared after repair time");
        var ev = e.Events.Where(x => x.NodeId == "T/B").ToList();
        Assert.Equal(2, ev.Count, "one raised + one cleared event");
        Assert.True(ev[0].Kind == SimEventKind.FaultRaised && ev[0].Text.Contains("wstrzyknięta"), "raised event marked as injected");
        Assert.Equal(SimEventKind.FaultCleared, ev[1].Kind, "cleared event");
    }

    [Test] public static void InjectionIsDeviceOnly()
    {
        var e = Fixtures.Engine();
        Assert.True(!e.Command("L1", Cmd.InjectFault), "line target refused");
        Assert.True(!e.Command(e.Hall.Id, Cmd.InjectFault), "hall target refused");
        Assert.True(!e.Command("L1/NOPE", Cmd.InjectFault), "unknown path refused");
        Assert.Equal(0, e.Hall.Lines[0].ActiveFaults, "no device faulted");
    }

    [Test] public static void InjectionKeepsRandomStreamDeterministic()
    {
        // Same seed + same injection at the same time = same run (no RNG draw inside InjectFault).
        long Good()
        {
            var e = Fixtures.Engine();
            e.Run(120);
            e.Command("L1/LAB", Cmd.InjectFault);
            e.Run(300);
            return e.Hall.Lines[0].GoodUnits;
        }
        Assert.Equal(Good(), Good(), "good units of two identical runs");
    }
}
