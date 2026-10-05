// @summary: Level 1 screen (HallScreen): zones, one tile per line with state, KPIs and a live mini-map of its stations.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class HallView
{
    public static void Build(IUANode screen, FX.FactoryManifest m)
    {
        Ui.Background(screen, "Background", N.BackgroundArgb);
        Ui.Text(screen, "Title", m.Hall.Name + " — hala, przegląd", 24, 14, 20, N.TitleArgb, bold: true);

        foreach (var z in m.Hall.Zones)
        {
            Ui.Box(screen, "Zone_" + z.Id, N.X(z.Rect[0]), N.Y(z.Rect[1]), N.L(z.Rect[2]), N.L(z.Rect[3]),
                z.Planned ? N.BackgroundArgb : N.GroupArgb);
            Ui.Text(screen, "ZoneName_" + z.Id, z.Name + (z.Planned ? " (planowana)" : ""),
                N.X(z.Rect[0]) + 12, N.Y(z.Rect[1]) + 8, 16, z.Planned ? N.UnitsArgb : N.TitleArgb, bold: true);
        }
        // Per-screen request variable: a variable of the screen itself is per session at runtime (the screen is instantiated
        // in each session's MainNav), a Model variable would switch the tab for every client.
        var openTab = InformationModel.MakeVariable(N.OpenTab, OpcUa.DataTypes.Int32);
        openTab.Value = -1;
        screen.Add(openTab);
        foreach (var l in m.Lines) LineTile(screen, l, FX.NavTabs.IndexOf(m, l.Id));
    }

    static void LineTile(IUANode screen, FX.LineSpec l, int tabIndex)
    {
        double x = N.X(l.Rect[0]), y = N.Y(l.Rect[1]), w = N.L(l.Rect[2]), h = N.L(l.Rect[3]);
        var tile = Ui.Box(screen, "Line_" + l.Id, x, y, w, h, N.BackgroundArgb);

        var ind = Ui.Box(tile, "State", 16, 16, 22, 22, N.BackgroundArgb);
        Ui.Link(ind.FillColorVariable, N.ModelVar(l.Id, N.StateColorVar));
        Ui.Value(tile, "StateText", "{0}", 48, 16, 16, N.ModelVar(l.Id, nameof(FM.Line.StateText)));
        Ui.Text(tile, "Name", l.Name, 16, 48, 20, N.TitleArgb, bold: true);
        Ui.Text(tile, "Product", $"{l.Product.Name} ({l.Product.Sku})", 16, 76, 13, N.UnitsArgb);

        var kpis = new (string Name, string Format, string Signal)[]
        {
            ("Oee", "OEE {0} %", nameof(FM.Line.Oee)),
            ("Throughput", "{0} szt./min", nameof(FM.Line.ThroughputPerMin)),
            ("Good", "Dobre {0} szt.", nameof(FM.Line.GoodUnits)),
            ("Faults", "Awarie {0}", nameof(FM.Line.ActiveFaults)),
        };
        for (var i = 0; i < kpis.Length; i++)
            Ui.Value(tile, kpis[i].Name, kpis[i].Format, 380 + i * 200, 20, 18, N.ModelVar(l.Id, kpis[i].Signal));

        MiniMap(tile, l, 16, 110, w - 32, h - 126);

        // Level 1 -> level 2: whole tile clickable (last child = on top) + explicit button (web client ignores Rectangle clicks).
        var openTab = $"{N.ScreensFolder}/{N.HallScreen}/{N.OpenTab}";
        var hit = Ui.Box(tile, "Open", 0, 0, w, h, 0x01FFFFFF, 0x00000000, 0);   // alpha 1/255: alpha 0 is not hit-tested
        Ui.OnClickSet(hit, openTab, tabIndex, relative: true);
        var button = Ui.Button(tile, "OpenLine", "Ekran linii", w - 172, 56, 160);   // below the KPI row, above the mini-map
        Ui.OnClickSet(button, openTab, tabIndex, relative: true);
    }

    /// <summary>Stations and belts of the line, scaled into the tile: state colors at a glance (level 1).</summary>
    static void MiniMap(IUANode tile, FX.LineSpec l, double ox, double oy, double ow, double oh)
    {
        if (l.Stations.Count == 0) return;
        var rects = l.Stations.Select(FX.Layout.Station).ToList();
        double minX = rects.Min(r => r.X), minY = rects.Min(r => r.Y);
        double spanX = rects.Max(r => r.Right) - minX, spanY = rects.Max(r => r.Bottom) - minY;
        var k = Math.Min(ow / spanX, oh / spanY);
        double Mx(double v) => ox + (v - minX) * k;
        double My(double v) => oy + (v - minY) * k;

        var map = Ui.Box(tile, "Map", ox - 8, oy - 8, spanX * k + 16, spanY * k + 16, N.GroupArgb, N.SeparatorArgb);
        map.HitTestVisible = false;
        var byId = l.Stations.ToDictionary(s => s.Id);
        foreach (var c in l.Conveyors)
        {
            var r = FX.Layout.Conveyor(byId[c.From], byId[c.To]);
            var belt = Ui.Box(tile, "Belt_" + c.Id, Mx(r.X), My(r.Y), r.W * k, r.H * k, N.LineArgb, N.LineArgb);
            Ui.Link(belt.FillColorVariable, N.ModelVar($"{l.Id}/{c.Id}", N.StateColorVar));
        }
        foreach (var s in l.Stations)
        {
            var r = FX.Layout.Station(s);
            var box = Ui.Box(tile, "Station_" + s.Id, Mx(r.X), My(r.Y), r.W * k, r.H * k, N.BackgroundArgb);
            Ui.Link(box.FillColorVariable, N.ModelVar($"{l.Id}/{s.Id}", N.StateColorVar));
            Ui.Text(box, "Id", s.Id, 6, 4, 12, N.TitleArgb, bold: true);
            Ui.Value(box, "State", "{0}", 6, 22, 11, N.ModelVar($"{l.Id}/{s.Id}", nameof(FM.Equipment.StateText)));
        }
    }
}
