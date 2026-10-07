// @summary: Short stops (< 30 s) while running: exponential gap on running time, short pause; a Performance loss, not a fault or alarm.
#nullable enable
namespace Factory.Core.Sim;

/// <summary>Minor stops (jam, missing cap, sensor glitch cleared by the machine itself). In OEE they are a
/// Performance loss: the station stays "running" (no alarm, no Availability loss) but makes no progress.</summary>
public sealed class MicroStopModel
{
    /// <summary>Upper limit of one stop; longer stoppages are faults (Availability loss).</summary>
    public const double MaxS = 29;

    /// <summary>Mean running time between stops (s). 0 = no micro stops.</summary>
    public double MtbsS { get; init; }
    /// <summary>Mean stop duration (s), drawn exponentially and clamped to [1, MaxS].</summary>
    public double MeanS { get; init; }

    /// <summary>Own random stream (seed from hall seed + device path), so adding micro stops
    /// does not reshuffle the fault draws of the shared stream.</summary>
    public SimRandom Rng { get; init; } = new(0);

    double untilStop = double.NaN;
    public double LeftS { get; private set; }
    public bool Active => LeftS > 0;
    public long Count { get; private set; }
    public double TotalS { get; private set; }

    public static MicroStopModel None => new();

    /// <summary>Deterministic per-device seed (string.GetHashCode is randomized per process, so not used).</summary>
    public static int SeedFor(int hallSeed, string path)
    {
        unchecked
        {
            var h = hallSeed * 397 + 17;
            foreach (var c in path) h = h * 31 + c;
            return h & 0x7FFFFFFF;
        }
    }

    /// <summary>Advance by productive running time; returns true when a new stop starts.</summary>
    public bool Accumulate(double runningDt)
    {
        if (MtbsS <= 0 || Active) return false;
        if (double.IsNaN(untilStop)) untilStop = Rng.Exponential(MtbsS);
        untilStop -= runningDt;
        if (untilStop > 0) return false;
        untilStop = double.NaN;
        LeftS = Math.Clamp(Rng.Exponential(MeanS), 1, MaxS);
        Count++;
        return true;
    }

    /// <summary>Consume stop time; returns true while the station is paused by a micro stop.</summary>
    public bool Pause(double dt)
    {
        if (!Active) return false;
        var used = Math.Min(dt, LeftS);
        LeftS -= used;
        TotalS += used;
        return true;
    }
}
