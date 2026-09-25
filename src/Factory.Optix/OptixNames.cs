// @summary: Optix-side naming: project paths, camelCase variable names, .NET -> OPC UA data type mapping, UI colors, screen scale.
#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
#endregion

public static class OptixNames
{
    public const string ModelFolder = "Factory";              // Model/Factory (instances, generated)
    public const string TypesFolder = "Templates";            // Model/Templates/Factory (types, generated)
    public const string AlarmsFolder = "Factory";             // Alarms/Factory (generated)
    public const string SimLogic = "SimulationLogic";         // Model/SimulationLogic (runtime NetLogic, created once)
    public const string ScreensFolder = "UI/Screens";
    public const string HallScreen = "HallScreen";            // generated
    public const string LineScreenPrefix = "LineScreen_";     // generated, one per manifest line
    public const string MainWindow = "UI/MainWindow";
    /// <summary>Children of MainWindow owned by FactoryBuilder; everything else in the window is left alone.</summary>
    public static readonly string[] WindowParts = { "Background", "Header", "MainNav" };
    public const string ManifestFile = "factory.json";        // ProjectFiles/factory.json
    public const string StateColorVar = "stateColor";
    /// <summary>HMI -> simulation command variables (PLC-style: HMI sets true, SimulationLogic executes and clears).
    /// Buttons use the built-in VariableCommands.Set, so no NetLogic method nodes are needed.</summary>
    public const string CmdStart = "cmdStart", CmdStop = "cmdStop", CmdReset = "cmdReset", TimeScale = "timeScale";

    // Rockwell Process HMI Style Guide (docs/hmi-style.md, design/theme.json).
    public const uint BackgroundArgb = 0xFFE0E0E0, GroupArgb = 0xFFE8E8E8, LineArgb = 0xFFA0A0A4, TitleArgb = 0xFF3F3F3F,
        DataArgb = 0xFF475CA7, UnitsArgb = 0xFF767676, TabArgb = 0xFFC0C0C0, SeparatorArgb = 0xFFD8D8D8,
        ButtonArgb = 0xFFC6C6C6, ButtonBorderArgb = 0xFFAAAAAA, DataBgArgb = 0xFFD4D4D4, FaultArgb = 0xFFEC8629, WhiteArgb = 0xFFFFFFFF;

    /// <summary>Logical manifest canvas (1600x900) -> screen pixels. The top band of the canvas (y &lt; OriginY) is the
    /// preview's title area; Optix has its own header, so it is cut off.</summary>
    public const double Scale = 1.2, OriginY = 120;
    public static double X(double logicalX) => logicalX * Scale;
    public static double Y(double logicalY) => (logicalY - OriginY) * Scale;
    public static double L(double logicalLength) => logicalLength * Scale;

    /// <summary>Model variables use camelCase (FactoryTalk Optix good practice).</summary>
    public static string Var(string signal) => char.ToLowerInvariant(signal[0]) + signal.Substring(1);

    /// <summary>Project path of a generated model variable: ModelVar("L1/FILL", "Good") = Model/Factory/L1/FILL/good.</summary>
    public static string ModelVar(string nodePath, string signal) => $"Model/{ModelFolder}/{nodePath}/{Var(signal)}";

    public static NodeId DataType(Type t)
    {
        if (t == typeof(bool)) return OpcUa.DataTypes.Boolean;
        if (t == typeof(int) || t.IsEnum) return OpcUa.DataTypes.Int32;
        if (t == typeof(long)) return OpcUa.DataTypes.Int64;
        if (t == typeof(uint)) return OpcUa.DataTypes.UInt32;
        if (t == typeof(double)) return OpcUa.DataTypes.Double;
        return OpcUa.DataTypes.String;
    }
}
