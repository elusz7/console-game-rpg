namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public record ActiveEffectDoTDamageResults(
    List<DoTEffectResult> EffectResults,
    List<ActiveEffect> ProcessedEffects,
    List<ActiveEffect> ExpiredEffects
    );