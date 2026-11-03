
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
        // Sort living monsters by their Speed at the beginning of the round.
        // ThenBy preserves their original list order when Speeds are equal.
        var orderedMonsters = monsters
            .Where(m => m.CurrentHealth > 0)
            .Select((monster, index) => new { Monster = monster, Index = index })
            .OrderByDescending(x => x.Monster.GetStat(StatType.Speed))
            .ThenBy(x => x.Index)
            .Select(x => x.Monster)
            .ToList();

        // No living monsters means there is no combat round to schedule.
        if (orderedMonsters.Count == 0)
            yield break;

        bool playerTurnScheduled = false;

        foreach (var monster in orderedMonsters)
        {
            // Check current Speeds immediately before this monster's turn.
            // The player wins ties but only receives one turn per round.
            if (!playerTurnScheduled &&
                player.GetStat(StatType.Speed) >= monster.GetStat(StatType.Speed))
            {
                playerTurnScheduled = true;
                yield return new CombatTurn(CombatantType.Player);
            }

            yield return new CombatTurn(CombatantType.Monster, monster);
        }

        // If every monster was faster, the player acts last.
        if (!playerTurnScheduled)
            yield return new CombatTurn(CombatantType.Player);
    }
}