// @summary: Screen geometry from factory.json (1600x900 logical canvas): station/conveyor rects + layout lint. Used by Optix builder and preview.
#nullable enable
namespace Factory.Core.Manifest;

public sealed record Rect(double X, double Y, double W, double H)
{
    public double Right => X + W;
    public double Bottom => Y + H;
    public double Cx => X + W / 2;
    public double Cy => Y + H / 2;
    public bool Overlaps(Rect o) => X < o.Right && o.X < Right && Y < o.Bottom && o.Y < Bottom;
}

public static class Layout
{
    public const double CanvasW = 1600, CanvasH = 900;
    public const double StationW = 150, StationH = 110, Belt = 20;
    /// <summary>Optix screens are 1920x1080: multiply logical coordinates by this.</summary>
    public const double OptixScale = 1.2;

    public static Rect Station(StationSpec s) => new(s.Pos[0], s.Pos[1], StationW, StationH);

    /// <summary>Straight belt between facing edges; horizontal when rows match, vertical when columns match.</summary>
    public static Rect Conveyor(StationSpec from, StationSpec to)
    {
        Rect a = Station(from), b = Station(to);
        if (Math.Abs(a.Y - b.Y) < 1)
        {
            var (l, r) = a.X < b.X ? (a.Right, b.X) : (b.Right, a.X);
            return new Rect(l, a.Cy - Belt / 2, r - l, Belt);
        }
        var (t, btm) = a.Y < b.Y ? (a.Bottom, b.Y) : (b.Bottom, a.Y);
        return new Rect(a.Cx - Belt / 2, t, Belt, btm - t);
    }

    public static bool IsHorizontal(StationSpec from, StationSpec to) => Math.Abs(from.Pos[1] - to.Pos[1]) < 1;

    /// <summary>Layout problems: overlaps, off-canvas, belts not straight or too short to see.</summary>
    public static List<string> Lint(FactoryManifest m)
    {
        var issues = new List<string>();
        foreach (var line in m.Lines)
        {
            var st = line.Stations.ToDictionary(s => s.Id);
            var rects = line.Stations.Select(s => (s.Id, R: Station(s))).ToList();
            foreach (var (id, r) in rects)
                if (r.X < 0 || r.Y < 0 || r.Right > CanvasW || r.Bottom > CanvasH) issues.Add($"{line.Id}/{id}: outside {CanvasW}x{CanvasH}");
            for (var i = 0; i < rects.Count; i++)
                for (var j = i + 1; j < rects.Count; j++)
                    if (rects[i].R.Overlaps(rects[j].R)) issues.Add($"{line.Id}: {rects[i].Id} overlaps {rects[j].Id}");
            foreach (var c in line.Conveyors.Where(c => st.ContainsKey(c.From) && st.ContainsKey(c.To)))
            {
                var (a, b) = (st[c.From], st[c.To]);
                if (Math.Abs(a.Pos[0] - b.Pos[0]) >= 1 && Math.Abs(a.Pos[1] - b.Pos[1]) >= 1) issues.Add($"{line.Id}/{c.Id}: stations not in one row/column");
                var r = Conveyor(a, b);
                if (Math.Max(r.W, r.H) < 40) issues.Add($"{line.Id}/{c.Id}: belt shorter than 40 px");
            }
        }
        return issues;
    }
}
