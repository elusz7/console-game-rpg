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
    public static int CalculateDamage(
        int incomingDamage,
        DamageType? damageType,
        int defense,
        int resistance)
    {
        ValidateInputs(incomingDamage, damageType);

        return damageType.Value switch
        {
            DamageType.Martial =>
                Math.Max(1, incomingDamage - defense),

            DamageType.Magical =>
                Math.Max(1, incomingDamage - resistance),

            DamageType.Hybrid =>
                Math.Max(1, incomingDamage -
                            (Math.Max(defense, resistance) / 2)),

            _ => throw new ArgumentOutOfRangeException(nameof(damageType))
        };
    }
    
    /// <summary>
    /// Validates incoming damage and its damage type.
    /// </summary>
    public static void ValidateInputs(int incomingDamage, DamageType? damageType)
    {
        if (incomingDamage <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(incomingDamage),
                "Incoming damage must be greater than zero.");

        if (damageType is null || !Enum.IsDefined(damageType.Value))
            throw new ArgumentOutOfRangeException(
                nameof(damageType),
                "A valid damage type is required.");
    }
}