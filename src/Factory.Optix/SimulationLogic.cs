// @summary: Runtime NetLogic (Model/SimulationLogic): ticks SimEngine every 100 ms, publishes signals, executes HMI command bits, sets demo passwords.
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
    CommandBits commands;
    PeriodicTask tick;

    public override void Start()
    {
        try
        {
            var (_, hall) = ManifestSource.Load();
            engine = new FS.SimEngine(hall);
            var root = Project.Current.Get("Model/" + OptixNames.ModelFolder);
            binder = new OptixBinder(root, engine);
            commands = new CommandBits(root, engine);
            tick = new PeriodicTask(Step, PeriodMs, LogicObject);
            tick.Start();
            Log.Info("SimulationLogic", $"Started {hall.Lines.Count} line(s), seed {hall.Seed}");
        }
        catch (Exception ex)
        {
            Log.Error("SimulationLogic", "Start failed: " + ex.Message);
        }
        try { SecurityGenerator.ApplyPasswords(Session); }   // demo accounts (ProjectFiles/demo-users.json)
        catch (Exception ex) { Log.Warning("SimulationLogic", "Demo passwords not set: " + ex.Message); }
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
            try
            {
                commands.Execute(engine);
                var n = commands.TimeScale;
                for (var i = 0; i < n; i++) engine.Tick(Dt);
                binder.PublishChanged();
            }
            catch (Exception ex)
            {
                Log.Error("SimulationLogic", "Step failed: " + ex.Message);
            }
        }
    }
}
