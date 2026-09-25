// @summary: One DigitalAlarm per equipment in Alarms/Factory, linked to Model/Factory/<path>/faultActive; operator acknowledges.
#region Using directives
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.Alarm;
#endregion
using FM = Factory.Core.Model;

public static class AlarmGenerator
{
    public static void Build(IUANode folder, FM.Hall hall)
    {
        foreach (var line in hall.Lines)
            foreach (var eq in line.Equipment)
            {
                var source = Project.Current.GetVariable($"Model/{OptixNames.ModelFolder}/{eq.Path}/{OptixNames.Var(nameof(FM.Equipment.FaultActive))}");
                if (source == null) continue;
                var alarm = InformationModel.MakeObject<DigitalAlarm>($"{line.Id}_{eq.Id}_Fault");
                alarm.InputValueVariable.SetDynamicLink(source);
                alarm.Message = $"{line.Name} / {eq.Name}: awaria";
                alarm.AutoAcknowledge = false;   // operator acknowledges in the Alarms tab (ISA-18.2)
                alarm.AutoConfirm = false;
                folder.Add(alarm);
            }
    }
}
