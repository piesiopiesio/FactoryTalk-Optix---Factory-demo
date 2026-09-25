// @summary: Small node helpers for generators: ensure/reset folders, clear children, typed UI element factories.
#region Using directives
using System;
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.UI;
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

    public static Rectangle Box(string name, double x, double y, double w, double h, uint fill, double scale)
    {
        var r = InformationModel.Make<Rectangle>(name);
        r.LeftMargin = (float)(x * scale);
        r.TopMargin = (float)(y * scale);
        r.Width = (float)(w * scale);
        r.Height = (float)(h * scale);
        r.FillColor = new Color(fill);
        r.CornerRadius = 2;
        return r;
    }

    public static Label Text(string name, string text, double x, double y, float size, uint color, double scale)
    {
        var l = InformationModel.Make<Label>(name);
        l.Text = text;
        l.LeftMargin = (float)(x * scale);
        l.TopMargin = (float)(y * scale);
        l.FontSize = size;
        l.TextColor = new Color(color);
        return l;
    }
}
