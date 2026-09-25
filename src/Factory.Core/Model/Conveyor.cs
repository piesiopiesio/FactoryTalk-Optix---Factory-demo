// @summary: Accumulating belt: items keep pitch spacing, travel length/speed, block the upstream station when full.
#nullable enable
using Factory.Core.Sim;

namespace Factory.Core.Model;

public sealed class Conveyor : Equipment
{
    sealed class Slot { public Slot(Item item) { Item = item; } public Item Item { get; } public double Pos; }
    readonly List<Slot> belt = new();

    public double LengthM { get; internal set; } = 2;
    public double PitchM { get; internal set; } = 0.1;
    public string FromId { get; internal set; } = "";
    public string ToId { get; internal set; } = "";
    public int Capacity => Math.Max(1, (int)Math.Floor(LengthM / PitchM));

    [Signal(Unit = "m/s")] public double SpeedMps { get; internal set; } = 0.4;
    [Signal] public int ItemsOnBelt => belt.Count;
    [Signal(Unit = "%", Decimals = 0)] public double Occupancy => 100.0 * belt.Count / Capacity;

    protected override string[] FaultCatalog => new[] { "Zacięcie na taśmie", "Przeciążenie silnika" };

    public bool CanAccept => Commanded && !Fault.Active && belt.Count < Capacity && (belt.Count == 0 || belt[^1].Pos >= PitchM);

    public bool TryPut(Item item)
    {
        if (!CanAccept) return false;
        belt.Add(new Slot(item));
        return true;
    }

    public Item? TryTake()
    {
        if (belt.Count == 0 || belt[0].Pos < LengthM - 1e-9) return null;
        var item = belt[0].Item;
        belt.RemoveAt(0);
        return item;
    }

    /// <summary>Item positions 0..1 along the belt (head first) for the preview animation.</summary>
    public IEnumerable<double> Positions() => belt.Select(s => Math.Round(s.Pos / LengthM, 3));

    public IEnumerable<Item> Items => belt.Select(s => s.Item);

    protected override MachineState Step(SimContext ctx)
    {
        var travel = SpeedMps * SpeedPct / 100.0 * ctx.Dt;
        var limit = LengthM;
        foreach (var slot in belt)
        {
            slot.Pos = Math.Min(slot.Pos + travel, limit);
            limit = slot.Pos - PitchM;
        }
        var full = belt.Count >= Capacity && belt[0].Pos >= LengthM - 1e-9;
        return full ? MachineState.Blocked : MachineState.Running;
    }
}
