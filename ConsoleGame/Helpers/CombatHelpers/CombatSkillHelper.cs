using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Determines which skills a player can currently use during combat.
/// </summary>
public class CombatSkillHelper
{
    /// <summary>
    /// Determines which unlocked skills are usable and which are waiting
    /// for sufficient resources or cooldown recovery.
    /// </summary>
    public CombatSkillAvailability GetAvailableSkills(Player player)
    {
        // Only consider skills unlocked at the player's current level.
        var allSkills = player.Archetype.Skills
            .Where(s => s.RequiredLevel <= player.Level)
            .ToList();

        // Identify regular skills that have sufficient resources and are off cooldown.
        var usableSkills = allSkills
            .Where(s => s.Cost <= player.Archetype.CurrentResource)
            .Where(s => !s.IsOnCooldown)
            .Where(s => s is not UltimateSkill)
            .ToList();

        // Ultimate skills are available only when ready.
        usableSkills.AddRange(
            allSkills.Where(s => s is UltimateSkill ult && ult.IsReady)
                .Where(s => s.Cost <= player.Archetype.CurrentResource));

        // Identify regular skills waiting for resources or cooldown recovery.
        var waitingSkills = allSkills
            .Where(s =>
                (s.Cost > player.Archetype.CurrentResource || s.IsOnCooldown) &&
                s.Cost <= player.Archetype.MaxResource &&
                s is not UltimateSkill)
            .ToList();

        // Include ultimate skills that are not ready.
        waitingSkills.AddRange(
            allSkills.Where(s => s is UltimateSkill ult && !ult.IsReady));

        return new CombatSkillAvailability(usableSkills, waitingSkills);
    }
}