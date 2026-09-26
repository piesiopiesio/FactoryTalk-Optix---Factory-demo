// @summary: Runtime side of HMI commands: polls cmdStart/cmdStop/cmdReset bits and Hall/timeScale, executes on SimEngine, clears bits.
#region Using directives
using System;
using System.Collections.Generic;
using UAManagedCore;
#endregion
using FM = Factory.Core.Model;
using FS = Factory.Core.Sim;

public sealed class CommandBits
{
    sealed class Bit
    {
        public IUAVariable Variable;
        public string Target;
        public FM.Cmd Cmd;
    }

    readonly List<Bit> bits = new List<Bit>();
    readonly IUAVariable timeScale;

    public CommandBits(IUANode modelRoot, FS.SimEngine engine)
    {
        var hall = engine.Hall;
        timeScale = modelRoot.GetVariable($"{hall.Id}/{OptixNames.TimeScale}");
        foreach (var line in hall.Lines)
        {
            Add(modelRoot, line.Id, OptixNames.CmdStart, FM.Cmd.Start);
            Add(modelRoot, line.Id, OptixNames.CmdStop, FM.Cmd.Stop);
            Add(modelRoot, line.Id, OptixNames.CmdReset, FM.Cmd.Reset);
            foreach (var eq in line.Equipment) Add(modelRoot, eq.Path, OptixNames.CmdReset, FM.Cmd.Reset);
            var stopRequest = modelRoot.GetVariable($"{line.Id}/{OptixNames.StopRequest}");
            if (stopRequest != null) stopRequest.Value = false;   // no stale Stop confirmation after a restart
        }
        Log.Info("CommandBits", $"{bits.Count} command bits watched");
    }

    void Add(IUANode root, string path, string name, FM.Cmd cmd)
    {
        var v = root.GetVariable($"{path}/{name}");
        if (v == null) { Log.Warning("CommandBits", $"Missing {path}/{name}; run FactoryBuilder.Build"); return; }
        v.Value = false;
        bits.Add(new Bit { Variable = v, Target = path, Cmd = cmd });
    }

    /// <summary>Called every tick under the simulation lock: execute raised bits, then clear them (handshake).</summary>
    public void Execute(FS.SimEngine engine)
    {
        foreach (var b in bits)
        {
            if (!(b.Variable.Value.Value is bool raised) || !raised) continue;
            engine.Command(b.Target, b.Cmd);
            b.Variable.Value = false;
            Log.Info("CommandBits", $"{b.Cmd} {b.Target}");
        }
    }

    /// <summary>Simulated seconds per real second, 1..20 (written by the HMI buttons).</summary>
    public int TimeScale
    {
        get
        {
            if (timeScale == null) return 1;
            var raw = timeScale.Value.Value;
            var v = raw is int i ? i : raw is uint u ? (int)u : 1;
            return Math.Max(1, Math.Min(20, v));
        }
    }
}
