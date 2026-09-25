// @summary: Runtime bridge: writes changed [Signal] values (+ stateColor) from SimEngine into Model/Factory variables.
#region Using directives
using System;
using System.Collections.Generic;
using UAManagedCore;
#endregion
using FM = Factory.Core.Model;
using FS = Factory.Core.Sim;

public sealed class OptixBinder
{
    sealed class Binding
    {
        public IUAVariable Variable;
        public Func<object> Read;
        public object Last;
    }

    readonly List<Binding> bindings = new List<Binding>();

    public OptixBinder(IUANode modelRoot, FS.SimEngine engine)
    {
        foreach (var (path, node) in engine.Nodes())
        {
            var obj = modelRoot.Get(path);
            if (obj == null) { Log.Warning("OptixBinder", $"Missing Model/Factory/{path}; run FactoryBuilder.Build"); continue; }

            foreach (var signal in FM.Signals.Of(node.GetType()))
            {
                var variable = obj.GetVariable(OptixNames.Var(signal.Name));
                if (variable == null) continue;
                var s = signal; var n = node;
                bindings.Add(new Binding { Variable = variable, Read = () => s.Read(n) });
            }

            var stateProp = node.GetType().GetProperty("State");
            var colorVar = obj.GetVariable(OptixNames.StateColorVar);
            if (stateProp != null && colorVar != null)
            {
                var n = node;
                bindings.Add(new Binding { Variable = colorVar, Read = () => FM.StatePalette.Argb((FM.MachineState)stateProp.GetValue(n)) });
            }
        }
        Log.Info("OptixBinder", $"{bindings.Count} variables bound");
    }

    /// <summary>Write only values that changed since the last call (keeps OPC UA traffic low).</summary>
    public void PublishChanged()
    {
        foreach (var b in bindings)
        {
            var value = b.Read();
            if (Equals(value, b.Last)) continue;
            b.Variable.Value = ToUa(value);
            b.Last = value;
        }
    }

    static UAValue ToUa(object v) => v switch
    {
        bool x => x,
        int x => x,
        uint x => x,
        long x => x,
        double x => x,
        string x => x,
        _ => v.ToString(),
    };
}
