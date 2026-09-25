// @summary: Creates one Optix ObjectType per Core class (stations, Conveyor, Line, Hall) with a variable per [Signal].
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

    public static Dictionary<Type, IUAObjectType> Build(IUANode folder)
    {
        var result = new Dictionary<Type, IUAObjectType>();
        foreach (var t in CoreTypes)
        {
            var type = InformationModel.MakeObjectType(t.Name);
            foreach (var s in FM.Signals.Of(t))
                type.Add(InformationModel.MakeVariable(OptixNames.Var(s.Name), OptixNames.DataType(s.Type)));
            if (t.GetProperty("State") != null)
                type.Add(InformationModel.MakeVariable(OptixNames.StateColorVar, OpcUa.DataTypes.UInt32));
            folder.Add(type);
            result[t] = type;
        }
        return result;
    }
}
