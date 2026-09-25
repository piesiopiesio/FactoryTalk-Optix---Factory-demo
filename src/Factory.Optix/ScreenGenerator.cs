// @summary: Regenerates the HMI from factory.json: HallScreen, LineScreen_<id> per line, Alarms/Trends screens, MainWindow chrome/tabs.
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
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

        var tabs = new List<(string Title, IUANode Screen)>();
        var hallScreen = NewScreen(screens, N.HallScreen);
        HallView.Build(hallScreen, manifest);
        tabs.Add(("Hala", hallScreen));

        foreach (var spec in manifest.Lines)
        {
            var line = hall.Lines.First(l => l.Id == spec.Id);
            var screen = NewScreen(screens, N.LineScreenPrefix + spec.Id);
            LineView.Build(screen, spec, line);
            StationTable.Build(screen, spec);
            LinePanel.Build(screen, spec, line, hall.Id);
            tabs.Add((spec.Name, screen));
        }

        var alarms = NewScreen(screens, N.AlarmsScreen);
        LibraryViews.Alarms(alarms);
        tabs.Add(("Alarmy", alarms));
        var trends = NewScreen(screens, N.TrendsScreen);
        LibraryViews.Trends(trends);
        tabs.Add(("Trendy", trends));

        WindowGenerator.Build(window, hall, tabs);
    }

    /// <summary>Removes only generated screens (HallScreen, LineScreen_*); hand-made screens stay.</summary>
    public static void Clean(IUANode screens)
    {
        foreach (var s in screens.Children.ToList())
            if (s.BrowseName == N.HallScreen || s.BrowseName == N.AlarmsScreen || s.BrowseName == N.TrendsScreen
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
