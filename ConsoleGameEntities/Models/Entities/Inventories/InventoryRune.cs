using ConsoleGameEntities.Models.Runes;

namespace ConsoleGameEntities.Models.Entities.Inventories;

public class InventoryRune
{
    public int InventoryId { get; set; }
    public virtual Inventory Inventory { get; set; } = null!;

    public int RuneId { get; set; }
    public virtual Rune Rune { get; set; } = null!;

    public int Tier { get; set; }
    public int Quantity { get; set; }
    
    public virtual List<EquippedRune> EquippedRunes { get; set; } = [];
}