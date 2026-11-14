using System.ComponentModel.DataAnnotations.Schema;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes;

public class WeaponRune : Rune
{
    [NotMapped]
    public virtual EquippedRune? EquippedRune { get; set; }
    
    public int Duration { get; set; }

    public double StatusEffectChance => Rarity switch
    {
        RarityLevel.Common => 1.0,
        RarityLevel.Uncommon => 3.0,
        RarityLevel.Rare => 8.0,
        RarityLevel.Epic => 12.0,
        RarityLevel.Legendary => 20.0,
        RarityLevel.Mythic => 35.0,
        _ => 0.0
    };

    public (int Damage, bool StatusEffectApplied) Use(int tier)
    {
        var damage = Power * tier;

        var applyStatus = Random.Shared.NextDouble() < (StatusEffectChance / 100.0);

        return (damage, applyStatus);
    }
}