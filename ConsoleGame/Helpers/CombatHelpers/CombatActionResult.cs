namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Describes the outcome of executing a combat action.
/// </summary>
public enum CombatActionOutcome
{
    Success,
    Failed,
    Cancelled,
    TurnSkipped
}

/// <summary>
/// Represents the result of executing a combat action.
/// </summary>
public record CombatActionResult(
    CombatActionOutcome Outcome,
    bool TurnConsumed)
{
    public static CombatActionResult Success() =>
        new(CombatActionOutcome.Success, true);

    public static CombatActionResult Failed() =>
        new(CombatActionOutcome.Failed, false);

    public static CombatActionResult Cancelled() =>
        new(CombatActionOutcome.Cancelled, false);

    public static CombatActionResult Skipped() =>
        new(CombatActionOutcome.TurnSkipped, true);
}