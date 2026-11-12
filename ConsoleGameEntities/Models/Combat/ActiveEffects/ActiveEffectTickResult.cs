namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public record ActiveEffectTickResult(
    IReadOnlyList<ActiveEffect> ProcessedEffects,
    IReadOnlyList<ActiveEffect> ExpiredEffects)
{
    public int ProcessedCount => ProcessedEffects.Count;

    public int ExpiredCount => ExpiredEffects.Count;
}