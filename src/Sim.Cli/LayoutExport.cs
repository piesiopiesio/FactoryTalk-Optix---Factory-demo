// @summary: Writes design/data/layout.json (zones, line rects, station + belt rects) so the preview never re-implements geometry.
using Factory.Core.Manifest;

namespace Sim.Cli;

public static class LayoutExport
{
    public static object Build(FactoryManifest m) => new
    {
        canvas = new[] { Layout.CanvasW, Layout.CanvasH },
        hall = new
        {
            m.Hall.Id, m.Hall.Name, m.Hall.Size,
            zones = m.Hall.Zones.Select(z => new { z.Id, z.Name, z.Rect, z.Planned }),
            lines = m.Lines.Select(l => new { l.Id, l.Name, l.Zone, l.Rect }),
        },
        lines = m.Lines.ToDictionary(l => l.Id, l => new
        {
            l.Name, l.OeeStation, product = l.Product, l.KpiTargets,
            stations = l.Stations.Select(s => new { s.Id, s.Name, s.Type, rect = R(Layout.Station(s)) }),
            conveyors = l.Conveyors.Select(c =>
            {
                var (a, b) = (l.Stations.Single(s => s.Id == c.From), l.Stations.Single(s => s.Id == c.To));
                var dir = Layout.IsHorizontal(a, b) ? (a.Pos[0] < b.Pos[0] ? "right" : "left") : (a.Pos[1] < b.Pos[1] ? "down" : "up");
                return new { c.Id, c.From, c.To, c.LengthM, rect = R(Layout.Conveyor(a, b)), dir };
            }),
        }),
    };

    static double[] R(Rect r) => [r.X, r.Y, r.W, r.H];
}
