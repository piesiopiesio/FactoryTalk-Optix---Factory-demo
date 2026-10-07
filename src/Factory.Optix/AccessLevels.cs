// @summary: Operator access model: level from the session user's groups (0 anonymous, 1 Operatorzy, 2 UtrzymanieRuchu) + control thresholds.
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
#endregion

public static class AccessLevels
{
    public const string OperatorGroup = "Operatorzy", MaintenanceGroup = "UtrzymanieRuchu";
    public const int None = 0, Operator = 1, Maintenance = 2;

    /// <summary>Line panel controls (path under LinePanel) and the minimum level that enables them.</summary>
    public static readonly (string Path, int MinLevel)[] Controls =
    {
        ("Start", Operator), ("Stop", Operator), ("Time1", Operator), ("Time10", Operator),
        ("StopConfirm/StopConfirmYes", Operator), ("Reset", Maintenance),
    };

    /// <summary>Highest level granted by the user's groups; Anonymous or no groups = 0.</summary>
    public static int Of(IUANode user)
    {
        if (user == null) return None;
        var groups = user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false).Select(g => g.BrowseName).ToList();
        if (groups.Contains(MaintenanceGroup)) return Maintenance;
        if (groups.Contains(OperatorGroup)) return Operator;
        return None;
    }

    public static string Describe(int level) => level switch
    {
        Maintenance => "Utrzymanie ruchu",
        Operator => "Operator",
        _ => "tylko podgląd — zaloguj się",
    };
}
