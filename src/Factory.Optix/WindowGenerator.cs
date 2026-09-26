// @summary: MainWindow chrome owned by the builder: background, header (plant KPIs + fault annunciator) and NavigationPanel tabs.
#region Using directives
using System;
using System.Collections.Generic;
using UAManagedCore;
using FTOptix.UI;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion
using FM = Factory.Core.Model;
using N = OptixNames;

public static class WindowGenerator
{
    const double HeaderH = 56;

    public static void Clear(IUANode window) => NodeUtil.DeleteChildren(window, N.WindowParts);

    public static void Build(IUANode window, FM.Hall hall, IReadOnlyList<(string Title, IUANode Screen)> tabs)
    {
        Clear(window);
        Ui.Background(window, "Background", N.BackgroundArgb);

        var header = InformationModel.Make<Rectangle>("Header");
        header.HorizontalAlignment = HorizontalAlignment.Stretch;
        header.Height = (float)HeaderH;
        header.FillColor = new Color(N.TabArgb);
        header.BorderColor = new Color(N.LineArgb);
        header.BorderThickness = 1;
        window.Add(header);

        Ui.Text(header, "Title", hall.Name, 20, 14, 22, N.TitleArgb, bold: true);
        Ui.Text(header, "Subtitle", "dane z symulacji", 250, 22, 13, N.UnitsArgb);
        // Template Library AlarmBanner: newest active alarm, rotates when several are active.
        LibraryViews.Instance(header, "AlarmBanner", N.LibAlarmBanner, 420, 8, 580, 40);
        var hallPath = hall.Id;
        Ui.Text(header, "OeeCaption", "Średnie OEE", 1040, 20, 14, N.TitleArgb);
        Ui.Value(header, "Oee", "{0} %", 1140, 16, 20, N.ModelVar(hallPath, nameof(FM.Hall.AverageOee)));
        Ui.Text(header, "LinesCaption", "Linie w pracy", 1300, 20, 14, N.TitleArgb);
        Ui.Value(header, "Lines", "{0} / " + hall.Lines.Count, 1410, 16, 20, N.ModelVar(hallPath, nameof(FM.Hall.ActiveLines)));

        // Fault annunciator: alarm color only when something is actually faulted (style guide).
        var ann = Ui.Box(header, "FaultLamp", 1560, 16, 24, 24, N.FaultArgb, N.TitleArgb);
        Ui.Expression(ann.VisibleVariable, "{0} > 0", N.ModelVar(hallPath, nameof(FM.Hall.ActiveFaults)));
        Ui.Text(header, "FaultsCaption", "Awarie", 1596, 20, 14, N.TitleArgb);
        Ui.Value(header, "Faults", "{0}", 1660, 16, 20, N.ModelVar(hallPath, nameof(FM.Hall.ActiveFaults)));

        // Template Library UsernameLabel: {Session}/User name ("Anonymous" until someone logs in on the Logowanie tab).
        Ui.Text(header, "UserCaption", "Użytkownik", 1720, 4, 12, N.UnitsArgb);
        LibraryViews.Instance(header, "UserName", N.LibUsernameLabel, 1720, 22, 180, 26);

        var nav = InformationModel.Make<NavigationPanel>("MainNav");
        nav.HorizontalAlignment = HorizontalAlignment.Stretch;
        nav.VerticalAlignment = VerticalAlignment.Stretch;
        nav.TopMargin = (float)HeaderH;
        window.Add(nav);
        for (var i = 0; i < tabs.Count; i++)
        {
            var item = InformationModel.Make<NavigationPanelItem>("Tab" + i);
            item.Title = tabs[i].Title;
            item.Panel = tabs[i].Screen.NodeId;
            nav.Panels.Add(item);
        }
        nav.CurrentTabIndex = 0;
    }
}
