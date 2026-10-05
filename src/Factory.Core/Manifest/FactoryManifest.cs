// @summary: DTOs mirroring factory.json (camelCase JSON). Pure data, no behavior.
#nullable enable
namespace Factory.Core.Manifest;

public sealed class FactoryManifest
{
    public int Version { get; set; } = 1;
    public HallSpec Hall { get; set; } = new();
    public List<LineSpec> Lines { get; set; } = new();
}

public sealed class HallSpec
{
    public string Id { get; set; } = "Hall";
    public string Name { get; set; } = "";
    public int[] Size { get; set; } = { 1600, 900 };
    public int Seed { get; set; } = 1;
    public List<ZoneSpec> Zones { get; set; } = new();
}

public sealed class ZoneSpec
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int[] Rect { get; set; } = { 0, 0, 0, 0 };
    public bool Planned { get; set; }
}

public sealed class LineSpec
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Zone { get; set; } = "";
    public int[] Rect { get; set; } = { 0, 0, 0, 0 };
    public bool AutoStart { get; set; } = true;
    public string OeeStation { get; set; } = "";
    public ProductSpec Product { get; set; } = new();
    public KpiTargets KpiTargets { get; set; } = new();
    public List<StationSpec> Stations { get; set; } = new();
    public List<ConveyorSpec> Conveyors { get; set; } = new();
}

public sealed class ProductSpec
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int UnitsPerCase { get; set; } = 12;
    public int CasesPerPallet { get; set; } = 40;
}

public sealed class KpiTargets
{
    public double[] Oee { get; set; } = { 0, 1 };
    public double[] ThroughputPerMin { get; set; } = { 0, double.MaxValue };
}

public sealed class StationSpec
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public int[] Pos { get; set; } = { 0, 0 };
    public double CycleS { get; set; } = 1;
    public FaultSpec? Fault { get; set; }
    public MicroStopSpec? MicroStop { get; set; }
    public Dictionary<string, double> Params { get; set; } = new();
}

public sealed class ConveyorSpec
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    public double LengthM { get; set; } = 2;
    public double SpeedMps { get; set; } = 0.4;
    public double PitchM { get; set; } = 0.1;
    public FaultSpec? Fault { get; set; }
}

public sealed class FaultSpec
{
    public double MtbfS { get; set; }
    public double MttrS { get; set; }
    public bool AutoRecover { get; set; } = true;
}

/// <summary>Short stops (< 30 s) counted as Performance loss: mean running time between stops and mean duration.</summary>
public sealed class MicroStopSpec
{
    public double MtbsS { get; set; }
    public double MeanS { get; set; }
}
