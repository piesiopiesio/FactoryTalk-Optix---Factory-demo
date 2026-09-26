// @summary: Applies labels from a roll; random misses mark the bottle defective; roll stock is consumed.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("labeler")]
public sealed class Labeler : Station
{
    double stock = double.NaN;

    [Signal(Unit = "%", Decimals = 1, Label = "Etykiety")] public double LabelStock => double.IsNaN(stock) ? 100 : 100 * stock / Param("rollLabels", 6000);

    protected override string[] FaultCatalog => new[] { "Zerwana taśma etykiet", "Błąd czujnika etykiety" };

    public override AlarmPriority AlarmPriority => AlarmPriority.Low;

    protected override Outcome Process(Item item, SimContext ctx)
    {
        var roll = Param("rollLabels", 6000);
        if (double.IsNaN(stock)) stock = roll;
        stock -= 1;
        if (stock <= 0) stock = roll;   // TODO backlog #3: roll change with Maintenance state
        item.Labeled = true;
        if (ctx.Rng.Chance(Param("missRate", 0.004))) item.Defect = true;
        return Outcome.Pass(item);
    }
}
