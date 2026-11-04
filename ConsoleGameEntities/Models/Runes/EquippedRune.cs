using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Runes;

public class EquippedRune
{
    public int Id { get; set; }
    public int? ItemId { get; set; }
    
    public int InventoryId { get; set; }
    public int RuneId { get; set; }
    public int Tier { get; set; }
    
    public virtual Item Item { get; set; } = null!;
    public virtual InventoryRune InventoryRune { get; set; } = null!;
}