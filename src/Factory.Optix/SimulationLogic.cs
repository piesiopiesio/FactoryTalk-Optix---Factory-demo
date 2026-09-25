// @summary: Runtime NetLogic (Model/Factory/SimulationLogic): ticks SimEngine every 100 ms, publishes signals, exposes commands.
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
using FM = Factory.Core.Model;
using FS = Factory.Core.Sim;

public class SimulationLogic : BaseNetLogic
{
    const int PeriodMs = 100;
    const double Dt = PeriodMs / 1000.0;

    readonly object sync = new object();
    FS.SimEngine engine;
    OptixBinder binder;
    PeriodicTask tick;
    int timeScale = 1;

    public override void Start()
    {
        try
        {
            var (_, hall) = ManifestSource.Load();
            engine = new FS.SimEngine(hall);
            binder = new OptixBinder(Project.Current.Get("Model/" + OptixNames.ModelFolder), engine);
            tick = new PeriodicTask(Step, PeriodMs, LogicObject);
            tick.Start();
            Log.Info("SimulationLogic", $"Started {hall.Lines.Count} line(s), seed {hall.Seed}");
        }
        catch (Exception ex)
        {
            Log.Error("SimulationLogic", "Start failed: " + ex.Message);
        }
    }

    public override void Stop()
    {
        tick?.Dispose();
        tick = null;
    }

    void Step()
    {
        lock (sync)
        {
            for (var i = 0; i < timeScale; i++) engine.Tick(Dt);
            binder.PublishChanged();
        }
    }

    [ExportMethod] public void StartLine(string lineId) => Command(lineId, FM.Cmd.Start);
    [ExportMethod] public void StopLine(string lineId) => Command(lineId, FM.Cmd.Stop);
    [ExportMethod] public void ResetFault(string equipmentPath) => Command(equipmentPath, FM.Cmd.Reset);

    /// <summary>1..20 simulated seconds per real second.</summary>
    [ExportMethod] public void SetTimeScale(int scale)
    {
        lock (sync) timeScale = Math.Max(1, Math.Min(20, scale));
    }

    void Command(string target, FM.Cmd cmd)
    {
        lock (sync)
        {
            if (engine == null || !engine.Command(target, cmd)) Log.Warning("SimulationLogic", $"Unknown target '{target}' for {cmd}");
        }
    }
}
