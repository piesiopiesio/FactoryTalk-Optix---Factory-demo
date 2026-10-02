// @summary: Applies labels from a roll; random misses mark the bottle defective; an empty roll triggers a timed roll change (Maintenance).
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("labeler")]
public sealed class Labeler : Station
{
    double stock = double.NaN, changeLeftS;

    [Signal(Unit = "%", Decimals = 1, Label = "Etykiety")] public double LabelStock => double.IsNaN(stock) ? 100 : 100 * stock / Param("rollLabels", 6000);

    protected override string[] FaultCatalog => new[] { "Zerwana taśma etykiet", "Błąd czujnika etykiety" };

    public override AlarmPriority AlarmPriority => AlarmPriority.Low;

    protected override MachineState? Hold(SimContext ctx)
    {
        if (changeLeftS <= 0) return null;
        changeLeftS -= ctx.Dt;
        if (changeLeftS > 0) return MachineState.Maintenance;
        stock = Param("rollLabels", 6000);
        ctx.Raise(Path, SimEventKind.MaintenanceEnded, 0, $"{Name}: nowa rolka etykiet");
        return null;
    }

    protected override Outcome Process(Item item, SimContext ctx)
    {
        var roll = Param("rollLabels", 6000);
        if (double.IsNaN(stock)) stock = roll;
        stock -= 1;
        if (stock <= 0)
        {
            stock = 0;
            changeLeftS = Param("rollChangeS", 45);
            ctx.Raise(Path, SimEventKind.MaintenanceStarted, 0, $"{Name}: wymiana rolki etykiet");
        }
        item.Labeled = true;
        if (ctx.Rng.Chance(Param("missRate", 0.004))) item.Defect = true;
        return Outcome.Pass(item);
    }
}
