// @summary: Per-tick context passed to every node: dt, clock, RNG, event sink, id generator.
#nullable enable
namespace Factory.Core.Sim;

public sealed class SimContext
{
    readonly List<SimEvent> events;
    long nextItemId;

    public SimContext(SimRandom rng, List<SimEvent> events) { Rng = rng; this.events = events; }

    public SimRandom Rng { get; }
    public double Dt { get; internal set; }
    public double Time { get; internal set; }

    public long NewItemId() => ++nextItemId;

    public void Raise(string nodeId, SimEventKind kind, int code, string text) =>
        events.Add(new SimEvent(Math.Round(Time, 1), nodeId, kind, code, text));
}
