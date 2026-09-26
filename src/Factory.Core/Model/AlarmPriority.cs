// @summary: Alarm priority 1-4 (ISA-18.2 / Rockwell HMI guide) per equipment type and its OPC UA Severity (1-1000) mapping.
namespace Factory.Core.Model;

/// <summary>1 = Pilny (safety, whole line down), 2 = Wysoki (bottleneck/product loss), 3 = Średni, 4 = Niski (consumables).</summary>
public enum AlarmPriority { Urgent = 1, High = 2, Medium = 3, Low = 4 }

public static class AlarmPriorities
{
    /// <summary>OPC UA Severity: higher = more severe; bands spaced so filters like "Severity >= 500" stay simple.</summary>
    public static ushort Severity(AlarmPriority p) => p switch
    {
        AlarmPriority.Urgent => 900,
        AlarmPriority.High => 700,
        AlarmPriority.Medium => 500,
        _ => 300,
    };

    public static string Label(AlarmPriority p) => p switch
    {
        AlarmPriority.Urgent => "Pilny",
        AlarmPriority.High => "Wysoki",
        AlarmPriority.Medium => "Średni",
        _ => "Niski",
    };
}
