using ConsoleGameEntities.Exceptions.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Runes;

public sealed class EquippedRune
{
    public int Id { get; init; }
    public int InventoryId { get; init; }
    public int RuneId { get; init; }
    public int Tier { get; set; }
    
    public int? ArmorId { get; init; }
    public Armor? Armor { get; set; }

    public int? WeaponId { get; init; }
    public Weapon? Weapon { get; set; }
    public InventoryRune InventoryRune { get; init; } = null!;

    public int GetElementalAttack()
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

    public int GetStatusEffectPower()
    {
        return (int)InventoryRune.Rune.Rarity + Tier;
    }
    
    public (int dotDamage, ElementDamageType dotDamageType) GetDoTDamage()
    {
        return (Math.Max(1, (InventoryRune.Rune.Power * Tier + 1) / 2), GetElementDotType(InventoryRune.Rune.Element));
    }

    public ElementalStatusEffectType GetElementalStatusEffect()
    {
        return GetStatusEffectType(InventoryRune.Rune.Element);
    }
}