// @summary: Samples all [Signal] values + belt positions every frame into a compact columnar trace.
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Sim.Cli;

public sealed class TraceRecorder
{
    readonly SimEngine engine;
    readonly List<(string Path, object Node, IReadOnlyList<SignalInfo> Signals)> nodes;

    public TraceRecorder(SimEngine engine)
    {
        this.engine = engine;
        nodes = engine.Nodes()
            .Select(n => (n.Path, n.Node, (IReadOnlyList<SignalInfo>)Signals.Of(n.Node.GetType()).Where(s => s.Type != typeof(string)).ToList()))
            .ToList();
    }

    public List<object> Frames { get; } = new();

    /// <summary>Signal schema: node path -> [type, [signal names], [units]].</summary>
    public Dictionary<string, object> Schema() => nodes.ToDictionary(
        n => n.Path,
        n => (object)new
        {
            type = n.Node is Equipment eqp ? eqp.TypeKey : n.Node is Line ? "line" : "hall",
            name = n.Node switch { Equipment eq => eq.Name, Line l => l.Name, Hall h => h.Name, _ => n.Path },
            signals = n.Signals.Select(s => s.Name).ToArray(),
            units = n.Signals.Select(s => s.Unit ?? "").ToArray(),
        });

    public void Capture()
    {
        var values = nodes.ToDictionary(n => n.Path, n => n.Signals.Select(s => s.Read(n.Node)).ToArray());
        var belts = engine.Hall.Lines.SelectMany(l => l.Conveyors).ToDictionary(c => c.Path, c => c.Positions().ToArray());
        Frames.Add(new { t = Math.Round(engine.Time, 1), v = values, belts });
    }
}
