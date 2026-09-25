// @summary: Screens built from Template Library widgets: Alarms (AlarmGrid + filtered history) and Trends (AdvancedTrend on DataLogger1).
#region Using directives
using System;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.UI;
#endregion
using N = OptixNames;

public static class LibraryViews
{
    /// <summary>Instance of a library type imported into the project; null (and a warning) when it was not imported.</summary>
    public static IUAObject Instance(IUANode parent, string name, string typePath, double x, double y, double w, double h)
    {
        var type = Project.Current.Get(typePath);
        if (type == null)
        {
            Log.Warning("LibraryViews", $"{typePath} not in project (import it from Template Library); {name} skipped");
            return null;
        }
        var obj = InformationModel.MakeObject(name, type.NodeId);
        parent.Add(obj);
        if (obj is Item item)
        {
            item.LeftMargin = (float)x;
            item.TopMargin = (float)y;
            item.Width = (float)w;
            item.Height = (float)h;
        }
        return obj;
    }

    /// <summary>Level 4 alarms screen: active alarms with Ack/Confirm on top, filtered history (AlarmsEventLogger1) below.</summary>
    public static void Alarms(IUANode screen)
    {
        Ui.Background(screen, "Background", N.BackgroundArgb);
        Ui.Text(screen, "Title", "Alarmy — aktywne", 24, 14, 20, N.TitleArgb, bold: true);
        Instance(screen, "ActiveAlarms", N.LibAlarmGrid, 24, 50, 1872, 420);

        Ui.Text(screen, "HistoryTitle", "Historia alarmów", 24, 490, 20, N.TitleArgb, bold: true);
        var history = Instance(screen, "AlarmHistory", N.LibAlarmHistory, 24, 526, 1872, 420);
        var logger = Project.Current.Get(N.AlarmLogger);
        if (history != null && logger != null) history.SetAlias("AlarmsEventLogger", logger.NodeId);
        else if (history != null) Log.Warning("LibraryViews", N.AlarmLogger + " missing; alarm history has no source");
    }

    /// <summary>Trends screen: AdvancedTrend bound to the factory DataLogger (pens are chosen at runtime in the widget).</summary>
    public static void Trends(IUANode screen)
    {
        Ui.Background(screen, "Background", N.BackgroundArgb);
        Ui.Text(screen, "Title", "Trendy — OEE, przepustowość, zbiornik, taśmy", 24, 14, 20, N.TitleArgb, bold: true);
        var trend = Instance(screen, "Trend", N.LibTrend, 24, 50, 1872, 900);
        var logger = Project.Current.Get(N.DataLogger);
        if (trend != null && logger != null) trend.GetVariable("Logger").Value = logger.NodeId;
    }
}
