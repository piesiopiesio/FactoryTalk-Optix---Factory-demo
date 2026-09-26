// @summary: Demo accounts: groups Operatorzy/UtrzymanieRuchu and users from <project>/demo-users.json (test passwords, not in the repo).
#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion

public static class SecurityGenerator
{
    public const string SeedFile = "demo-users.json";   // project root, next to Factory_demo.optix (not deployed)

    sealed class Seed { public SeedUser[] Users { get; set; } = Array.Empty<SeedUser>(); }
    sealed class SeedUser { public string Name { get; set; } = ""; public string Group { get; set; } = ""; public string Password { get; set; } = ""; }

    public static string SeedPath => Path.GetFullPath(Path.Combine(ManifestSource.ProjectFilesDir, "..", SeedFile));

    /// <summary>Idempotent: missing groups/users are created and put in their group. Returns (user, password) pairs
    /// for the caller to apply with Session.ChangePassword (needs a NetLogic session).</summary>
    public static List<(string Name, string Password)> Build()
    {
        var toApply = new List<(string Name, string Password)>();
        var groups = Project.Current.Get("Security/Groups");
        var users = Project.Current.Get("Security/Users");
        if (groups == null || users == null) { Log.Warning("SecurityGenerator", "Security/Groups or Security/Users missing"); return toApply; }
        foreach (var g in new[] { AccessLevels.OperatorGroup, AccessLevels.MaintenanceGroup })
            if (groups.Get(g) == null) groups.Add(InformationModel.MakeObject<Group>(g));

        if (!File.Exists(SeedPath)) { Log.Warning("SecurityGenerator", "No " + SeedPath + "; groups only"); return toApply; }
        var seed = JsonSerializer.Deserialize<Seed>(File.ReadAllText(SeedPath), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        foreach (var u in seed?.Users ?? Array.Empty<SeedUser>())
        {
            var user = users.Get(u.Name) as User;
            if (user == null) { user = InformationModel.MakeObject<User>(u.Name); users.Add(user); }
            var group = groups.Get(u.Group);
            if (group != null && !user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false).Any(x => x.NodeId == group.NodeId))
                user.Refs.AddReference(FTOptix.Core.ReferenceTypes.HasGroup, group);
            toApply.Add((u.Name, u.Password));
        }
        return toApply;
    }
}
