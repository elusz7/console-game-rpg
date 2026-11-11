namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public record ActiveEffectRegisterResult(
    ActiveEffect Effect,
    RegistrationStatus Registration
    );