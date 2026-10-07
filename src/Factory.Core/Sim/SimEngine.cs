// @summary: Runs the hall: fixed-step Tick(dt), commands by path ("L1", "L1/FILL"), event log, node enumeration.
#nullable enable
using Factory.Core.Model;

namespace Factory.Core.Sim;

public sealed class SimEngine
{
    readonly List<SimEvent> events = new();
    readonly SimContext ctx;
    int drained;

    public SimEngine(Hall hall, int? seed = null)
    {
        Hall = hall;
        ctx = new SimContext(new SimRandom(seed ?? hall.Seed), events);
        foreach (var line in hall.Lines.Where(l => l.AutoStart)) line.Command(Cmd.Start, ctx);
    }

    public Hall Hall { get; }
    public double Time { get; private set; }
    public IReadOnlyList<SimEvent> Events => events;

    public void Tick(double dt)
    {
        ctx.Dt = dt;
        ctx.Time = Time;
        foreach (var line in Hall.Lines) line.Tick(ctx);
        Time = Math.Round(Time + dt, 6);
    }

    /// <summary>Run for a span of simulated time; optional callback after each tick.</summary>
    public void Run(double seconds, double dt = 0.1, Action<SimEngine>? afterTick = null)
    {
        var steps = (int)Math.Round(seconds / dt);
        for (var i = 0; i < steps; i++) { Tick(dt); afterTick?.Invoke(this); }
    }

    /// <summary>Events raised since the previous call (for alarm bridges).</summary>
    public IReadOnlyList<SimEvent> DrainNewEvents()
    {
        var fresh = events.Skip(drained).ToList();
        drained = events.Count;
        return fresh;
    }

    /// <summary>Target: line id ("L1"), equipment path ("L1/FILL") or "Hall" (all lines).
    /// InjectFault only for a device; returns false for a line, the hall or an unknown path.</summary>
    public bool Command(string target, Cmd cmd)
    {
        ctx.Time = Time;
        if (cmd == Cmd.InjectFault && (target == Hall.Id || Hall.Lines.Any(l => l.Id == target))) return false;
        if (target == Hall.Id) { foreach (var l in Hall.Lines) l.Command(cmd, ctx); return true; }
        var line = Hall.Lines.FirstOrDefault(l => l.Id == target);
        if (line != null) { line.Command(cmd, ctx); return true; }
        var eq = Hall.Lines.SelectMany(l => l.Equipment).FirstOrDefault(e => e.Path == target || e.Id == target);
        eq?.Command(cmd, ctx);
        return eq != null;
    }

    /// <summary>Every signal source with its model path: "Hall", "L1", "L1/FILL", "L1/C1".</summary>
    public IEnumerable<(string Path, object Node)> Nodes()
    {
        yield return (Hall.Id, Hall);
        foreach (var line in Hall.Lines)
        {
            yield return (line.Id, line);
            foreach (var e in line.Equipment) yield return (e.Path, e);
        }
    }
}
