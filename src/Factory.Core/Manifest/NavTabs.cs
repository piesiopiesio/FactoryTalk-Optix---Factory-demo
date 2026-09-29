// @summary: Order of the main navigation tabs (Hala, one per line, Alarmy, Trendy, Logowanie) and the tab index of each line screen.
#nullable enable
namespace Factory.Core.Manifest;

/// <summary>One tab of the main window: <see cref="Key"/> identifies the screen ("Hall", line id, "Alarms"...).</summary>
public sealed record NavTab(string Key, string Title);

public static class NavTabs
{
    public const string Hall = "Hall", Alarms = "Alarms", Trends = "Trends", Login = "Login";

    /// <summary>Tab order used by the Optix window and by hall tiles that jump to a line: hall first, lines in manifest order.</summary>
    public static IReadOnlyList<NavTab> Of(FactoryManifest m)
    {
        var tabs = new List<NavTab> { new(Hall, "Hala") };
        tabs.AddRange(m.Lines.Select(l => new NavTab(l.Id, l.Name)));
        tabs.Add(new(Alarms, "Alarmy"));
        tabs.Add(new(Trends, "Trendy"));
        tabs.Add(new(Login, "Logowanie"));
        return tabs;
    }

    /// <summary>Index for NavigationPanel.CurrentTabIndex that shows the given screen; throws for an unknown key.</summary>
    public static int IndexOf(FactoryManifest m, string key)
    {
        var tabs = Of(m);
        for (var i = 0; i < tabs.Count; i++)
            if (tabs[i].Key == key) return i;
        throw new ArgumentException($"No navigation tab for '{key}'");
    }
}
