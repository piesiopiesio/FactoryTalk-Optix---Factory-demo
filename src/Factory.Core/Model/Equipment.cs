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
    [Signal(Unit = "%", Decimals = 0)] public double SpeedPct { get; set; } = 100;

    public double TimeIn(MachineState s) => timeInState[(int)s];
    public double Param(string name, double fallback) => Params.TryGetValue(name, out var v) ? v : fallback;

    /// <summary>Fault catalog of this device type; code = index + 1.</summary>
    protected virtual string[] FaultCatalog => new[] { "Awaria napędu", "Czujnik nie odpowiada" };
    public string Describe(int code) => code >= 1 && code <= FaultCatalog.Length ? FaultCatalog[code - 1] : $"Kod {code}";

    public void Tick(SimContext ctx)
    {
        if (Fault.Active && Fault.Repair(ctx.Dt)) ClearFault(ctx);

        State = Fault.Active ? MachineState.Faulted
              : !Commanded ? MachineState.Stopped
              : Step(ctx);

        timeInState[(int)State] += ctx.Dt;

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
        }
    }

    void ClearFault(SimContext ctx)
    {
        ctx.Raise(Path, SimEventKind.FaultCleared, FaultCode, FaultText);
        FaultCode = 0;
    }
}
