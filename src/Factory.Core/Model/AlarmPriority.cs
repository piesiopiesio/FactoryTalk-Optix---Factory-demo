// @summary: Alarm priority 1-4 (ISA-18.2 / Rockwell HMI guide) per equipment type and its OPC UA Severity (1-1000) mapping.
namespace Factory.Core.Model;

/// <summary>1 = Pilny (safety, whole line down), 2 = Wysoki (bottleneck/product loss), 3 = Średni, 4 = Niski (consumables).</summary>
public enum AlarmPriority { Urgent = 1, High = 2, Medium = 3, Low = 4 }

public static class AlarmPriorities
{
    /// <summary>OPC UA Severity: one value inside each Optix band (see <see cref="OptixBand"/>), so the four
    /// priorities show as four different priorities in AlarmGrid (300/500 both fell into "Medium").</summary>
    public static ushort Severity(AlarmPriority p) => p switch
    {
        AlarmPriority.Urgent => 900,
        AlarmPriority.High => 700,
        AlarmPriority.Medium => 400,
        _ => 200,
    };

    /// <summary>Priority Optix shows for a Severity: 1-250 Low, 251-500 Medium, 501-750 High, 751-1000 Urgent.</summary>
    public static AlarmPriority OptixBand(ushort severity) => severity switch
    {
        > 750 => AlarmPriority.Urgent,
        > 500 => AlarmPriority.High,
        > 250 => AlarmPriority.Medium,
        _ => AlarmPriority.Low,
    };

    public static string Label(AlarmPriority p) => p switch
    {
        AlarmPriority.Urgent => "Pilny",
        AlarmPriority.High => "Wysoki",
        AlarmPriority.Medium => "Średni",
        _ => "Niski",
    };
}
