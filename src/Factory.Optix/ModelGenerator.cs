// @summary: Builds Model/Factory: Hall object + one object per line with its stations and conveyors (paths match SimEngine.Nodes()).
#region Using directives
using System;
using System.Collections.Generic;
using UAManagedCore;
using FTOptix.HMIProject;
#endregion
using FM = Factory.Core.Model;

public static class ModelGenerator
{
    public static void Build(IUANode root, FM.Hall hall, IReadOnlyDictionary<Type, IUAObjectType> types)
    {
        root.Add(InformationModel.MakeObject(hall.Id, types[typeof(FM.Hall)].NodeId));
        foreach (var line in hall.Lines)
        {
            var lineObj = InformationModel.MakeObject(line.Id, types[typeof(FM.Line)].NodeId);
            foreach (var eq in line.Equipment)
                lineObj.Add(InformationModel.MakeObject(eq.Id, types[eq.GetType()].NodeId));
            root.Add(lineObj);   // add the finished subtree once (faster, fewer async calls)
        }
    }
}
