using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.Helpers.CombatHelpers;

/// <summary>
/// Base type for actions a player can choose during combat.
/// </summary>
public abstract record CombatAction;

/// <summary>
/// Represents a basic attack against a selected monster.
/// </summary>
public sealed record AttackAction(Monster Target) : CombatAction;

/// <summary>
/// Represents using a selected skill.
/// Target is optional because some skills target the player
/// or all enemies rather than one specific monster.
/// </summary>
public sealed record UseSkillAction(
    Skill Skill,
    Monster? Target = null) : CombatAction;

/// <summary>
/// Represents the player's decision to use a consumable.
/// The consumable itself is selected during action execution.
/// </summary>
public sealed record UseConsumableAction : CombatAction;

/// <summary>
/// Represents the player's decision to skip their turn.
/// </summary>
public sealed record SkipTurnAction : CombatAction;