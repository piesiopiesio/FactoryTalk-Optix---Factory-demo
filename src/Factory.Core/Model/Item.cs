// @summary: A product unit flowing through the line (bottle, case or pallet) with quality flags.
#nullable enable
namespace Factory.Core.Model;

public enum ItemKind { Bottle, Case, Pallet }

public sealed class Item
{
    public long Id { get; init; }
    public ItemKind Kind { get; init; } = ItemKind.Bottle;
    public double FillMl { get; set; }
    public bool Filled { get; set; }
    public bool Capped { get; set; }
    public bool Labeled { get; set; }
    public bool Defect { get; set; }
    /// <summary>Id of the station that made the first defect (OEE quality loss is blamed on it).</summary>
    public string? DefectBy { get; private set; }
    public int Units { get; set; } = 1;   // bottles contained (case = 12, pallet = 480)

    public void MarkDefect(string stationId) { Defect = true; DefectBy ??= stationId; }

    public bool IsGoodBottle => Filled && Capped && Labeled && !Defect;
}
