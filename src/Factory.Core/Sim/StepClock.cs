// @summary: Wall-clock pacing for the fixed-step engine: steps per call follow measured elapsed time (PeriodicTask period + run time).
#nullable enable
namespace Factory.Core.Sim;

/// <summary>Optix PeriodicTask runs every "period + execution time" (help: 1000 ms + 500 ms work = 1500 ms), so counting one
/// fixed step per call makes simulated time lag the clock. The caller passes the measured elapsed time; the clock returns how
/// many fixed <see cref="Dt"/> steps are due and carries the remainder, so the engine stays deterministic (always Dt) while
/// simulated time tracks wall time × time scale. After a stall (debugger, PC sleep) the backlog is dropped, not replayed.</summary>
public sealed class StepClock
{
    public double Dt { get; }
    public int MaxStepsPerCall { get; }
    double debtS;

    public StepClock(double dt, int maxStepsPerCall)
    {
        if (dt <= 0) throw new System.ArgumentOutOfRangeException(nameof(dt));
        Dt = dt;
        MaxStepsPerCall = System.Math.Max(1, maxStepsPerCall);
    }

    /// <summary>Fixed steps due for <paramref name="elapsedS"/> seconds of wall time at <paramref name="timeScale"/> (≥ 1).</summary>
    public int Steps(double elapsedS, int timeScale)
    {
        if (!(elapsedS > 0)) return 0;                       // NaN, 0 or a clock going backwards
        debtS += elapsedS * System.Math.Max(1, timeScale);
        var n = (int)System.Math.Floor(debtS / Dt + 1e-9);
        if (n > MaxStepsPerCall) { debtS = 0; return MaxStepsPerCall; }
        debtS -= n * Dt;
        return n;
    }
}
