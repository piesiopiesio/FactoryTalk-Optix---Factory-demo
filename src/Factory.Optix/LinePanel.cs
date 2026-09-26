// @summary: Right-hand panel of a line screen: state, OEE/KPIs, commands (Start, Stop with confirmation, reset, time) by user role, active fault list.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class LinePanel
{
    const double X0 = 1572, W = 336, Pad = 16;

    public static void Build(IUANode screen, FX.LineSpec spec, FM.Line line, string hallId)
    {
        var panel = Ui.Box(screen, "LinePanel", X0, 60, W, 880, N.GroupArgb, N.LineArgb);
        double y = 14;

        var ind = Ui.Box(panel, "State", Pad, y + 2, 22, 22, N.BackgroundArgb);
        Ui.Link(ind.FillColorVariable, N.ModelVar(spec.Id, N.StateColorVar));
        Ui.Value(panel, "StateText", "{0}", Pad + 32, y, 18, N.ModelVar(spec.Id, nameof(FM.Line.StateText)));
        y += 36;
        Ui.Text(panel, "Product", $"{spec.Product.Name} ({spec.Product.Sku})", Pad, y, 13, N.UnitsArgb);
        y += 30;

        var kpis = new (string Caption, string Signal, string Unit)[]
        {
            ("OEE", nameof(FM.Line.Oee), "%"),
            ("Dostępność", nameof(FM.Line.Availability), "%"),
            ("Wydajność", nameof(FM.Line.Performance), "%"),
            ("Jakość", nameof(FM.Line.Quality), "%"),
            ("Przepustowość", nameof(FM.Line.ThroughputPerMin), "szt./min"),
            ("Dobre", nameof(FM.Line.GoodUnits), "szt."),
            ("Odrzuty", nameof(FM.Line.RejectUnits), "szt."),
        };
        foreach (var k in kpis)
        {
            var big = k.Signal == nameof(FM.Line.Oee);
            Ui.Text(panel, "Kpi_" + k.Signal, k.Caption, Pad, y + (big ? 6 : 0), 14, N.TitleArgb);
            Ui.Value(panel, "KpiValue_" + k.Signal, "{0} " + k.Unit, 150, y, big ? 26 : 16, N.ModelVar(spec.Id, k.Signal));
            y += big ? 42 : 28;
        }
        y += 12;

        // Filled by AccessLogic (per session): which commands the logged-in user may use.
        Ui.Text(panel, "AccessInfo", "Uprawnienia: …", Pad, y, 13, N.UnitsArgb);
        y += 26;
        var commandsY = y;
        Command(panel, "Start", "Start", Pad, y, 146, N.ModelVar(spec.Id, N.CmdStart), true);
        Command(panel, "Stop", "Stop", Pad + 158, y, 146, N.ModelVar(spec.Id, N.StopRequest), true);   // opens the confirmation
        y += 54;
        Command(panel, "Reset", "Kasuj awarie", Pad, y, 304, N.ModelVar(spec.Id, N.CmdReset), true);
        y += 54;
        Command(panel, "Time1", "Czas ×1", Pad, y, 146, N.ModelVar(hallId, N.TimeScale), 1);
        Command(panel, "Time10", "Czas ×10", Pad + 158, y, 146, N.ModelVar(hallId, N.TimeScale), 10);
        y += 50;
        Ui.Value(panel, "TimeScale", "Tempo symulacji ×{0}", Pad, y, 13, N.ModelVar(hallId, N.TimeScale));
        y += 34;
        StopConfirm(panel, spec, Pad - 6, commandsY - 6);   // after the buttons: drawn on top of them

        Ui.Text(panel, "FaultsTitle", "Aktywne awarie", Pad, y, 16, N.TitleArgb, bold: true);
        Ui.Value(panel, "FaultsCount", "({0})", 150, y, 16, N.ModelVar(spec.Id, nameof(FM.Line.ActiveFaults)));
        y += 30;
        FaultList(panel, line, Pad, y);

    }

    /// <summary>Confirmation over the command buttons, visible while the line's stopRequest is set (Stop pressed).</summary>
    static void StopConfirm(IUANode panel, FX.LineSpec spec, double x, double y)
    {
        var box = Ui.Box(panel, "StopConfirm", x, y, 316, 150, N.WhiteArgb, N.TitleArgb, 2);
        Ui.Link(box.VisibleVariable, N.ModelVar(spec.Id, N.StopRequest));
        Ui.Text(box, "Question", "Zatrzymać linię?", 14, 10, 18, N.TitleArgb, bold: true);
        Ui.Text(box, "Detail", $"{spec.Name}: praca stacji zostanie przerwana.", 14, 40, 13, N.UnitsArgb);
        var yes = Ui.Button(box, "StopConfirmYes", "Zatrzymaj", 14, 90, 140);
        Ui.OnClickSet(yes, N.ModelVar(spec.Id, N.CmdStop), true);
        Ui.OnClickSet(yes, N.ModelVar(spec.Id, N.StopRequest), false);
        var no = Ui.Button(box, "StopConfirmNo", "Anuluj", 162, 90, 140);
        Ui.OnClickSet(no, N.ModelVar(spec.Id, N.StopRequest), false);
    }

    static void Command(IUANode parent, string name, string text, double x, double y, double w, string variablePath, object value)
    {
        var b = Ui.Button(parent, name, text, x, y, w);
        Ui.OnClickSet(b, variablePath, value);
    }

    /// <summary>One row per equipment, visible only while it is faulted (ColumnLayout collapses hidden rows).</summary>
    static void FaultList(IUANode panel, FM.Line line, double x, double y)
    {
        var list = InformationModel.Make<ColumnLayout>("FaultList");
        list.LeftMargin = (float)x;
        list.TopMargin = (float)y;
        list.Width = 304;
        list.VerticalGap = 6;
        panel.Add(list);

        foreach (var eq in line.Equipment)
        {
            var row = InformationModel.Make<Rectangle>("Fault_" + eq.Id);
            row.Width = 304;
            row.Height = 48;
            row.FillColor = new Color(N.WhiteArgb);
            row.BorderColor = new Color(N.LineArgb);
            row.BorderThickness = 1;
            list.Add(row);
            Ui.Link(row.VisibleVariable, N.ModelVar(eq.Path, nameof(FM.Equipment.FaultActive)));

            Ui.Box(row, "Marker", 0, 0, 8, 48, N.FaultArgb, N.FaultArgb, 0);
            var text = Ui.Value(row, "Text", "{0}", 16, 6, 12, N.ModelVar(eq.Path, nameof(FM.Equipment.FaultText)));
            text.Width = 196;
            text.WordWrap = true;
            text.TextColor = new Color(N.TitleArgb);
            var reset = Ui.Button(row, "Reset", "Kasuj", 218, 4, 80, 40);
            Ui.OnClickSet(reset, N.ModelVar(eq.Path, N.CmdReset), true);
        }
    }
}
