// @summary: Runtime UI NetLogic in MainWindow: enables commands/resets by the session user's level; hall tile click switches the tab.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.NetLogic;
using FTOptix.Core;
#endregion

/// <summary>Lives in MainWindow, created once and never regenerated: Studio replaces the class file of a NetLogic node that
/// FactoryBuilder deletes and re-creates with an empty template. The NavigationPanel shows the open tab as MainNav/&lt;Screen&gt;.</summary>
public class AccessLogic : BaseNetLogic
{
    PeriodicTask poll;

    public override void Start()
    {
        Apply();
        poll = new PeriodicTask(Apply, 500, LogicObject);   // user, open tab and hall tile clicks picked up within 0.5 s
        poll.Start();
    }

    public override void Stop()
    {
        poll?.Dispose();
        poll = null;
    }

    void Apply()
    {
        try
        {
            var nav = Owner.Get("MainNav");
            if (nav == null) return;
            var level = AccessLevels.Of(Session.User);
            int? requestedTab = null;
            foreach (var screen in nav.Children)
            {
                // Hall tile click writes the line's tab index into this session's HallScreen/openTab (-1 = nothing pending).
                if (screen.GetVariable(OptixNames.OpenTab) is IUAVariable open && open.Value.Value is int tab && tab >= 0)
                {
                    requestedTab = tab;
                    open.Value = -1;
                }
                var panel = screen.Get("LinePanel");
                if (panel == null) continue;
                foreach (var (path, min) in AccessLevels.Controls)
                    if (panel.Get(path) is Item item) item.Enabled = level >= min;
                if (panel.Get("FaultList") is IUANode list)
                    foreach (var row in list.Children)
                        if (row.Get("Reset") is Item reset) reset.Enabled = level >= AccessLevels.Maintenance;
                foreach (var detail in screen.Children.Where(c => c.BrowseName.StartsWith("Detail_")))
                    foreach (var name in new[] { "Reset", "InjectFault" })
                        if (detail.Get(name) is Item b) b.Enabled = level >= AccessLevels.Maintenance;
                if (panel.Get("AccessInfo") is Label info) info.Text = "Uprawnienia: " + AccessLevels.Describe(level);
            }
            if (requestedTab is int t && nav is NavigationPanel np) np.CurrentTabIndex = t;   // after the loop: switching replaces MainNav's child
        }
        catch (Exception ex)
        {
            Log.Warning("AccessLogic", ex.Message);
        }
    }
}
