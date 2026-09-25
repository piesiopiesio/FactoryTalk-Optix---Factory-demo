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
    public int Units { get; set; } = 1;   // bottles contained (case = 12, pallet = 480)

    public bool IsGoodBottle => Filled && Capped && Labeled && !Defect;
}
