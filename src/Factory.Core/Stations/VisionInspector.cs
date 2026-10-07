// @summary: Inspects every bottle; anything not filled, capped, labeled and defect-free is rejected.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("vision")]
public sealed class VisionInspector : Station
{
    [Signal(Unit = "%", Decimals = 2, Label = "Odrzuty")] public double RejectRate => Processed == 0 ? 0 : 100.0 * Reject / Processed;

    protected override string[] FaultCatalog => new[] { "Kamera nie odpowiada", "Brudny obiektyw" };

    public override AlarmPriority AlarmPriority => AlarmPriority.Medium;

    protected override Outcome Process(Item item, SimContext ctx) =>
        item.IsGoodBottle ? Outcome.Pass(item) : Outcome.Reject;
}
