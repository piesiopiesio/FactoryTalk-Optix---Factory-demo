// @summary: Level 2 line diagram (LineScreen_<id>): belts with occupancy bars and station tiles (state, counts, own signals, progress).
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.UI;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class LineView
{
    public static void Build(IUANode screen, FX.LineSpec spec, FM.Line line)
    {
        Ui.Background(screen, "Background", N.BackgroundArgb);
        Ui.Text(screen, "Title", spec.Name + " — schemat linii", 24, 14, 20, N.TitleArgb, bold: true);

        var byId = spec.Stations.ToDictionary(s => s.Id);
        foreach (var c in spec.Conveyors) Belt(screen, spec.Id, c, byId[c.From], byId[c.To]);
        for (var i = 0; i < spec.Stations.Count; i++)
            Tile(screen, spec.Id, spec.Stations[i], line.Stations.First(x => x.Id == spec.Stations[i].Id), i + 1);
    }

    static void Belt(IUANode screen, string lineId, FX.ConveyorSpec c, FX.StationSpec from, FX.StationSpec to)
    {
        var r = FX.Layout.Conveyor(from, to);
        double x = N.X(r.X), y = N.Y(r.Y), w = N.L(r.W), h = N.L(r.H);
        var path = $"{lineId}/{c.Id}";
        var belt = Ui.Box(screen, "Belt_" + c.Id, x, y, w, h, N.BackgroundArgb, N.LineArgb, 2);
        Ui.Link(belt.FillColorVariable, N.ModelVar(path, N.StateColorVar));

        var horizontal = FX.Layout.IsHorizontal(from, to);
        Ui.Bar(belt, "Load", 3, 3, w - 6, h - 6, !horizontal, N.LineArgb, N.ModelVar(path, nameof(FM.Conveyor.Occupancy)));

        // Belt id + items: above a horizontal belt, right of a vertical one.
        double lx = horizontal ? x + w / 2 - 40 : x + w + 8, ly = horizontal ? y - 22 : y + h / 2 - 10;
        Ui.Value(screen, "BeltInfo_" + c.Id, c.Id + "  {0} szt.", lx, ly, 13, N.ModelVar(path, nameof(FM.Conveyor.ItemsOnBelt)));
    }

    static void Tile(IUANode screen, string lineId, FX.StationSpec s, FM.Station station, int k)
    {
        var r = FX.Layout.Station(s);
        double w = N.L(r.W), h = N.L(r.H);
        var path = $"{lineId}/{s.Id}";
        var tile = Ui.Box(screen, "Station_" + s.Id, N.X(r.X), N.Y(r.Y), w, h, N.BackgroundArgb, N.LineArgb, 1);

        Ui.Text(tile, "Name", s.Name, 10, 8, 15, N.TitleArgb, bold: true);
        Ui.Text(tile, "Id", s.Id, 10, 28, 11, N.UnitsArgb);
        var ind = Ui.Box(tile, "State", w - 30, 8, 20, 20, N.BackgroundArgb);
        Ui.Link(ind.FillColorVariable, N.ModelVar(path, N.StateColorVar));
        Ui.Value(tile, "StateText", "{0}", 10, 46, 13, N.ModelVar(path, nameof(FM.Equipment.StateText)));
        Ui.Value(tile, "Good", "Dobre {0}", 10, 66, 13, N.ModelVar(path, nameof(FM.Station.Good)));

        // Own signals of the station class (Filler: fill + tank, Capper: torque...), else the cycle time.
        var own = FM.Signals.Of(station.GetType()).Where(x => x.Property.DeclaringType == station.GetType()).Take(2).ToList();
        if (own.Count == 0) own = FM.Signals.Of(station.GetType()).Where(x => x.Name == nameof(FM.Station.CycleTimeS)).ToList();
        for (var i = 0; i < own.Count; i++)
            Ui.Value(tile, "Signal_" + own[i].Name, $"{own[i].Caption} {{0}} {own[i].Unit}".TrimEnd(), 10, 86 + i * 18, 13,
                N.ModelVar(path, own[i].Name));

        Ui.Bar(tile, "Progress", 4, h - 7, w - 8, 4, false, N.LineArgb, N.ModelVar(path, nameof(FM.Station.Progress)));
        StationDetail.ClickTarget(tile, w, h, lineId, k);   // click on the tile -> station faceplate (native UI)
        StationDetail.OpenButton(screen, s.Id, N.X(r.X), N.Y(r.Y), lineId, k);   // explicit button (works in web too)
    }
}
