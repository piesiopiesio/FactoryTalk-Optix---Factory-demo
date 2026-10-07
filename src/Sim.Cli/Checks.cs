// @summary: Behavior checks on a finished run: KPI targets, faults present, no dead station, bottle conservation.
using Factory.Core.Manifest;
using Factory.Core.Model;
using Factory.Core.Sim;

namespace Sim.Cli;

public sealed record Check(string Name, bool Ok, string Detail);

public static class Checks
{
    public static List<Check> Run(SimEngine engine, FactoryManifest manifest)
    {
        var list = new List<Check>();
        var lint = Layout.Lint(manifest);
        list.Add(new("layout lint", lint.Count == 0, lint.Count == 0 ? "ok" : string.Join("; ", lint)));
        foreach (var line in engine.Hall.Lines)
        {
            var spec = manifest.Lines.Single(l => l.Id == line.Id);
            var oee = line.Oee / 100;
            list.Add(new($"{line.Id}: OEE in target", InRange(oee, spec.KpiTargets.Oee), $"{oee:P1} vs [{spec.KpiTargets.Oee[0]:P0}, {spec.KpiTargets.Oee[1]:P0}]"));
            var thr = line.GoodUnits / Math.Max(1e-9, engine.Time / 60);
            list.Add(new($"{line.Id}: throughput in target", InRange(thr, spec.KpiTargets.ThroughputPerMin), $"{thr:F1}/min avg"));

            var raised = engine.Events.Count(e => e.Kind == SimEventKind.FaultRaised && e.NodeId != "");
            var cleared = engine.Events.Count(e => e.Kind == SimEventKind.FaultCleared);
            list.Add(new($"{line.Id}: faults happen and recover", raised > 0 && cleared > 0, $"{raised} raised, {cleared} cleared"));

            var dead = line.Stations.Where(s => s.TimeIn(MachineState.Running) <= 0).Select(s => s.Id).ToList();
            list.Add(new($"{line.Id}: every station runs", dead.Count == 0, dead.Count == 0 ? "ok" : "never running: " + string.Join(",", dead)));

            var (fed, accounted) = Balance(line);
            list.Add(new($"{line.Id}: bottle balance", fed == accounted, $"fed {fed} = good {line.GoodUnits} + reject {line.RejectUnits} + wip {accounted - line.GoodUnits - line.RejectUnits}"));
        }
        return list;
    }

    /// <summary>Bottles put on the first belt vs. delivered + rejected + still inside the line.</summary>
    public static (long Fed, long Accounted) Balance(Line line)
    {
        var fed = line.Stations[0].Good;
        var wip = line.Conveyors.Sum(c => c.Items.Sum(i => (long)i.Units)) + line.Stations.Skip(1).Sum(s => (long)s.WipUnits);
        return (fed, line.GoodUnits + line.RejectUnits + wip);
    }

    static bool InRange(double v, double[] r) => r.Length == 2 && v >= r[0] && v <= r[1];
}
