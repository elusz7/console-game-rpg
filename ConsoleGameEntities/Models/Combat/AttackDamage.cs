using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Combat;

public record AttackDamage(
    int PhysicalDamage,
    DamageType DamageType,
    int ElementalDamage,
    ElementType? Element);