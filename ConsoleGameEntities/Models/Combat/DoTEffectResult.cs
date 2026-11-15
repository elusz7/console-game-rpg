using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Models.Combat;

public record DoTEffectResult(
    int InitialDamage,
    int FinalDamage,
    ModelEnums.ElementType Element,
    bool TargetDied
    );