// @summary: Runtime UI NetLogic in MainWindow: enables line commands and faceplate resets by the session user's level (AccessLevels).
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
        poll = new PeriodicTask(Apply, 500, LogicObject);   // user and open tab changes picked up within 0.5 s
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
            foreach (var screen in nav.Children)
            {
                var panel = screen.Get("LinePanel");
                if (panel == null) continue;
                foreach (var (path, min) in AccessLevels.Controls)
                    if (panel.Get(path) is Item item) item.Enabled = level >= min;
                if (panel.Get("FaultList") is IUANode list)
                    foreach (var row in list.Children)
                        if (row.Get("Reset") is Item reset) reset.Enabled = level >= AccessLevels.Maintenance;
                foreach (var detail in screen.Children.Where(c => c.BrowseName.StartsWith("Detail_")))
                    if (detail.Get("Reset") is Item reset) reset.Enabled = level >= AccessLevels.Maintenance;
                if (panel.Get("AccessInfo") is Label info) info.Text = "Uprawnienia: " + AccessLevels.Describe(level);
            }
        }
        catch (Exception ex)
        {
            Log.Warning("AccessLogic", ex.Message);
        }
    }
}
