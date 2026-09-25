// @summary: Screws caps; torque outside tolerance marks the bottle defective.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("capper")]
public sealed class Capper : Station
{
    [Signal(Unit = "Nm", Label = "Moment")] public double Torque { get; private set; }

    protected override string[] FaultCatalog => new[] { "Brak nakrętek", "Przeciążenie głowicy" };

    protected override Outcome Process(Item item, SimContext ctx)
    {
        var target = Param("torqueNm", 2.2);
        Torque = ctx.Rng.Normal(target, Param("torqueSigma", 0.08));
        item.Capped = true;
        if (Math.Abs(Torque - target) > Param("torqueTol", 0.3)) item.Defect = true;
        return Outcome.Pass(item);
    }
}
