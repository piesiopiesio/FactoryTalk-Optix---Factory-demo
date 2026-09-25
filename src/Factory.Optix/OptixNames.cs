// @summary: Optix-side naming: project paths, camelCase variable names, .NET -> OPC UA data type mapping, UI colors.
#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
#endregion

public static class OptixNames
{
    public const string ModelFolder = "Factory";              // Model/Factory
    public const string TypesFolder = "Templates";            // Model/Templates/Factory
    public const string AlarmsFolder = "Factory";             // Alarms/Factory
    public const string HallContent = "UI/Screens/HallScreen/FactoryContent";
    public const string LineContent = "UI/Screens/LineScreen/FactoryContent";
    public const string ManifestFile = "factory.json";        // ProjectFiles/factory.json
    public const string StateColorVar = "stateColor";

    // Rockwell Process HMI Style Guide (docs/hmi-style.md): background, group box, equipment line, title, live data, units.
    public const uint BackgroundArgb = 0xFFE0E0E0, GroupArgb = 0xFFE8E8E8, LineArgb = 0xFFA0A0A4, TitleArgb = 0xFF3F3F3F,
        DataArgb = 0xFF475CA7, UnitsArgb = 0xFF767676;

    /// <summary>Model variables use camelCase (FactoryTalk Optix good practice).</summary>
    public static string Var(string signal) => char.ToLowerInvariant(signal[0]) + signal.Substring(1);

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
