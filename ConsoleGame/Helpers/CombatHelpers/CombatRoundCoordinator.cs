using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Coordinates the execution and completion of a combat round.
/// </summary>
public class CombatRoundCoordinator(CombatTurnScheduler turnScheduler)
{
    private readonly CombatTurnScheduler _turnScheduler = turnScheduler;

    /// <summary>
    /// Executes one complete combat round, including turn processing
    /// and end-of-round cooldown and resource updates.
    /// </summary>
    /// <param name="player">The player participating in combat.</param>
    /// <param name="monsters">The living monsters at the start of the round.</param>
    /// <param name="playerTurn">Callback that executes the player's turn.</param>
    /// <param name="beforeMonsterTurn">Callback invoked before a monster's turn.</param>
    /// <param name="displayActions">Callback that displays and clears pending combat actions.</param>
    public void ExecuteRound(
        Player player,
        List<Monster> monsters,
        Action playerTurn,
        Action<Monster> beforeMonsterTurn,
        Action displayActions)
    {
        // Process turns in the order determined by the scheduler.
        var scheduledTurns = _turnScheduler.ScheduleTurns(player, monsters);

        foreach (var turn in scheduledTurns)
        {
            if (turn.Type == CombatantType.Player)
            {
                playerTurn();
                displayActions();
                continue;
            }

            var monster = turn.Monster!;

            // Reset support skills before the next monster acts.
            ResetSupportSkills(monsters);

            beforeMonsterTurn(monster);

            // A monster may have been defeated before its scheduled turn.
            if (monster.CurrentHealth > 0)
            {
                monster.Attack(player);
                displayActions();
            }
        }

        // Cooldowns and player resource recovery happen once per completed round.
        ElapseTime(player, monsters);
        displayActions();
    }

    /// <summary>
    /// Resets support skills belonging to defeated monsters.
    /// </summary>
    private static void ResetSupportSkills(List<Monster> monsters)
    {
        foreach (var monster in monsters)
        {
            if (monster.CurrentHealth <= 0)
            {
                foreach (var skill in monster.Skills?.OfType<SupportSkill>().ToList() ?? [])
                {
                    skill.Reset();
                }
            }
        }
    }

    /// <summary>
    /// Advances monster and player skill cooldowns and recovers player resources.
    /// </summary>
    private static void ElapseTime(Player player, List<Monster> monsters)
    {
        foreach (var monster in monsters)
        {
            foreach (var skill in monster.Skills ?? [])
            {
                skill.UpdateElapsedTime();
            }
        }

        foreach (var skill in player.Archetype.Skills ?? [])
        {
            skill.UpdateElapsedTime();
        }

        player.Archetype.RecoverResource();
    }
}