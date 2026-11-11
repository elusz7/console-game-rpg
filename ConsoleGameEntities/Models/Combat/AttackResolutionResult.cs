using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Models.Combat;

/// <summary>
/// Represents the outcome of resolving an incoming attack.
/// </summary>
public record AttackResolutionResult(
    int IncomingStandardDamage,
    int FinalStandardDamage,
    DamageType StandardDamageType,
    int? IncomingElementalDamage,
    int FinalElementalDamage,
    ElementType? Element,
    bool IsDodged,
    bool IsCritical);