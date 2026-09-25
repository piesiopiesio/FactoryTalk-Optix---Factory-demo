// @summary: Fills HallScreen/LineScreen FactoryContent panels: zones, line tiles, stations and belts at manifest positions (x1.2).
#region Using directives
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.CoreBase;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;

public static class ScreenGenerator
{
    const double S = FX.Layout.OptixScale;

    public static void Build(FX.FactoryManifest manifest, FM.Hall hall)
    {
        var hallContent = Project.Current.Get(OptixNames.HallContent);
        var lineContent = Project.Current.Get(OptixNames.LineContent);
        if (hallContent == null || lineContent == null)
        {
            Log.Warning("ScreenGenerator", "Create HallScreen/LineScreen with a Panel 'FactoryContent' (docs/studio-setup.md)");
            return;
        }
        NodeUtil.ClearChildren(hallContent);
        NodeUtil.ClearChildren(lineContent);
        BuildHall(hallContent, manifest);
        if (manifest.Lines.Count > 0) BuildLine(lineContent, manifest.Lines[0]);   // TODO backlog: alias-based LineScreen per line
    }

    // Style: docs/hmi-style.md (Rockwell Process HMI Style Guide). Equipment interior = background, border Gray 160,
    // state shown by a bordered indicator (normal states are calm), live data in blue.
    static void BuildHall(IUANode content, FX.FactoryManifest m)
    {
        foreach (var z in m.Hall.Zones)
        {
            var zone = NodeUtil.Box("Zone_" + z.Id, z.Rect[0], z.Rect[1], z.Rect[2], z.Rect[3], z.Planned ? OptixNames.BackgroundArgb : OptixNames.GroupArgb, S);
            zone.BorderColor = new Color(OptixNames.LineArgb);
            zone.BorderThickness = 1;
            content.Add(zone);
            content.Add(NodeUtil.Text("ZoneName_" + z.Id, z.Name + (z.Planned ? " (planowana)" : ""), z.Rect[0] + 12, z.Rect[1] + 8, 16, OptixNames.TitleArgb, S));
        }
        foreach (var l in m.Lines)
        {
            var tile = NodeUtil.Box("LineTile_" + l.Id, l.Rect[0], l.Rect[1], l.Rect[2], l.Rect[3], OptixNames.BackgroundArgb, S);
            tile.BorderColor = new Color(OptixNames.LineArgb);
            tile.BorderThickness = 1;
            content.Add(tile);
            content.Add(Indicator("LineState_" + l.Id, l.Rect[0] + 12, l.Rect[1] + 14, $"{l.Id}/{OptixNames.StateColorVar}"));
            content.Add(NodeUtil.Text("LineName_" + l.Id, l.Name, l.Rect[0] + 40, l.Rect[1] + 10, 18, OptixNames.TitleArgb, S));
            var oee = NodeUtil.Text("LineOee_" + l.Id, "", l.Rect[0] + 40, l.Rect[1] + 40, 15, OptixNames.DataArgb, S);
            Link(oee.TextVariable, $"{l.Id}/{OptixNames.Var(nameof(FM.Line.Oee))}");
            content.Add(oee);
        }
    }

    static void BuildLine(IUANode content, FX.LineSpec line)
    {
        var stations = line.Stations.ToDictionary(s => s.Id);
        foreach (var c in line.Conveyors)
        {
            var r = FX.Layout.Conveyor(stations[c.From], stations[c.To]);
            var belt = NodeUtil.Box("Belt_" + c.Id, r.X, r.Y, r.W, r.H, OptixNames.BackgroundArgb, S);
            belt.BorderColor = new Color(OptixNames.LineArgb);
            belt.BorderThickness = 3;
            content.Add(belt);
            var fill = NodeUtil.Text("BeltItems_" + c.Id, "", r.Cx - 10, r.Cy - 26, 12, OptixNames.DataArgb, S);
            Link(fill.TextVariable, $"{line.Id}/{c.Id}/{OptixNames.Var(nameof(FM.Conveyor.ItemsOnBelt))}");
            content.Add(fill);
        }
        foreach (var s in line.Stations)
        {
            var r = FX.Layout.Station(s);
            var body = NodeUtil.Box("Station_" + s.Id, r.X, r.Y, r.W, r.H, OptixNames.BackgroundArgb, S);
            body.BorderColor = new Color(OptixNames.LineArgb);
            body.BorderThickness = 1;
            content.Add(body);
            content.Add(Indicator("StationState_" + s.Id, r.X + r.W - 30, r.Y + 10, $"{line.Id}/{s.Id}/{OptixNames.StateColorVar}"));
            content.Add(NodeUtil.Text("StationName_" + s.Id, s.Name, r.X + 10, r.Y + 10, 14, OptixNames.TitleArgb, S));
            content.Add(NodeUtil.Text("StationId_" + s.Id, s.Id, r.X + 10, r.Y + 32, 11, OptixNames.UnitsArgb, S));
            var good = NodeUtil.Text("StationGood_" + s.Id, "", r.X + 10, r.Y + 66, 15, OptixNames.DataArgb, S);
            Link(good.TextVariable, $"{line.Id}/{s.Id}/{OptixNames.Var(nameof(FM.Station.Good))}");
            content.Add(good);
        }
    }

    /// <summary>20x20 state indicator with a Gray 160 border (off-white "running" stays visible on light background).</summary>
    static Rectangle Indicator(string name, double x, double y, string colorVarPath)
    {
        var box = NodeUtil.Box(name, x, y, 20, 20, OptixNames.BackgroundArgb, S);
        box.BorderColor = new Color(OptixNames.LineArgb);
        box.BorderThickness = 1;
        Link(box.FillColorVariable, colorVarPath);
        return box;
    }

    static void Link(IUAVariable target, string modelPath)
    {
        var source = Project.Current.GetVariable($"Model/{OptixNames.ModelFolder}/{modelPath}");
        if (source != null) target.SetDynamicLink(source);
        else Log.Warning("ScreenGenerator", "Missing variable " + modelPath);
    }
}
