namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public record ActiveEffectRegisterResult(
    ActiveEffect IncomingEffect,
    RegistrationStatus Registration
    );