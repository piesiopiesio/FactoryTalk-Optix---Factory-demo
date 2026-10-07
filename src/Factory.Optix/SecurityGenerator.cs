// @summary: Demo accounts: design time creates groups Operatorzy/UtrzymanieRuchu + users (locale pl-PL) from demo-users.json; runtime sets their test passwords.
#region Using directives
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion

public static class SecurityGenerator
{
    public const string SeedFile = "demo-users.json";   // test accounts, never in the repo

    sealed class Seed { public SeedUser[] Users { get; set; } = Array.Empty<SeedUser>(); }
    sealed class SeedUser { public string Name { get; set; } = ""; public string Group { get; set; } = ""; public string Password { get; set; } = ""; }

    /// <summary>Master copy: project root, next to Factory_demo.optix.</summary>
    public static string SeedPath => Path.GetFullPath(Path.Combine(ManifestSource.ProjectFilesDir, "..", SeedFile));
    /// <summary>Copy in ProjectFiles, read by the runtime (Session.ChangePassword exists only at runtime, not in Studio).</summary>
    public static string RuntimeSeedPath => Path.Combine(ManifestSource.ProjectFilesDir, SeedFile);

    static Seed Read(string path) => File.Exists(path)
        ? JsonSerializer.Deserialize<Seed>(File.ReadAllText(path), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        : null;

    /// <summary>Design time, idempotent: missing groups/users are created and put in their group, every demo user gets locale pl-PL
    /// (Polish number format after login); seed copied to ProjectFiles.</summary>
    public static void Build()
    {
        var groups = Project.Current.Get("Security/Groups");
        var users = Project.Current.Get("Security/Users");
        if (groups == null || users == null) { Log.Warning("SecurityGenerator", "Security/Groups or Security/Users missing"); return; }
        foreach (var g in new[] { AccessLevels.OperatorGroup, AccessLevels.MaintenanceGroup })
            if (groups.Get(g) == null) groups.Add(InformationModel.MakeObject<Group>(g));

        var seed = Read(SeedPath);
        if (seed == null) { Log.Warning("SecurityGenerator", "No " + SeedPath + "; groups only"); return; }
        foreach (var u in seed.Users)
        {
            var user = users.Get(u.Name) as User;
            if (user == null) { user = InformationModel.MakeObject<User>(u.Name); users.Add(user); }
            user.LocaleId = OptixNames.Locale;   // session locale -> numbers "2 148", "74,6" (StringFormatter follows it)
            var group = groups.Get(u.Group);
            if (group != null && !user.Refs.GetObjects(FTOptix.Core.ReferenceTypes.HasGroup, false).Any(x => x.NodeId == group.NodeId))
                user.Refs.AddReference(FTOptix.Core.ReferenceTypes.HasGroup, group);
        }
        File.Copy(SeedPath, RuntimeSeedPath, true);
        Log.Info("SecurityGenerator", $"{seed.Users.Length} demo user(s); passwords are set when the runtime starts");
    }

    /// <summary>Runtime: set each seed password (old password empty = never set). Already set -> WrongOldPassword, harmless.</summary>
    public static void ApplyPasswords(Session session)
    {
        var seed = Read(RuntimeSeedPath);
        if (seed == null) return;
        foreach (var u in seed.Users)
            Log.Info("SecurityGenerator", $"Password {u.Name}: {session.ChangePassword(u.Name, u.Password, string.Empty).ResultCode}");
    }
}
