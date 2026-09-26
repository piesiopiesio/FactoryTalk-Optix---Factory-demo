// @summary: Screens built from Template Library widgets: Alarms (AlarmGrid + history), Trends (AdvancedTrend), Login (LoginForm).
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

    /// <summary>Login tab: Template Library LoginForm (login / logout / change password) on Security/Users + role legend.</summary>
    public static void Login(IUANode screen)
    {
        Ui.Background(screen, "Background", N.BackgroundArgb);
        Ui.Text(screen, "Title", "Logowanie", 24, 14, 20, N.TitleArgb, bold: true);
        var box = Ui.Box(screen, "FormGroup", 24, 50, 360, 360, N.GroupArgb, N.LineArgb);
        var form = Instance(box, "LoginForm", N.LibLoginForm, 30, 30, 300, 300);
        var users = Project.Current.Get(N.UsersFolder);
        if (form != null && users != null) form.GetVariable("Users").Value = users.NodeId;

        var roles = Ui.Box(screen, "Roles", 408, 50, 560, 200, N.GroupArgb, N.LineArgb);
        Ui.Text(roles, "Title", "Uprawnienia", 16, 12, 16, N.TitleArgb, bold: true);
        Ui.Text(roles, "None", "Bez logowania: tylko podgląd (przyciski linii nieaktywne).", 16, 48, 14, N.TitleArgb);
        Ui.Text(roles, "Operator", "Operator (grupa Operatorzy): Start, Stop z potwierdzeniem, tempo symulacji.", 16, 80, 14, N.TitleArgb);
        Ui.Text(roles, "Maintenance", "Utrzymanie ruchu (grupa UtrzymanieRuchu): jak operator + kasowanie awarii.", 16, 112, 14, N.TitleArgb);
        Ui.Text(roles, "Accounts", "Konta demo: operator, serwis (hasła w pamięci projektu Claude).", 16, 156, 13, N.UnitsArgb);
    }
}
