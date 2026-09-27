// @summary: Fills Loggers/DataLogger1 with Core's TrendPens: column = BrowseName (stable), readable pen name = DisplayName.
#region Using directives
using System;
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
        foreach (var pen in FM.TrendPens.Of(hall))
        {
            var source = Project.Current.GetVariable(N.ModelVar(pen.Path, pen.Signal.Name));
            if (source == null) continue;
            // BrowseName = DB column (unchanged, keeps history); DisplayName = pen name shown in the trend legend.
            var v = InformationModel.MakeVariable<VariableToLog>(pen.Column, N.DataType(pen.Signal.Type));
            v.DisplayName = new LocalizedText(pen.Label, N.Locale);
            list.Add(v);
            v.SetDynamicLink(source);
            count++;
        }
        Log.Info("LoggerGenerator", $"{count} variables logged by {N.DataLogger}");
    }
}
