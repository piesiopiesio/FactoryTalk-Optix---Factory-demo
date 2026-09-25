// @summary: Fills bottles from a buffer tank; fill volume is noisy, out-of-tolerance bottles are marked defective.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("filler")]
public sealed class Filler : Station
{
    double tankL = double.NaN;

    [Signal(Unit = "ml", Decimals = 1)] public double FillMl { get; private set; }
    [Signal(Unit = "%", Decimals = 1)] public double TankLevel => double.IsNaN(tankL) ? 0 : 100 * tankL / Param("tankL", 800);

    protected override string[] FaultCatalog => new[] { "Zacięcie butelki w karuzeli", "Błąd zaworu nalewaka", "Niskie ciśnienie CO2" };

    protected override MachineState? Hold(SimContext ctx)
    {
        var capacity = Param("tankL", 800);
        if (double.IsNaN(tankL)) tankL = 0.9 * capacity;
        tankL = Math.Min(capacity, tankL + Param("inflowLps", 1.0) * ctx.Dt);   // TODO backlog #2: batch refill + LowTank alarm
        return tankL < Param("fillMl", 500) / 1000.0 ? MachineState.Starved : null;
    }

    protected override Outcome Process(Item item, SimContext ctx)
    {
        var target = Param("fillMl", 500);
        FillMl = ctx.Rng.Normal(target, Param("fillSigmaMl", 3));
        tankL -= FillMl / 1000.0;
        item.FillMl = FillMl;
        item.Filled = true;
        if (Math.Abs(FillMl - target) > Param("fillTolMl", 8)) item.Defect = true;
        return Outcome.Pass(item);
    }
}
