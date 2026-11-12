using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers;

/// <summary>
/// Calculates incoming damage after applying the target's defenses.
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// Calculates damage after applying the appropriate defense or resistance.
    /// </summary>
    public static int CalculateStandardDamage(
        int incomingDamage,
        DamageType damageType,
        int defense,
        int resistance)
    {
        return damageType switch //damageType is validated above
        {
            DamageType.Martial =>
                Math.Max(1, incomingDamage - defense),

            DamageType.Magical =>
                Math.Max(1, incomingDamage - resistance),

            _ =>
                Math.Max(1, incomingDamage -
                            Math.Max(defense, resistance) / 2)
        };
    }

    public static int CalculateMonsterPrimordialDamage(
        int incomingDamage, ElementType element,
        ElementType? resilience, ElementType? vulnverability)
    {
        if (resilience is not null && resilience.Value == element)
            return Math.Max(0, (int)(incomingDamage / 2.0));
        
        if (vulnverability is not null && vulnverability.Value == element)
            return (int)(incomingDamage * 1.5);

        return incomingDamage;
    }

    public static int CalculatePlayerPrimordialDamage(int incomingDamage, int resilience)
    {
        if (resilience > 0)
            return Math.Max(0, incomingDamage - resilience);
        
        return incomingDamage;
    }
    /// <summary>
    /// Validates incoming damage and its damage type.
    /// </summary>
    public static void ValidateInputs(AttackDamage attack)
    {
        if (attack.StandardDamage < 0)
            throw new ArgumentOutOfRangeException(
                nameof(attack), "Standard damage cannot be negative.");

        if (!Enum.IsDefined(attack.DamageType))
            throw new ArgumentOutOfRangeException(nameof(attack), "Physical damage type is not defined.");

        if (attack.ElementalDamage != 0)
        {
            if (attack.ElementalDamage < 0)
                throw new ArgumentOutOfRangeException(nameof(attack), "Elemental damage must be greater than zero.");

            if (attack.Element is null || !Enum.IsDefined(attack.Element.Value))
                throw new ArgumentOutOfRangeException(nameof(attack), "Element type is not defined.");
        }
        
        if (attack is { StandardDamage: 0, ElementalDamage: 0 })
            throw new ArgumentOutOfRangeException(
                nameof(attack), "At least one damage component must be greater than zero.");
    }

    public static void ValidateMonsterModifiers(MonsterDamageModifiers modifiers)
    {
        if (modifiers is null)
            throw new ArgumentNullException(nameof(modifiers));

        if (modifiers.Resilience is not null && !Enum.IsDefined(modifiers.Resilience.Value))
            throw new ArgumentOutOfRangeException(nameof(modifiers), "Resilience is not defined.");
        
        if (modifiers.Vulnverability is not null && !Enum.IsDefined(modifiers.Vulnverability.Value))
            throw new ArgumentOutOfRangeException(nameof(modifiers), "Vulnerability is not defined.");
    }

    public static void ValidatePlayerModifiers(PlayerDamageModifiers modifiers)
    {
        if (modifiers is null)
            throw new ArgumentNullException(nameof(modifiers));
    }
}