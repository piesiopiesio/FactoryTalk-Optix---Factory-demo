// @summary: Runtime UI NetLogic under each LinePanel: enables commands by the session user's level (AccessLevels), per session.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.NetLogic;
using FTOptix.Core;
#endregion

public class AccessLogic : BaseNetLogic
{
    PeriodicTask poll;
    int applied = -1;

    public override void Start()
    {
        Apply();
        poll = new PeriodicTask(Apply, 500, LogicObject);   // cheap and robust: user changes are picked up within 0.5 s
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
            var level = AccessLevels.Of(Session.User);
            if (level == applied) return;
            applied = level;
            var panel = Owner;
            foreach (var (path, min) in AccessLevels.Controls)
                if (panel.Get(path) is Item item) item.Enabled = level >= min;
            if (panel.Get("FaultList") is IUANode list)
                foreach (var row in list.Children)
                    if (row.Get("Reset") is Item reset) reset.Enabled = level >= AccessLevels.Maintenance;
            if (panel.Owner != null)   // station faceplates on the same screen: reset = maintenance
                foreach (var detail in panel.Owner.Children.Where(c => c.BrowseName.StartsWith("Detail_")))
                    if (detail.Get("Reset") is Item reset) reset.Enabled = level >= AccessLevels.Maintenance;
            if (panel.Get("AccessInfo") is Label info) info.Text = "Uprawnienia: " + AccessLevels.Describe(level);
        }
        catch (Exception ex)
        {
            Log.Warning("AccessLogic", ex.Message);
        }
    }
}
