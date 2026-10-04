// @summary: Machine states + shared color palette (used by Optix binder and Design preview).
#nullable enable
namespace Factory.Core.Model;

public enum MachineState { Stopped = 0, Running = 1, Starved = 2, Blocked = 3, Faulted = 4, Maintenance = 5 }

/// <summary>InjectFault = test/demo fault on one device (path "L1/FILL"); ignored for a whole line or the hall.</summary>
public enum Cmd { Start, Stop, Reset, InjectFault }

public static class StatePalette
{
    /// <summary>ARGB per state, Rockwell Process HMI Style Guide (ISA-101): calm colors for normal states,
    /// alarm color only for Faulted. Starved/Blocked/Maintenance share "transition" blue and differ by text.
    /// Keep in sync with design/theme.json (checked by tests).</summary>
    public static uint Argb(MachineState s) => s switch
    {
        MachineState.Running => 0xFFF0F0F0u,
        MachineState.Starved => 0xFF93C2E4u,
        MachineState.Blocked => 0xFF93C2E4u,
        MachineState.Faulted => 0xFFEC8629u,
        MachineState.Maintenance => 0xFF93C2E4u,
        _ => 0xFF808080u,
    };

    public static string Hex(MachineState s) => "#" + (Argb(s) & 0xFFFFFF).ToString("X6");

    /// <summary>Operator text for a state (style guide: state is never shown by color alone).</summary>
    public static string Label(MachineState s) => s switch
    {
        MachineState.Running => "Praca",
        MachineState.Starved => "Brak materiału",
        MachineState.Blocked => "Zablokowana",
        MachineState.Faulted => "Awaria",
        MachineState.Maintenance => "Obsługa",
        _ => "Zatrzymana",
    };
}
