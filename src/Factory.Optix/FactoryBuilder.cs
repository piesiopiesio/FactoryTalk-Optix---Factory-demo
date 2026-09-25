// @summary: Design-time NetLogic: Build() regenerates types, Model/Factory, alarms, screens and window tabs from factory.json; Clean() removes them.
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
using N = OptixNames;

public class FactoryBuilder : BaseNetLogic
{
    /// <summary>Idempotent: generated nodes are deleted and rebuilt. Hand-made UI lives in UI/Custom (untouched).
    /// Run from Studio: right-click NetLogic/FactoryBuilder -> Execute Build (must run on Studio's UI thread).</summary>
    [ExportMethod]
    public void Build()
    {
        try
        {
            ManifestSource.SyncFromRepo();
            var (manifest, hall) = ManifestSource.Load();
            var model = Project.Current.Get("Model");

            // Screens and window tabs link to model variables: remove them before the model is replaced.
            var window = Project.Current.Get(N.MainWindow);
            if (window != null) WindowGenerator.Clear(window);
            var screens = Project.Current.Get(N.ScreensFolder);
            if (screens != null) ScreenGenerator.Clean(screens);
            Project.Current.Get("Alarms/" + N.AlarmsFolder)?.Delete();

            model.Get(N.ModelFolder)?.Delete();                        // instances before their types
            var templates = NodeUtil.EnsureFolder(model, N.TypesFolder);
            var types = TypeGenerator.Build(NodeUtil.ResetFolder(templates, "Factory"));
            ModelGenerator.Build(NodeUtil.ResetFolder(model, N.ModelFolder), hall, types);
            ModelGenerator.EnsureSimulationLogic(model);
            LoggerGenerator.Build(hall);

            var alarms = Project.Current.Get("Alarms");
            if (alarms != null) AlarmGenerator.Build(NodeUtil.ResetFolder(alarms, N.AlarmsFolder), hall);
            else Log.Warning("FactoryBuilder", "No Alarms folder in project; alarms skipped");

            ScreenGenerator.Build(manifest, hall);
            Log.Info("FactoryBuilder", $"Built {hall.Lines.Count} line(s), {types.Count} types, screens + tabs from {ManifestSource.FilePath}");
        }
        catch (Exception ex)
        {
            Log.Error("FactoryBuilder", "Build failed: " + ex);
        }
    }

    [ExportMethod]
    public void Clean()
    {
        var window = Project.Current.Get(N.MainWindow);
        if (window != null) WindowGenerator.Clear(window);
        var screens = Project.Current.Get(N.ScreensFolder);
        if (screens != null) ScreenGenerator.Clean(screens);
        Project.Current.Get("Alarms/" + N.AlarmsFolder)?.Delete();
        Project.Current.Get("Model/" + N.ModelFolder)?.Delete();
        Project.Current.Get("Model/" + N.TypesFolder + "/Factory")?.Delete();
        Log.Info("FactoryBuilder", "Generated nodes removed (Model/SimulationLogic kept)");
    }
}
