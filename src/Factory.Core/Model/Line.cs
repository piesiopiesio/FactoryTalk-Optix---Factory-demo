// @summary: Production line: ordered stations + conveyors, line commands, aggregate state and OEE/throughput KPIs.
#nullable enable
using Factory.Core.Sim;

namespace Factory.Core.Model;

public sealed class Line
{
    readonly Queue<(double t, long units)> window = new();
    long deliveredUnits;

    public Line(string id, string name, IReadOnlyList<Station> stations, IReadOnlyList<Conveyor> conveyors, Station oeeStation)
    {
        Id = id; Name = name; Stations = stations; Conveyors = conveyors; OeeStation = oeeStation;
        Sink = stations[^1];
        Sink.Delivered = item => deliveredUnits += item.Units;
        Losses = new OeeLosses(this);
    }

    public string Id { get; }
    public string Name { get; }
    public bool AutoStart { get; init; }
    public IReadOnlyList<Station> Stations { get; }
    public IReadOnlyList<Conveyor> Conveyors { get; }
    public Station OeeStation { get; }
    public Station Sink { get; }
    /// <summary>Where the OEE went: lost time per equipment (Pareto).</summary>
    public OeeLosses Losses { get; }
    public IEnumerable<Equipment> Equipment => Stations.Cast<Equipment>().Concat(Conveyors);

    [Signal] public MachineState State { get; private set; }
    [Signal] public string StateText => StatePalette.Label(State);
    [Signal(Unit = "%", Decimals = 1, Label = "OEE")] public double Oee => Availability * Performance * Quality / 10000.0;
    [Signal(Unit = "%", Decimals = 1, Label = "Dostępność")] public double Availability { get; private set; }
    [Signal(Unit = "%", Decimals = 1, Label = "Wydajność")] public double Performance { get; private set; }
    [Signal(Unit = "%", Decimals = 1, Label = "Jakość")] public double Quality { get; private set; }
    [Signal(Unit = "/min", Decimals = 1, Label = "Przepustowość")] public double ThroughputPerMin { get; private set; }
    [Signal] public long GoodUnits => deliveredUnits;
    [Signal] public long RejectUnits => Stations.Sum(s => s.Reject);
    [Signal] public int ActiveFaults => Equipment.Count(e => e.FaultActive);

    public void Command(Cmd cmd, SimContext ctx)
    {
        if (cmd == Cmd.InjectFault) return;   // device-level only: a whole-line injection would fault every station at once
        foreach (var e in Equipment) e.Command(cmd, ctx);
    }

    public void Tick(SimContext ctx)
    {
        foreach (var c in Conveyors) c.Tick(ctx);
        for (var i = Stations.Count - 1; i >= 0; i--) Stations[i].Tick(ctx);   // downstream first frees space
        UpdateKpis(ctx.Time + ctx.Dt);
        Losses.Tick(ctx.Dt);
    }

    void UpdateKpis(double now)
    {
        var s = OeeStation;
        var planned = Enum.GetValues<MachineState>().Where(x => x != MachineState.Stopped).Sum(s.TimeIn);
        var down = s.TimeIn(MachineState.Faulted) + s.TimeIn(MachineState.Maintenance);
        var run = planned - down;
        Availability = planned <= 0 ? 0 : 100 * run / planned;
        Performance = run <= 0 ? 0 : Math.Min(100, 100 * s.Processed * s.CycleS / run);
        Quality = s.Processed == 0 ? 100 : Math.Clamp(100 * (1 - (double)RejectUnits / s.Processed), 0, 100);

        window.Enqueue((now, deliveredUnits));
        while (window.Count > 1 && now - window.Peek().t > 60) window.Dequeue();
        var (t0, u0) = window.Peek();
        ThroughputPerMin = now - t0 < 1 ? 0 : (deliveredUnits - u0) * 60.0 / (now - t0);

        State = Equipment.Any(e => e.State == MachineState.Faulted) ? MachineState.Faulted
              : Equipment.All(e => e.State == MachineState.Stopped) ? MachineState.Stopped
              : s.State;
    }
}
