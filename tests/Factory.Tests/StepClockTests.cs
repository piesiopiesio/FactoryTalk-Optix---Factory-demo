// @summary: Tests for StepClock: simulated time follows wall time despite late PeriodicTask calls; stalls are dropped.
using Factory.Core.Sim;

namespace Factory.Tests;

public static class StepClockTests
{
    [Test] public static void LateCallsStillTrackWallTime()
    {
        // PeriodicTask 100 ms + ~30 ms of work: 600 calls of 130 ms = 78 s of wall time.
        var c = new StepClock(0.1, 200);
        var steps = 0;
        for (var i = 0; i < 600; i++) steps += c.Steps(0.13, 1);
        Assert.True(System.Math.Abs(steps * 0.1 - 78.0) <= 0.1, $"simulated {steps * 0.1:0.0} s for 78 s of wall time");
        var oneStepPerCall = 600 * 0.1;
        Assert.True(oneStepPerCall < 61, "the old one-step-per-call loop would lag by 18 s");
    }

    [Test] public static void TimeScaleAndStallCap()
    {
        var c = new StepClock(0.1, 200);
        Assert.Equal(10, c.Steps(0.1, 10), "x10: ten steps per 100 ms");
        Assert.Equal(200, c.Steps(3600, 1), "PC slept an hour: capped, not replayed");
        Assert.Equal(1, c.Steps(0.1, 1), "backlog dropped after the cap");
        Assert.Equal(0, c.Steps(-1, 1), "clock going backwards = no step");
    }
}
