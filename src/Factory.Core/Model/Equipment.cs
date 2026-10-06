// @summary: Base of every simulated device: identity, commands, fault model, state machine hook, time-in-state.
#nullable enable
using Factory.Core.Sim;

namespace Factory.Core.Model;

public abstract class Equipment
{
    static readonly IReadOnlyDictionary<string, double> NoParams = new Dictionary<string, double>();
    readonly double[] timeInState = new double[Enum.GetValues<MachineState>().Length];

    public string Id { get; internal set; } = "";
    public string Name { get; internal set; } = "";
    public string TypeKey { get; internal set; } = "";
    public string LineId { get; internal set; } = "";
    public string Path => $"{LineId}/{Id}";
    public IReadOnlyDictionary<string, double> Params { get; internal set; } = NoParams;
    public FaultModel Fault { get; internal set; } = FaultModel.None;
    public bool Commanded { get; private set; }

    [Signal] public MachineState State { get; private set; } = MachineState.Stopped;
    [Signal] public string StateText => StatePalette.Label(State);
    [Signal] public bool FaultActive => State == MachineState.Faulted;
    [Signal] public int FaultCode { get; private set; }
    [Signal] public string FaultText => FaultCode == 0 ? "" : $"{Name}: {Describe(FaultCode)}";
    /// <summary>Seconds of the line's OEE lost because of this equipment (set by <see cref="OeeLosses"/>).</summary>
    [Signal(Unit = "s", Decimals = 0, Label = "Strata OEE")] public double OeeLossS { get; internal set; }
    [Signal(Unit = "%", Decimals = 0)] public double SpeedPct { get; set; } = 100;

    /// <summary>State this tick was accounted in (a fault raised at the end of the tick shows from the next one).</summary>
    public MachineState TickState { get; private set; } = MachineState.Stopped;
    public double TimeIn(MachineState s) => timeInState[(int)s];
    public double Param(string name, double fallback) => Params.TryGetValue(name, out var v) ? v : fallback;

    /// <summary>Fault catalog of this device type; code = index + 1.</summary>
    protected virtual string[] FaultCatalog => new[] { "Awaria napędu", "Czujnik nie odpowiada" };
    /// <summary>Priority of this device's fault alarm (one alarm per device); drives Optix Severity.</summary>
    public virtual AlarmPriority AlarmPriority => AlarmPriority.Medium;
    public string Describe(int code) => code >= 1 && code <= FaultCatalog.Length ? FaultCatalog[code - 1] : $"Kod {code}";

    public void Tick(SimContext ctx)
    {
        if (Fault.Active && Fault.Repair(ctx.Dt)) ClearFault(ctx);

        State = Fault.Active ? MachineState.Faulted
              : !Commanded ? MachineState.Stopped
              : Step(ctx);

        timeInState[(int)State] += ctx.Dt;
        TickState = State;

        if (State == MachineState.Running && Fault.Accumulate(ctx.Dt, ctx.Rng))
        {
            FaultCode = 1 + (int)(ctx.Rng.Uniform() * FaultCatalog.Length);
            State = MachineState.Faulted;
            ctx.Raise(Path, SimEventKind.FaultRaised, FaultCode, FaultText);
        }
    }

    /// <summary>One tick of normal operation (commanded, not faulted). Returns the resulting state.</summary>
    protected abstract MachineState Step(SimContext ctx);

    public void Command(Cmd cmd, SimContext ctx)
    {
        switch (cmd)
        {
            case Cmd.Start: Commanded = true; break;
            case Cmd.Stop: Commanded = false; break;
            case Cmd.Reset: if (Fault.Reset()) ClearFault(ctx); break;
            case Cmd.InjectFault: InjectFault(ctx); break;
        }
    }

    /// <summary>Repair time of an injected fault when the device has no MTTR (fault-free devices).</summary>
    public const double InjectedRepairS = 30;

    /// <summary>Operator/test fault: first catalog entry, repair = mean MTTR (no RNG draw, so the run stays deterministic).
    /// No-op while already faulted.</summary>
    void InjectFault(SimContext ctx)
    {
        if (Fault.Active) return;
        if (Fault.MttrS <= 0) Fault = new FaultModel { MtbfS = Fault.MtbfS, MttrS = InjectedRepairS, AutoRecover = Fault.AutoRecover };
        Fault.Force(Fault.MttrS);
        FaultCode = 1;
        State = MachineState.Faulted;
        ctx.Raise(Path, SimEventKind.FaultRaised, FaultCode, FaultText + " (wstrzyknięta)");
    }

    void ClearFault(SimContext ctx)
    {
        ctx.Raise(Path, SimEventKind.FaultCleared, FaultCode, FaultText);
        FaultCode = 0;
    }
}
