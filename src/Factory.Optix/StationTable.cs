// @summary: Station table under the line diagram: one row per station (state, good/reject, cycle, speed, progress, fault message).
#region Using directives
using System;
using UAManagedCore;
using FTOptix.UI;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class StationTable
{
    const double X0 = 24, Y0 = 688, RowH = 30, Width = 1524;

    static readonly (string Header, double X, string Signal, string Format)[] Columns =
    {
        ("Stacja", 0, "", ""),
        ("Stan", 230, nameof(FM.Equipment.StateText), "{0}"),
        ("Dobre [szt.]", 420, nameof(FM.Station.Good), "{0}"),
        ("Odrzuty [szt.]", 550, nameof(FM.Station.Reject), "{0}"),
        ("Takt [s]", 690, nameof(FM.Station.CycleTimeS), "{0}"),
        ("Prędkość [%]", 800, nameof(FM.Equipment.SpeedPct), "{0}"),
        ("Postęp [%]", 930, nameof(FM.Station.Progress), "{0}"),
        ("Komunikat", 1050, nameof(FM.Equipment.FaultText), "{0}"),
    };

    public static void Build(IUANode screen, FX.LineSpec spec)
    {
        var table = Ui.Box(screen, "StationTable", X0, Y0, Width, RowH * (spec.Stations.Count + 1), N.GroupArgb, N.LineArgb);
        var header = Ui.Box(table, "Header", 0, 0, Width, RowH, N.TabArgb, N.LineArgb, 0);
        foreach (var c in Columns) Ui.Text(header, "H_" + Safe(c.Header), c.Header, c.X + 10, 6, 13, N.TitleArgb, bold: true);

        for (var i = 0; i < spec.Stations.Count; i++)
        {
            var s = spec.Stations[i];
            var path = $"{spec.Id}/{s.Id}";
            var row = Ui.Box(table, "Row_" + s.Id, 0, RowH * (i + 1), Width, RowH, i % 2 == 0 ? N.GroupArgb : N.BackgroundArgb, N.SeparatorArgb, 0);
            Ui.Text(row, "Name", $"{s.Id}  {s.Name}", 10, 6, 13, N.TitleArgb);
            var ind = Ui.Box(row, "State", Columns[1].X + 10, 8, 14, 14, N.BackgroundArgb);
            Ui.Link(ind.FillColorVariable, N.ModelVar(path, N.StateColorVar));
            for (var c = 1; c < Columns.Length; c++)
            {
                var x = Columns[c].X + 10 + (c == 1 ? 22 : 0);
                Ui.Value(row, "C_" + Columns[c].Signal, Columns[c].Format, x, 6, 13, N.ModelVar(path, Columns[c].Signal));
            }
        }
    }

    static string Safe(string s)
    {
        var chars = s.ToCharArray();
        for (var i = 0; i < chars.Length; i++) if (!char.IsLetterOrDigit(chars[i])) chars[i] = '_';
        return new string(chars);
    }
}
