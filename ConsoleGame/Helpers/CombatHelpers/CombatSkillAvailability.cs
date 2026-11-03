using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Contains the skills currently usable by a player and those waiting
/// for sufficient resources or cooldown recovery.
/// </summary>
public record CombatSkillAvailability(
    List<Skill> UsableSkills,
    List<Skill> WaitingSkills);