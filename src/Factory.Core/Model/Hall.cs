// @summary: Root of the factory: hall metadata, zones (layout only) and production lines.
#nullable enable
namespace Factory.Core.Model;

public sealed record Zone(string Id, string Name, int[] Rect, bool Planned);

public sealed class Hall
{
    public string Id { get; init; } = "Hall";
    public string Name { get; init; } = "";
    public int Seed { get; init; } = 1;
    public IReadOnlyList<Zone> Zones { get; init; } = Array.Empty<Zone>();
    public IReadOnlyList<Line> Lines { get; init; } = Array.Empty<Line>();

    [Signal] public int ActiveLines => Lines.Count(l => l.State != MachineState.Stopped);
    [Signal(Unit = "%", Decimals = 1)] public double AverageOee => Lines.Count == 0 ? 0 : Lines.Average(l => l.Oee);
}
