using System.ComponentModel.DataAnnotations.Schema;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Runes;

public class WeaponRune : Rune
{
    public ElementDamageType Description => Element switch
    {
        ElementType.Fire => ElementDamageType.Burning,
        ElementType.Lightning => ElementDamageType.Static,
        ElementType.Ice => ElementDamageType.Frostbite,
        ElementType.Nature => ElementDamageType.Poisoned,
        ElementType.Radiance => ElementDamageType.Weakened,
        ElementType.Abyssal => ElementDamageType.Despair,
        _ => throw new ArgumentOutOfRangeException()
    };
    public ElementalStatusEffectType StatusEffect => Element switch
    {
        ElementType.Fire => ElementalStatusEffectType.Charred,
        ElementType.Lightning => ElementalStatusEffectType.Shocked,
        ElementType.Ice => ElementalStatusEffectType.Frozen,
        ElementType.Nature => ElementalStatusEffectType.Snared,
        ElementType.Radiance => ElementalStatusEffectType.Blinded,
        ElementType.Abyssal => ElementalStatusEffectType.Corrupted,
        _ => throw new ArgumentOutOfRangeException()
    };
    [NotMapped]
    public int ElapsedTime { get; set; }
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

    public (int Damage, bool StatusEffectApplied) Use()
    {
        int damage = Power * (Tier + 1);

        bool applyStatus = false;

        if (ElapsedTime > Duration)
        {
            if (Random.Shared.NextDouble() < StatusEffectChance / 100.0)
            {
                applyStatus = true;
            }
        }

        return (damage, applyStatus);
    }

    public bool ElapseTime()
    {
        ElapsedTime++;

        return ElapsedTime == Duration;
    }

    public void EnableEffect()
    {
        ElapsedTime = 0;
    }
}
