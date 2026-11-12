using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers;

/// <summary>
/// Resolves incoming attacks by determining dodge outcomes
/// and calculating damage.
/// </summary>
public class DamageResolver(Random? rng = null)
{
    private readonly Random _rng = rng ?? Random.Shared;

    /// <summary>
    /// Resolves an incoming attack against a target.
    /// </summary>
    public AttackResolutionResult ResolveMonsterDamage(AttackDamage attack, MonsterDamageModifiers modifiers, double dodgeChance)
    {
        DamageCalculator.ValidateInputs(attack);
        DamageCalculator.ValidateMonsterModifiers(modifiers);

        var chance = Math.Clamp(dodgeChance, 0, 33);
        var dodged = _rng.NextDouble() * 100 < chance;

        if (dodged)
        {
            return new AttackResolutionResult(
                attack.StandardDamage,
                0,
                attack.DamageType,
                attack.ElementalDamage,
                0,
                attack.Element,
                true,
                false);
        }

        var finalStandardDamage = attack.StandardDamage == 0
            ? 0
            : DamageCalculator.CalculateStandardDamage(
                attack.StandardDamage,
                attack.DamageType,
                modifiers.Defense,
                modifiers.Resistance);

        var finalElementalDamage = attack.ElementalDamage is 0
            ? 0
            : DamageCalculator.CalculateMonsterPrimordialDamage(
                attack.ElementalDamage,
                attack.Element!.Value,
                modifiers.Resilience,
                modifiers.Vulnverability);

        return new AttackResolutionResult(
            attack.StandardDamage,
            finalStandardDamage,
            attack.DamageType,
            attack.ElementalDamage,
            finalElementalDamage,
            attack.Element,
            false,
            false);
    }

    public AttackResolutionResult ResolvePlayerDamage(AttackDamage attack, PlayerDamageModifiers modifiers,
        double dodgeChance)
    {
        DamageCalculator.ValidateInputs(attack);
        DamageCalculator.ValidatePlayerModifiers(modifiers);

        var chance = Math.Clamp(dodgeChance, 0, 33);
        var dodged = _rng.NextDouble() * 100 < chance;

        if (dodged)
        {
            return new AttackResolutionResult(
                attack.StandardDamage,
                0,
                attack.DamageType,
                attack.ElementalDamage,
                0,
                attack.Element,
                true,
                false);
        }

        var finalStandardDamage = attack.StandardDamage == 0
            ? 0
            : DamageCalculator.CalculateStandardDamage(
                attack.StandardDamage,
                attack.DamageType,
                modifiers.Defense,
                modifiers.Resistance);
        
        var finalElementalDamage = attack.ElementalDamage is 0
            ? 0
            : DamageCalculator.CalculatePlayerPrimordialDamage(
                attack.ElementalDamage,
                modifiers.ArmorResilience);

        return new AttackResolutionResult(
            attack.StandardDamage,
            finalStandardDamage,
            attack.DamageType,
            attack.ElementalDamage,
            finalElementalDamage,
            attack.Element,
            false,
            false);
    }
}