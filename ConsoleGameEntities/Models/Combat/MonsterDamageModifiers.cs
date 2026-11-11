using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Combat;

public record MonsterDamageModifiers(
    int Defense,
    int Resistance,
    ElementType? Resilience,
    ElementType? Vulnverability);