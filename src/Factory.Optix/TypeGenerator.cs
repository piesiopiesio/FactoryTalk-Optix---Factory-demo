// @summary: Creates one Optix ObjectType per Core class (FillerType, ConveyorType, LineType, HallType...) with a variable per [Signal].
#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
#endregion
using FM = Factory.Core.Model;
using FX = Factory.Core.Manifest;

public static class TypeGenerator
{
    public static IEnumerable<Type> CoreTypes =>
        FX.StationRegistry.Types.Values.OrderBy(t => t.Name)
            .Concat(new[] { typeof(FM.Conveyor), typeof(FM.Line), typeof(FM.Hall) });

    /// <summary>"Type" suffix is required: Studio generates a global C# proxy class per project ObjectType, and a proxy
    /// named "Hall"/"Line"/"Conveyor" would shadow the Core classes (global types win over using directives).</summary>
    public static string TypeName(Type t) => t.Name + "Type";

    /// <summary>Command bits per node type: lines start/stop/reset, every device can be reset or get a test fault.</summary>
    public static IEnumerable<string> Commands(Type t) =>
        t == typeof(FM.Line) ? new[] { OptixNames.CmdStart, OptixNames.CmdStop, OptixNames.CmdReset }
        : typeof(FM.Equipment).IsAssignableFrom(t) ? new[] { OptixNames.CmdReset, OptixNames.CmdFault }
        : Array.Empty<string>();

    public static Dictionary<Type, IUAObjectType> Build(IUANode folder)
    {
        var result = new Dictionary<Type, IUAObjectType>();
        foreach (var t in CoreTypes)
        {
            var type = InformationModel.MakeObjectType(TypeName(t));
            foreach (var s in FM.Signals.Of(t))
                type.Add(InformationModel.MakeVariable(OptixNames.Var(s.Name), OptixNames.DataType(s.Type)));
            if (t.GetProperty("State") != null)
                type.Add(InformationModel.MakeVariable(OptixNames.StateColorVar, OpcUa.DataTypes.UInt32));
            foreach (var cmd in Commands(t))
                type.Add(InformationModel.MakeVariable(cmd, OpcUa.DataTypes.Boolean));
            if (t == typeof(FM.Hall))
            {
                var scale = InformationModel.MakeVariable(OptixNames.TimeScale, OpcUa.DataTypes.Int32);
                scale.Value = 1;
                type.Add(scale);
            }
            folder.Add(type);
            result[t] = type;
        }
        return result;
    }
}
