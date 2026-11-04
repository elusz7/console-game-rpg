using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers;

/// <summary>
/// Resolves incoming attacks by determining dodge outcomes
/// and calculating damage.
/// </summary>
public class DamageResolver
{
    private readonly Random _rng;

    public DamageResolver(Random? rng = null)
    {
        _rng = rng ?? Random.Shared;
    }

    /// <summary>
    /// Resolves an incoming attack against a target.
    /// </summary>
    public DamageResolutionResult ResolveDamage(
        int incomingDamage,
        DamageType? damageType,
        int defense,
        int resistance,
        double dodgeChance)
    {
        DamageCalculator.ValidateInputs(incomingDamage, damageType);

        var chance = Math.Clamp(dodgeChance, 0, 33);
        var dodged = _rng.NextDouble() * 100 < chance;

        if (dodged)
        {
            return new DamageResolutionResult(
                incomingDamage,
                0,
                damageType!.Value,
                true,
                false);
        }

        var finalDamage = DamageCalculator.CalculateDamage(
            incomingDamage,
            damageType,
            defense,
            resistance);

        return new DamageResolutionResult(
            incomingDamage,
            finalDamage,
            damageType!.Value,
            false,
            false);
    }
}