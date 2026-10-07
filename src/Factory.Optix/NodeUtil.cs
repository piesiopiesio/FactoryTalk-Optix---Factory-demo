// @summary: Small node helpers for generators: ensure/reset folders, clear children, delete by name.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.Core;
#endregion

public static class NodeUtil
{
    public static IUANode EnsureFolder(IUANode parent, string name)
    {
        var existing = parent.Get(name);
        if (existing != null) return existing;
        var folder = InformationModel.Make<Folder>(name);
        parent.Add(folder);
        return folder;
    }

    /// <summary>Delete and recreate: generated folders are always rebuilt from factory.json.</summary>
    public static IUANode ResetFolder(IUANode parent, string name)
    {
        parent.Get(name)?.Delete();
        return EnsureFolder(parent, name);
    }

    public static void ClearChildren(IUANode node)
    {
        foreach (var child in node.Children.ToList()) child.Delete();
    }

    public static void DeleteChildren(IUANode parent, params string[] names)
    {
        foreach (var name in names) parent.Get(name)?.Delete();
    }
}
