// @summary: Line sink: stacks cases on pallets; a full pallet triggers a timed pallet change (Maintenance).
#nullable enable
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Factory.Core.Stations;

[StationType("palletizer")]
public sealed class Palletizer : Station
{
    double changeLeftS;

    [Signal(Label = "Na palecie")] public int CasesOnPallet { get; private set; }
    [Signal(Label = "Palety")] public int Pallets { get; private set; }

    protected override string[] FaultCatalog => new[] { "Kurtyna świetlna naruszona", "Brak pustych palet" };

    protected override MachineState? Hold(SimContext ctx)
    {
        if (changeLeftS <= 0) return null;
        changeLeftS -= ctx.Dt;
        if (changeLeftS <= 0) ctx.Raise(Path, SimEventKind.MaintenanceEnded, 0, $"{Name}: nowa paleta");
        return MachineState.Maintenance;
    }

    protected override Outcome Process(Item item, SimContext ctx)
    {
        CasesOnPallet++;
        if (CasesOnPallet >= (int)Param("casesPerPallet", 40))
        {
            Pallets++;
            CasesOnPallet = 0;
            changeLeftS = Param("palletChangeS", 20);
            ctx.Raise(Path, SimEventKind.MaintenanceStarted, 0, $"{Name}: wymiana palety");
        }
        return Outcome.Pass(item);
    }
}
