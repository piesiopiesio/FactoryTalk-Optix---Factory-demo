// @summary: Design-time NetLogic: Build() regenerates types, Model/Factory instances, alarms and screen content from factory.json.
#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.NativeUI;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Core;
using FTOptix.NetLogic;
#endregion

public class FactoryBuilder : BaseNetLogic
{
    /// <summary>Idempotent: everything under */Factory is deleted and rebuilt. Hand-made UI lives in UI/Custom.</summary>
    [ExportMethod]
    public void Build()
    {
        try
        {
            ManifestSource.SyncFromRepo();
            var (manifest, hall) = ManifestSource.Load();
            var model = Project.Current.Get("Model");

            model.Get(OptixNames.ModelFolder)?.Delete();                // instances before their types
            var templates = NodeUtil.EnsureFolder(model, OptixNames.TypesFolder);
            var types = TypeGenerator.Build(NodeUtil.ResetFolder(templates, "Factory"));
            ModelGenerator.Build(NodeUtil.ResetFolder(model, OptixNames.ModelFolder), hall, types);

            var alarms = Project.Current.Get("Alarms");
            if (alarms != null) AlarmGenerator.Build(NodeUtil.ResetFolder(alarms, OptixNames.AlarmsFolder), hall);
            else Log.Warning("FactoryBuilder", "No Alarms folder in project; alarms skipped");

            ScreenGenerator.Build(manifest, hall);
            Log.Info("FactoryBuilder", $"Built {hall.Lines.Count} line(s), {types.Count} types from {ManifestSource.FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error("FactoryBuilder", "Build failed: " + ex.Message);
        }
    }

    [ExportMethod]
    public void Clean()
    {
        Project.Current.Get("Model/" + OptixNames.ModelFolder)?.Delete();
        Project.Current.Get("Model/" + OptixNames.TypesFolder + "/Factory")?.Delete();
        Project.Current.Get("Alarms/" + OptixNames.AlarmsFolder)?.Delete();
        foreach (var path in new[] { OptixNames.HallContent, OptixNames.LineContent })
        {
            var content = Project.Current.Get(path);
            if (content != null) NodeUtil.ClearChildren(content);
        }
        Log.Info("FactoryBuilder", "Generated nodes removed");
    }
}
