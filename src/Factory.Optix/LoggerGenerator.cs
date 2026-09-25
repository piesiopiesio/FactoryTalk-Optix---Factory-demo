// @summary: Fills Loggers/DataLogger1 with the KPI signals worth trending: line doubles, stations' own doubles, belt occupancy.
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.DataLogger;
#endregion
using FM = Factory.Core.Model;
using N = OptixNames;

public static class LoggerGenerator
{
    public static void Build(FM.Hall hall)
    {
        var logger = Project.Current.Get<DataLogger>(N.DataLogger);
        if (logger == null)
        {
            Log.Warning("LoggerGenerator", N.DataLogger + " missing (create it in Studio: Loggers > New > Data logger); trends skipped");
            return;
        }
        var list = logger.Get("VariablesToLog");
        NodeUtil.ClearChildren(list);
        var count = 0;
        foreach (var (path, signal) in Selection(hall))
        {
            var source = Project.Current.GetVariable(N.ModelVar(path, signal.Name));
            if (source == null) continue;
            var v = InformationModel.MakeVariable<VariableToLog>(path.Replace('/', '_') + "_" + N.Var(signal.Name), N.DataType(signal.Type));
            list.Add(v);
            v.SetDynamicLink(source);
            count++;
        }
        Log.Info("LoggerGenerator", $"{count} variables logged by {N.DataLogger}");
    }

    /// <summary>Line KPIs (all doubles), each station's own doubles (tank, torque...), belt occupancy.</summary>
    static IEnumerable<(string Path, FM.SignalInfo Signal)> Selection(FM.Hall hall)
    {
        foreach (var line in hall.Lines)
        {
            foreach (var s in FM.Signals.Of(typeof(FM.Line)).Where(s => s.Type == typeof(double)))
                yield return (line.Id, s);
            foreach (var st in line.Stations)
                foreach (var s in FM.Signals.Of(st.GetType()).Where(s => s.Type == typeof(double) && s.Property.DeclaringType == st.GetType()))
                    yield return (st.Path, s);
            foreach (var c in line.Conveyors)
                foreach (var s in FM.Signals.Of(typeof(FM.Conveyor)).Where(s => s.Name == nameof(FM.Conveyor.Occupancy)))
                    yield return (c.Path, s);
        }
    }
}
