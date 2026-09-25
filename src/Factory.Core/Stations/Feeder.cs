// @summary: Source station: creates empty bottles at its cycle rate (infinite supply).
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("feeder")]
public sealed class Feeder : Station
{
    protected override string[] FaultCatalog => new[] { "Przewrócona butelka", "Brak butelek w zasobniku" };

    protected override Item? Acquire(SimContext ctx) => new() { Id = ctx.NewItemId(), Kind = ItemKind.Bottle };

    protected override Outcome Process(Item item, SimContext ctx) => Outcome.Pass(item);
}
