// @summary: Runtime NetLogic (Model/SimulationLogic): ticks SimEngine every 100 ms, publishes signals, executes HMI command bits, InjectFault method, sets demo passwords.
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
    const int MaxStepsPerCall = 200;   // 20 s of simulation per call at most; longer stalls are dropped (StepClock)

    readonly object sync = new object();
    FS.SimEngine engine;
    OptixBinder binder;
    CommandBits commands;
    PeriodicTask tick;
    readonly FS.StepClock clock = new FS.StepClock(Dt, MaxStepsPerCall);
    readonly System.Diagnostics.Stopwatch wall = new System.Diagnostics.Stopwatch();

    public override void Start()
    {
        try
        {
            var (_, hall) = ManifestSource.Load();
            engine = new FS.SimEngine(hall);
            var root = Project.Current.Get("Model/" + OptixNames.ModelFolder);
            binder = new OptixBinder(root, engine);
            commands = new CommandBits(root, engine);
            wall.Restart();
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

    /// <summary>Test fault on one device ("L1/FILL"): for scripts, the Studio method browser and remote demos.
    /// The faceplate button uses the cmdFault bit instead (same Core command).</summary>
    [ExportMethod]
    public void InjectFault(string path)
    {
        if (engine == null) return;
        lock (sync)
        {
            var ok = engine.Command(path, FM.Cmd.InjectFault);
            Log.Info("SimulationLogic", ok ? $"Injected fault on {path}" : $"InjectFault ignored: '{path}' is not a device");
        }
    }

    void Step()
    {
        lock (sync)
        {
            try
            {
                commands.Execute(engine);
                // Real period = 100 ms + run time (Optix help, PeriodicTask): steps follow the measured wall time.
                var elapsed = wall.Elapsed.TotalSeconds;
                wall.Restart();
                var n = clock.Steps(elapsed, commands.TimeScale);
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
