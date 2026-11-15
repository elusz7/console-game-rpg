
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Identifies which type of combatant takes a turn.
/// </summary>
public enum CombatantType
{
    Player,
    Monster
}

/// <summary>
/// Represents one scheduled combat turn.
/// Monster is null when the player is taking the turn.
/// </summary>
public record CombatTurn(CombatantType Type, Monster? Monster = null);

/// <summary>
/// Determines the order in which combatants take their turns.
/// </summary>
public class CombatTurnScheduler
{
    public IEnumerable<CombatTurn> ScheduleTurns(Player player, List<Monster> monsters)
    {
        // Capture the player's effective Speed at the beginning of the round.
        var roundStartPlayerSpeed = player.GetStat(StatType.Speed);

        // Capture each living monster's effective Speed and original list index.
        var orderedMonsters = monsters
            .Select((monster, index) => new { Monster = monster, Index = index })
            .Where(x => x.Monster.CurrentHealth > 0)
            .Select(x => new
            {
                x.Monster,
                x.Index,
                Speed = x.Monster.GetStat(StatType.Speed)
            })
            .OrderByDescending(x => x.Speed)
            .ThenBy(x => x.Index)
            .ToList();

        // No living monsters means there is no combat round to schedule.
        if (orderedMonsters.Count == 0)
            return Array.Empty<CombatTurn>();

        // Build the complete turn sequence immediately using captured Speeds.
        var scheduledTurns = new List<CombatTurn>();
        bool playerTurnScheduled = false;

        foreach (var entry in orderedMonsters)
        {
            // The player wins ties, using the round-start Speed snapshot.
            if (!playerTurnScheduled &&
                roundStartPlayerSpeed >= entry.Speed)
            {
                playerTurnScheduled = true;
                scheduledTurns.Add(new CombatTurn(CombatantType.Player));
            }

            scheduledTurns.Add(
                new CombatTurn(CombatantType.Monster, entry.Monster));
        }

        // If every monster was faster, the player acts last.
        if (!playerTurnScheduled)
            scheduledTurns.Add(new CombatTurn(CombatantType.Player));

        return scheduledTurns;
    }
}