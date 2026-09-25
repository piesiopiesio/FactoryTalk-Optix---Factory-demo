// @summary: Loads ProjectFiles/factory.json inside Optix (builds the Core Hall); design-time sync copies the repo-root manifest in.
#region Using directives
using System.IO;
using UAManagedCore;
using FTOptix.Core;
#endregion
using FX = Factory.Core.Manifest;
using FM = Factory.Core.Model;

public static class ManifestSource
{
    /// <summary>ProjectFiles/factory.json: what runtime (and a deployed panel) reads.</summary>
    public static string FilePath => ResourceUri.FromProjectRelativePath(OptixNames.ManifestFile).Uri;

    /// <summary>Repo layout: optix/OptixDemoFactory/ProjectFiles -> ../../../factory.json is the source of truth.</summary>
    public static string RepoPath => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(FilePath) ?? ".", "..", "..", "..", OptixNames.ManifestFile));

    /// <summary>Design time only: copy the repo manifest into ProjectFiles when it is newer.</summary>
    public static void SyncFromRepo()
    {
        if (!File.Exists(RepoPath)) return;
        if (File.Exists(FilePath) && File.GetLastWriteTimeUtc(FilePath) >= File.GetLastWriteTimeUtc(RepoPath)) return;
        File.Copy(RepoPath, FilePath, true);
        Log.Info("ManifestSource", $"Copied {RepoPath} -> {FilePath}");
    }

    public static (FX.FactoryManifest Manifest, FM.Hall Hall) Load()
    {
        var manifest = FX.FactoryLoader.Parse(File.ReadAllText(FilePath));
        return (manifest, FX.FactoryLoader.Build(manifest));
    }
}
