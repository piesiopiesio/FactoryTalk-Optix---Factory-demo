// @summary: Builds Model/Factory (Hall + lines with stations and conveyors, paths match SimEngine.Nodes()) and ensures Model/SimulationLogic.
#region Using directives
using System;
using System.Collections.Generic;
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.NetLogic;
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

    /// <summary>Runtime NetLogic node bound by name to class SimulationLogic. Created once, never deleted by Build.</summary>
    public static IUANode EnsureSimulationLogic(IUANode model)
    {
        var existing = model.Get(OptixNames.SimLogic);
        if (existing != null) return existing;
        var logic = InformationModel.MakeObject(OptixNames.SimLogic, FTOptix.NetLogic.ObjectTypes.NetLogic);
        model.Add(logic);
        return logic;
    }
}
