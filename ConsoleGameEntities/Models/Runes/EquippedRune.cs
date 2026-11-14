using ConsoleGameEntities.Exceptions.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
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

    public int Use()
    {
        return InventoryRune.Rune.Power * Tier;
    }

    public bool ApplyStatusEffect()
    {
        return ApplyStatusEffect(Random.Shared.NextDouble());
    }

    internal bool ApplyStatusEffect(double roll)
    {
        if (roll is < 0.0 or >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(roll));

        var chance = InventoryRune.Rune.Rarity switch
        {
            RarityLevel.Common => 1.0,
            RarityLevel.Uncommon => 3.0,
            RarityLevel.Rare => 8.0,
            RarityLevel.Epic => 12.0,
            RarityLevel.Legendary => 20.0,
            RarityLevel.Mythic => 35.0,
            _ => throw new RuneException("Invalid Rune Rarity")
        };

        return roll < chance / 100.0;
    }
    
    public int GetDoTDamage()
    {
        return Math.Max(1, (InventoryRune.Rune.Power * Tier + 1) / 2);
    }
}