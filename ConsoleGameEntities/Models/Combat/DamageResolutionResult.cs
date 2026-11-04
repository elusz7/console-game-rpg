using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Models.Combat;

/// <summary>
/// Represents the outcome of resolving an incoming attack.
/// </summary>
public record DamageResolutionResult(
    int IncomingDamage,
    int FinalDamage,
    DamageType DamageType,
    bool IsDodged,
    bool IsCritical);