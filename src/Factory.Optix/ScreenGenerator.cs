// @summary: Regenerates the HMI from factory.json: HallScreen, LineScreen_<id> per line, Alarms/Trends/Login screens, MainWindow chrome/tabs.
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;
using N = OptixNames;

public static class ScreenGenerator
{
    public static void Build(FX.FactoryManifest manifest, FM.Hall hall)
    {
        var window = Project.Current.Get(N.MainWindow);
        var screens = Project.Current.Get(N.ScreensFolder);
        if (window == null || screens == null)
        {
            Log.Warning("ScreenGenerator", "UI/MainWindow or UI/Screens missing; screens skipped");
            return;
        }
        WindowGenerator.Clear(window);   // tabs point at the screens: drop them before the screens are replaced
        Clean(screens);

        var titles = FX.NavTabs.Of(manifest).ToDictionary(t => t.Key, t => t.Title);   // one tab order for window + hall tiles
        var tabs = new List<(string Title, IUANode Screen)>();
        var hallScreen = NewScreen(screens, N.HallScreen);
        HallView.Build(hallScreen, manifest);
        tabs.Add((titles[FX.NavTabs.Hall], hallScreen));

        foreach (var spec in manifest.Lines)
        {
            var line = hall.Lines.First(l => l.Id == spec.Id);
            var screen = NewScreen(screens, N.LineScreen(spec.Id));
            // HMI-only state in the screen, not in Model: every web client has its own session and screen instance (Optix help),
            // so one operator opening a faceplate or the Stop confirmation does not open it for everybody.
            State(screen, N.StopRequest, OpcUa.DataTypes.Boolean, false);
            State(screen, N.SelectedStation, OpcUa.DataTypes.Int32, 0);
            LineView.Build(screen, spec, line);
            StationTable.Build(screen, spec);
            LinePanel.Build(screen, spec, line, hall.Id);
            StationDetail.Build(screen, spec, line);   // last: drawn on top of the diagram
            tabs.Add((titles[spec.Id], screen));
        }

        var alarms = NewScreen(screens, N.AlarmsScreen);
        LibraryViews.Alarms(alarms);
        tabs.Add((titles[FX.NavTabs.Alarms], alarms));
        var trends = NewScreen(screens, N.TrendsScreen);
        LibraryViews.Trends(trends);
        tabs.Add((titles[FX.NavTabs.Trends], trends));
        var login = NewScreen(screens, N.LoginScreen);
        LibraryViews.Login(login);
        tabs.Add((titles[FX.NavTabs.Login], login));

        WindowGenerator.Build(window, hall, tabs);
    }

    static void State(IUANode screen, string name, NodeId dataType, UAValue initial)
    {
        var v = InformationModel.MakeVariable(name, dataType);
        v.Value = initial;
        screen.Add(v);
    }

    /// <summary>Removes only generated screens (HallScreen, LineScreen_*); hand-made screens stay.</summary>
    public static void Clean(IUANode screens)
    {
        foreach (var s in screens.Children.ToList())
            if (s.BrowseName == N.HallScreen || s.BrowseName == N.AlarmsScreen || s.BrowseName == N.TrendsScreen || s.BrowseName == N.LoginScreen
                || s.BrowseName.StartsWith(N.LineScreenPrefix)) s.Delete();
    }

    static IUANode NewScreen(IUANode folder, string name)
    {
        var screen = InformationModel.MakeObjectType<ScreenType>(name);
        screen.HorizontalAlignment = HorizontalAlignment.Stretch;
        screen.VerticalAlignment = VerticalAlignment.Stretch;
        folder.Add(screen);
        return screen;
    }
}
