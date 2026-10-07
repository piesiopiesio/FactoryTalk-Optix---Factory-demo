// @summary: Random failures: exponential MTBF on running time, MTTR repair, optional auto-recover.
#nullable enable
namespace Factory.Core.Sim;

public sealed class FaultModel
{
    public double MtbfS { get; init; }
    public double MttrS { get; init; }
    public bool AutoRecover { get; init; } = true;

    double untilFailure = double.NaN;
    public double RepairLeftS { get; private set; }
    public bool Active { get; private set; }

    public static FaultModel None => new() { MtbfS = 0, MttrS = 0 };

    /// <summary>Advance by running time; returns true when a new failure starts.</summary>
    public bool Accumulate(double runningDt, SimRandom rng)
    {
        if (MtbfS <= 0 || Active) return false;
        if (double.IsNaN(untilFailure)) untilFailure = rng.Exponential(MtbfS);
        untilFailure -= runningDt;
        if (untilFailure > 0) return false;
        Active = true;
        RepairLeftS = Math.Max(1, rng.Exponential(MttrS));
        untilFailure = double.NaN;
        return true;
    }

    /// <summary>Advance repair; returns true when the fault clears on its own.</summary>
    public bool Repair(double dt)
    {
        if (!Active) return false;
        RepairLeftS = Math.Max(0, RepairLeftS - dt);
        if (RepairLeftS > 0 || !AutoRecover) return false;
        Active = false;
        return true;
    }

    public void Force(double repairS) { Active = true; RepairLeftS = repairS; }

    /// <summary>Operator reset: clears only when the repair time has elapsed.</summary>
    public bool Reset()
    {
        if (!Active || RepairLeftS > 0) return false;
        Active = false;
        return true;
    }
}
