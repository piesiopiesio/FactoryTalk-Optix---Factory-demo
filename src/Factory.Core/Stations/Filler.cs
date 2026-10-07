// @summary: Fills bottles from a buffer tank refilled in batches (LowTank -> Maintenance); noisy fill volume marks defects.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("filler")]
public sealed class Filler : Station
{
    double tankL = double.NaN;

    [Signal(Unit = "ml", Decimals = 1, Label = "Napełnienie")] public double FillMl { get; private set; }
    [Signal(Unit = "%", Decimals = 1, Label = "Zbiornik")] public double TankLevel => double.IsNaN(tankL) ? 0 : 100 * tankL / Param("tankL", 800);

    protected override string[] FaultCatalog => new[] { "Zacięcie butelki w karuzeli", "Błąd zaworu nalewaka", "Niskie ciśnienie CO2" };

    public override AlarmPriority AlarmPriority => AlarmPriority.High;

    /// <summary>True while the buffer tank is being refilled (station in Maintenance).</summary>
    public bool Refilling { get; private set; }

    protected override MachineState? Hold(SimContext ctx)
    {
        var capacity = Param("tankL", 800);
        if (double.IsNaN(tankL)) tankL = 0.9 * capacity;
        if (Refilling)
        {
            tankL = Math.Min(capacity, tankL + Param("refillLps", 12) * ctx.Dt);
            if (tankL < Param("refillToPct", 95) / 100.0 * capacity) return MachineState.Maintenance;
            Refilling = false;
            ctx.Raise(Path, SimEventKind.MaintenanceEnded, 0, $"{Name}: zbiornik uzupełniony ({TankLevel:0} %)");
            return null;
        }
        if (tankL >= Param("lowPct", 20) / 100.0 * capacity) return null;
        Refilling = true;
        ctx.Raise(Path, SimEventKind.LowTank, 0, $"{Name}: niski poziom zbiornika ({TankLevel:0} %)");
        ctx.Raise(Path, SimEventKind.MaintenanceStarted, 0, $"{Name}: uzupełnianie zbiornika");
        return MachineState.Maintenance;
    }

    protected override Outcome Process(Item item, SimContext ctx)
    {
        var target = Param("fillMl", 500);
        FillMl = ctx.Rng.Normal(target, Param("fillSigmaMl", 3));
        tankL -= FillMl / 1000.0;
        item.FillMl = FillMl;
        item.Filled = true;
        if (Math.Abs(FillMl - target) > Param("fillTolMl", 8)) item.MarkDefect(Id);
        return Outcome.Pass(item);
    }
}
