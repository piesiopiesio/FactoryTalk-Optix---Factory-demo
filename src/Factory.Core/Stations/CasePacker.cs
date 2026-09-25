// @summary: Collects bottles into cases (unitsPerCase) and emits one case item per full case.
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("casepacker")]
public sealed class CasePacker : Station
{
    [Signal] public int UnitsInCase { get; private set; }
    protected override int HeldUnits => UnitsInCase;

    protected override string[] FaultCatalog => new[] { "Brak kartonów", "Zacięcie klapy kartonu", "Błąd kleju" };

    protected override Outcome Process(Item item, SimContext ctx)
    {
        UnitsInCase++;
        if (UnitsInCase < (int)Param("unitsPerCase", 12)) return Outcome.Absorb;
        var box = new Item { Id = ctx.NewItemId(), Kind = ItemKind.Case, Units = UnitsInCase };
        UnitsInCase = 0;
        return Outcome.Pass(box);
    }
}
