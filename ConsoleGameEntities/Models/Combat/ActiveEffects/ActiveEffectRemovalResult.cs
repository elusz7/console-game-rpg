namespace ConsoleGameEntities.Models.Combat.ActiveEffects;

public record ActiveEffectRemovalResult(
    IReadOnlyList<ActiveEffect> RemovedEffects)
{
    public bool HasRemovedEffects => RemovedEffects.Count > 0;

    public int Count => RemovedEffects.Count;

    public ActiveEffect? RemovedEffect =>
        RemovedEffects.Count == 1 ? RemovedEffects[0] : null;
}