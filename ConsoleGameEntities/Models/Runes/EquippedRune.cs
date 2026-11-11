using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Runes;

public class EquippedRune
{
    public int Id { get; set; }
    public int InventoryId { get; set; }
    public int RuneId { get; set; }
    public int Tier { get; set; }
    
    public int? ArmorId { get; set; }
    public virtual Armor? Armor { get; set; }

    public int? WeaponId { get; set; }
    public virtual Weapon? Weapon { get; set; }
    public virtual InventoryRune InventoryRune { get; set; } = null!;
}