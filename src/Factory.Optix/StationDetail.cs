// @summary: Level 3 station faceplate on the line screen: tile click opens it; Template Library graphic moved only by live signals.
#region Using directives
using System;
using System.Globalization;
using System.Linq;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion
using FM = Factory.Core.Model;
using FS = Factory.Core.Stations;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class StationDetail
{
    const double X0 = 420, Y0 = 110, W = 660, H = 440;            // over the line diagram, left of LinePanel
    const double GX = 20, GY = 64, GW = 260, GH = 300;            // graphic frame inside the faceplate
    const double BX = 105, BY = 140, BW = 50, BH = 138;           // bottle in the frame (Bottle1 is 13.2 x 36.5)
    const uint Glass = 0xFFE8E8E8, Cap = 0xFF767676, Label = 0xFFFFFFFF, Transparent = 0x00000000;

    /// <summary>Invisible click target over a station tile (last child, so on top): selectedStation = k.</summary>
    public static void ClickTarget(IUANode tile, double w, double h, string lineId, int k)
    {
        // Alpha 1/255, not 0: a fully transparent rectangle is not hit-tested (clicks fall through, checked in the emulator).
        var hit = Ui.Box(tile, "Open", 0, 0, w, h, 0x01FFFFFF, Transparent, 0);
        Ui.OnClickSet(hit, N.ModelVar(lineId, N.SelectedStation), k);
    }

    /// <summary>Explicit "Szczegóły" button above the tile (web client does not deliver clicks on a Rectangle).
    /// Narrow (78 px) so it stays left of a vertical belt leaving the tile centre.</summary>
    public static void OpenButton(IUANode screen, string stationId, double tileX, double tileY, string lineId, int k)
    {
        var b = Ui.Button(screen, "Details_" + stationId, "Szczegóły", tileX, tileY - 46, 78, 40);
        b.FontSize = 12;
        Ui.OnClickSet(b, N.ModelVar(lineId, N.SelectedStation), k);
    }

    public static void Build(IUANode screen, FX.LineSpec spec, FM.Line line)
    {
        for (var i = 0; i < spec.Stations.Count; i++)
            Faceplate(screen, spec.Id, spec.Stations[i], line.Stations.First(x => x.Id == spec.Stations[i].Id), i + 1);
    }

    static void Faceplate(IUANode screen, string lineId, FX.StationSpec s, FM.Station st, int k)
    {
        var path = $"{lineId}/{s.Id}";
        string V(string signal) => N.ModelVar(path, signal);
        var box = Ui.Box(screen, "Detail_" + s.Id, X0, Y0, W, H, N.GroupArgb, N.TitleArgb, 2);
        Ui.Expression(box.VisibleVariable, "{0} == " + k, N.ModelVar(lineId, N.SelectedStation));
        Ui.Text(box, "Title", $"{s.Name} ({s.Id})", 16, 14, 18, N.TitleArgb, bold: true);
        var close = Ui.Button(box, "Close", "Zamknij", W - 136, 8, 120);
        Ui.OnClickSet(close, N.ModelVar(lineId, N.SelectedStation), 0);

        // Station frame: fill = equipment state colour (same as the tile), product graphic inside.
        var frame = Ui.Box(box, "Graphic", GX, GY, GW, GH, N.BackgroundArgb, N.LineArgb, 2);
        Ui.Link(frame.FillColorVariable, V(N.StateColorVar));
        Graphic(frame, st, V);

        double y = GY;
        const double lx = GX + GW + 24, vx = lx + 150;
        void Row(string caption, string format, string signal)
        {
            Ui.Text(box, "Cap_" + signal, caption, lx, y + 2, 14, N.TitleArgb);
            Ui.Value(box, "Val_" + signal, format, vx, y, 16, V(signal));
            y += 30;
        }
        Row("Stan", "{0}", nameof(FM.Equipment.StateText));
        Row("Postęp cyklu", "{0} %", nameof(FM.Station.Progress));
        Row("Takt", "{0} s", nameof(FM.Station.CycleTimeS));
        Row("Dobre", "{0} szt.", nameof(FM.Station.Good));
        Row("Odrzuty", "{0} szt.", nameof(FM.Station.Reject));
        foreach (var sig in FM.Signals.Of(st.GetType()).Where(x => x.Property.DeclaringType == st.GetType()))
            Row(sig.Caption, ("{0} " + sig.Unit).TrimEnd(), sig.Name);
        var fault = Ui.Value(box, "Fault", "{0}", lx, y + 6, 13, V(nameof(FM.Equipment.FaultText)));
        fault.Width = (float)(W - lx - 16);
        fault.WordWrap = true;
        var reset = Ui.Button(box, "Reset", "Kasuj awarię", lx, H - 60, 160);     // enabled by AccessLogic (maintenance)
        Ui.OnClickSet(reset, V(N.CmdReset), true);
        var inject = Ui.Button(box, "InjectFault", "Wstrzyknij awarię", lx + 170, H - 60, 160);   // test fault, maintenance only
        Ui.OnClickSet(inject, V(N.CmdFault), true);
        var hint = Ui.Text(box, "Hint", "Grafika porusza się tylko z danymi procesu (cykl, poziom, liczba sztuk).", GX, GY + GH + 8, 11, N.UnitsArgb);
        hint.Width = (float)GW;
        hint.WordWrap = true;
    }

    /// <summary>Style guide: no decorative animation — every movement is a live value (cycle progress, level, count).</summary>
    static void Graphic(IUANode frame, FM.Station st, Func<string, string> V)
    {
        var progress = V(nameof(FM.Station.Progress));
        switch (st)
        {
            case FS.Feeder:
                Ui.Box(frame, "Belt", 10, BY + BH, GW - 20, 8, N.LineArgb, N.LineArgb, 0);
                var moving = Bottle(frame, 20, BY, level: "0", levelPath: null);
                if (moving is Item m) Ui.Expression(m.LeftMarginVariable, "20 + {0} * 1.7", progress);
                Caption(frame, "Butelka wjeżdża na linię (postęp cyklu)");
                break;
            case FS.Filler f:
                Ui.Box(frame, "Nozzle", BX + 18, 40, 14, BY - 44, N.LineArgb, N.TitleArgb, 1);
                Bottle(frame, BX, BY, "{0} * " + Num(100 / f.Param("fillMl", 500)), V(nameof(FS.Filler.FillMl)));
                UpBar(frame, "Tank", 20, BY + BH, 24, 200, N.DataArgb, V(nameof(FS.Filler.TankLevel)), 2.0);
                Caption(frame, "Poziom w butelce = napełnienie; słupek = zbiornik");
                break;
            case FS.Capper:
                var head = Ui.Box(frame, "Head", BX + 5, 40, BW - 10, 36, N.LineArgb, N.TitleArgb, 1);
                Ui.Expression(head.TopMarginVariable, "30 + {0} * 0.72", progress);   // reaches the cap at 100 %
                Bottle(frame, BX, BY, "100", null);
                Caption(frame, "Głowica schodzi na nakrętkę w rytmie cyklu");
                break;
            case FS.Labeler l:
                Bottle(frame, BX, BY, "100", null);
                var band = Ui.Box(frame, "Label", BX, BY + 70, 0, 34, Label, N.TitleArgb, 1);
                Ui.Expression(band.WidthVariable, "{0} * " + Num(BW / 100), progress);
                UpBar(frame, "Roll", 20, BY + BH, 24, 200, N.DataArgb, V(nameof(FS.Labeler.LabelStock)), 2.0);
                Caption(frame, "Etykieta owija się w cyklu; słupek = rolka");
                break;
            case FS.VisionInspector:
                Bottle(frame, BX, BY, "100", null);
                Set(Style(LibraryViews.Instance(frame, "Camera", N.LibPhotoEye, 16, BY + 30, 40, 60)), "IndicatorColor", Cap);
                var scan = Ui.Box(frame, "Scan", BX - 20, BY, BW + 40, 2, N.DataArgb, N.DataArgb, 0);
                Ui.Expression(scan.TopMarginVariable, Num(BY) + " + {0} * " + Num(BH / 100), progress);
                Caption(frame, "Linia skanu = postęp inspekcji");
                break;
            case FS.CasePacker c:
                var carton = Style(LibraryViews.Instance(frame, "Carton", N.LibCarton, 50, 90, 160, 160));
                Set(carton, "ShowOpen", true);
                Link(carton, "Level", "{0} * " + Num(100 / c.Param("unitsPerCase", 12)), V(nameof(FS.CasePacker.UnitsInCase)));
                Caption(frame, "Wypełnienie kartonu = butelki w kartonie");
                break;
            case FS.Palletizer p:
                Style(LibraryViews.Instance(frame, "Pallet", N.LibPallet, 40, BY + BH - 30, 180, 40));
                UpBar(frame, "Stack", 50, BY + BH - 30, 160, 200, N.LineArgb, V(nameof(FS.Palletizer.CasesOnPallet)),
                    200 / p.Param("casesPerPallet", 40));
                Caption(frame, "Wysokość stosu = kartony na palecie");
                break;
            default:
                Caption(frame, "Brak grafiki dla tej stacji");
                break;
        }
    }

    /// <summary>Template Library Bottle1: glass = background grey, liquid = live-data blue, no 3D overlay.</summary>
    static IUAObject Bottle(IUANode frame, double x, double y, string level, string levelPath)
    {
        var b = Style(LibraryViews.Instance(frame, "Bottle", N.LibBottle, x, y, BW, BH));
        if (b == null) return null;
        Set(b, "CapColor", Cap);
        if (levelPath == null) Set(b, "Level", double.Parse(level, CultureInfo.InvariantCulture));
        else Link(b, "Level", level, levelPath);
        return b;
    }

    /// <summary>Flat look (style guide): neutral fill, data-blue level, 3D overlay images hidden.</summary>
    static IUAObject Style(IUAObject graphic)
    {
        if (graphic == null) return null;
        Set(graphic, "FillColor", Glass);
        Set(graphic, "LevelColor", N.DataArgb);
        foreach (var child in graphic.Children)
            if (child.BrowseName.Contains("Overlay") && child is Item overlay) overlay.Visible = false;
        return graphic;
    }

    static void Link(IUAObject graphic, string variable, string expression, string sourcePath)
    {
        var v = graphic?.GetVariable(variable);
        if (v != null) Ui.Expression(v, expression, sourcePath);
    }

    /// <summary>Vertical bar growing upwards from bottomY: height = value * pxPerUnit.</summary>
    static void UpBar(IUANode parent, string name, double x, double bottomY, double w, double maxH, uint fill, string valuePath, double pxPerUnit)
    {
        Ui.Box(parent, name + "Frame", x - 2, bottomY - maxH - 2, w + 4, maxH + 4, Transparent, N.LineArgb, 1);
        var bar = Ui.Box(parent, name, x, bottomY, w, 0, fill, fill, 0);
        Ui.Expression(bar.HeightVariable, "{0} * " + Num(pxPerUnit), valuePath);
        Ui.Expression(bar.TopMarginVariable, Num(bottomY) + " - {0} * " + Num(pxPerUnit), valuePath);
    }

    static void Caption(IUANode frame, string text) => Ui.Text(frame, "Caption", text, 8, 6, 11, N.UnitsArgb);

    static string Num(double v) => v.ToString("0.####", CultureInfo.InvariantCulture);

    /// <summary>Colours as ARGB UInt32 (same as the stateColor links), numbers, flags; missing variable = no-op.</summary>
    static void Set(IUAObject graphic, string variable, UAValue value)
    {
        var v = graphic?.GetVariable(variable);
        if (v != null) v.Value = value;
    }
}
