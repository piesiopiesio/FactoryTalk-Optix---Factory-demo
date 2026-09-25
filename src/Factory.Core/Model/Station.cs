// @summary: Template-method work cycle for stations: acquire -> process (subclass) -> emit; tracks starved/blocked.
#nullable enable
using Factory.Core.Sim;

namespace Factory.Core.Model;

/// <summary>Result of processing one unit. Out = item to pass downstream (may be a new item).</summary>
public readonly record struct Outcome(Item? Out, bool Rejected)
{
    public static Outcome Pass(Item item) => new(item, false);
    public static Outcome Reject => new(null, true);
    public static Outcome Absorb => new(null, false);   // unit consumed into a batch (case, pallet)
}

public abstract class Station : Equipment
{
    Item? current;
    Item? outgoing;
    double progress;

    public Conveyor? Input { get; internal set; }
    public Conveyor? Output { get; internal set; }
    public double CycleS { get; internal set; } = 1;
    /// <summary>Called for every item leaving the line at a sink station (Output == null).</summary>
    public Action<Item>? Delivered { get; set; }

    [Signal] public long Processed { get; private set; }
    [Signal] public long Good { get; private set; }
    [Signal] public long Reject { get; private set; }
    [Signal(Unit = "s")] public double CycleTimeS => CycleS / Math.Max(0.01, SpeedPct / 100.0);
    [Signal(Unit = "%", Decimals = 0)] public double Progress => progress * 100;

    /// <summary>Bottles held inside the station (in process, waiting to leave, or collected into a batch).</summary>
    public int WipUnits => (current?.Units ?? 0) + (outgoing?.Units ?? 0) + HeldUnits;
    protected virtual int HeldUnits => 0;

    protected override MachineState Step(SimContext ctx)
    {
        var hold = Hold(ctx);
        if (hold.HasValue) return hold.Value;

        if (outgoing != null)
        {
            if (!TryEmit(outgoing)) return MachineState.Blocked;
            outgoing = null;
        }

        if (current == null)
        {
            current = Acquire(ctx);
            if (current == null) return MachineState.Starved;
            progress = 0;
        }

        progress += ctx.Dt / CycleTimeS;
        if (progress < 1) return MachineState.Running;

        var outcome = Process(current, ctx);
        Processed++;
        current = null;
        progress = 0;
        if (outcome.Rejected) Reject++;
        if (outcome.Out != null && !TryEmit(outcome.Out)) outgoing = outcome.Out;
        return MachineState.Running;
    }

    /// <summary>Take the next unit. Default: from the input conveyor. Sources override.</summary>
    protected virtual Item? Acquire(SimContext ctx) => Input?.TryTake();

    /// <summary>Optional pause before taking work (e.g. pallet change). Null = no hold.</summary>
    protected virtual MachineState? Hold(SimContext ctx) => null;

    /// <summary>Station-specific work on one unit. The only method most stations implement.</summary>
    protected abstract Outcome Process(Item item, SimContext ctx);

    bool TryEmit(Item item)
    {
        if (Output == null) { Delivered?.Invoke(item); Good++; return true; }
        if (!Output.TryPut(item)) return false;
        Good++;
        return true;
    }
}
