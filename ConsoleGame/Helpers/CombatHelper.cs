using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGame.Helpers.AdventureEnums;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class CombatHelper(
    IInputManager inputManager,
    IOutputManager outputManager,
    EquipmentHelper equipmentHelper,
    CombatRoundCoordinator roundCoordinator,
    CombatActionDisplayHelper actionDisplayHelper,
    CombatMenuDisplayHelper menuDisplayHelper,
    CombatSkillHelper combatSkillHelper)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly CombatRoundCoordinator _roundCoordinator = roundCoordinator;
    private readonly CombatActionDisplayHelper _actionDisplayHelper = actionDisplayHelper;
    private readonly CombatMenuDisplayHelper _menuDisplayHelper = menuDisplayHelper;
    private readonly CombatSkillHelper _skillHelper = combatSkillHelper;

    public void CombatRunner(Player player, List<Monster> monsters)
    {
        try
        {
            while (true)
            {
                // Get the living monsters for this round.
                var aliveMonsters = monsters
                    .Where(m => m.CurrentHealth > 0)
                    .ToList();

                if (aliveMonsters.Count == 0)
                    break;

                // Delegate round execution to the coordinator.
                _roundCoordinator.ExecuteRound(
                    player,
                    aliveMonsters,

                    // Execute the player's turn and preserve the display behavior.
                    () =>
                    {
                        _outputManager.Display();
                        PlayerCombatTurn(player, aliveMonsters);
                        _outputManager.Clear();
                    },

                    // Display monster information before its scheduled turn.
                    monster =>
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[{monster.Name}] ({monster.CurrentHealth}/{monster.MaxHealth}) {monster.DamageType}: {monster.GetStat(StatType.Attack)}, DEF: {monster.GetStat(StatType.Defense)}, RES: {monster.GetStat(StatType.Resistance)}");
                    },

                    // Display and clear pending combat action messages.
                    () => _actionDisplayHelper.DisplayActionItems(player, aliveMonsters)
                );
            }

            _actionDisplayHelper.DisplayActionItems(player, monsters);
        }
        catch (PlayerDeathException)
        {
            _actionDisplayHelper.DisplayActionItems(player, monsters);
            _outputManager.Display();
        }
    }

    /// <summary>
    /// Runs the player's combat menu until an action successfully consumes the turn.
    /// </summary>
    private void PlayerCombatTurn(Player player, List<Monster> validTargets)
    {
        _outputManager.WriteLine(
            $"\nYour turn! You have {player.CurrentHealth} health and {player.Archetype.CurrentResource} {player.Archetype.ResourceName}.\n",
            ConsoleColor.Green);

        while (true)
        {
            var action = SelectCombatAction(player, validTargets);

            // A null action means the player canceled or could not select an action.
            if (action == null)
                continue;

            var result = ExecuteCombatAction(player, action, validTargets);

            if (result.TurnConsumed)
                return;
        }
    }
    
    /// <summary>
    /// Displays the combat menu and creates the action selected by the player.
    /// Returns null when the selection is canceled or unavailable.
    /// </summary>
    private CombatAction? SelectCombatAction(Player player, List<Monster> validTargets)
    {
        var options = new Dictionary<string, CombatOptions>
        {
            ["Attack"] = CombatOptions.Attack,
            ["Use Skill"] = CombatOptions.UseSkill
        };

        if (player.Inventory.ContainsConsumables())
            options["Use Consumable"] = CombatOptions.UseConsumable;

        options["Skip Turn"] = CombatOptions.SkipTurn;

        _menuDisplayHelper.DisplayOptions(options.Keys);

        var choice = options.ElementAt(
            _inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

        switch (choice.Value)
        {
            case CombatOptions.Attack:
            {
                var target = validTargets.Count == 1
                    ? validTargets[0]
                    : SelectTarget(validTargets, "attack");

                return target == null ? null : new AttackAction(target);
            }

            case CombatOptions.UseSkill:
                return SelectSkillAction(player, validTargets);

            case CombatOptions.UseConsumable:
                return new UseConsumableAction();

            case CombatOptions.SkipTurn:
                return new SkipTurnAction();

            default:
                _outputManager.WriteLine("Invalid choice. Please try again.", ConsoleColor.Red);
                return null;
        }
    }
    
    /// <summary>
    /// Determines which skills are available and creates the selected skill action.
    /// </summary>
    private CombatAction? SelectSkillAction(Player player, List<Monster> targets)
    {
        var availability = _skillHelper.GetAvailableSkills(player);

        var usableSkills = availability.UsableSkills;
        var waitingSkills = availability.WaitingSkills;

        if (usableSkills.Count == 0)
        {
            if (waitingSkills.Count != 0)
            {
                _inputManager.Viewer(
                    waitingSkills,
                    s => ColorfulToStringHelper.SkillStatsString(player, s),
                    "",
                    _ => ConsoleColor.DarkRed);
            }

            _outputManager.WriteLine("No skills available to use currently.", ConsoleColor.Red);
            return null;
        }

        var skill = SelectSkill(player, usableSkills, waitingSkills);

        if (skill == null)
        {
            _outputManager.WriteLine("Skill selection cancelled.");
            return null;
        }

        Monster? target = null;

        if (skill.TargetType == TargetType.SingleEnemy)
        {
            _outputManager.WriteLine();

            target = targets.Count == 1
                ? targets[0]
                : SelectTarget(targets, $"use {skill.Name} on");

            if (target == null)
            {
                _outputManager.WriteLine("\nSkill use cancelled.\n", ConsoleColor.Red);
                return null;
            }
        }

        return new UseSkillAction(skill, target);
    }
    
    /// <summary>
    /// Executes a selected combat action and returns its outcome.
    /// </summary>
    private CombatActionResult ExecuteCombatAction(
        Player player,
        CombatAction action,
        List<Monster> targets)
    {
        switch (action)
        {
            case AttackAction attack:
                return ExecuteAttack(player, attack.Target);
    
            case UseSkillAction skill:
                return ExecuteSkill(player, skill, targets);

            case UseConsumableAction:
                return UseConsumable(player, targets);

            case SkipTurnAction:
                _outputManager.WriteLine("\nSkipping Turn...", ConsoleColor.DarkRed);
                return CombatActionResult.Skipped();
    
            default:
                throw new InvalidOperationException(
                    $"Unsupported combat action: {action.GetType().Name}");
        }
    }
    
    /// <summary>
    /// Executes an attack against a previously selected target.
    /// </summary>
    private CombatActionResult ExecuteAttack(Player player, Monster target)
    {
        try
        {
            player.Attack(target);
            _outputManager.WriteLine();
            return CombatActionResult.Success();
        }
        catch (MonsterDeathException)
        {
            // The attack succeeded even though the target was defeated.
            return CombatActionResult.Success();
        }
        catch (EquipmentException)
        {
            _outputManager.WriteLine(
                "\nYou cannot attack with your bare hands!",
                ConsoleColor.Red);

            _equipmentHelper.EquipItem(player, true);
            _outputManager.WriteLine();

            // Equipping a weapon does not consume the player's turn.
            return CombatActionResult.Failed();
        }
    }
    
    /// <summary>
    /// Activates a selected skill and handles any optional follow-up attack.
    /// </summary>
    private CombatActionResult ExecuteSkill(
        Player player,
        UseSkillAction action,
        List<Monster> targets)
    {
        var skill = action.Skill;

        try
        {
            switch (skill.TargetType)
            {
                case TargetType.Self:
                    skill.Activate(player);
                    break;

                case TargetType.SingleEnemy:
                    skill.Activate(player, singleEnemy: action.Target);
                    break;

                case TargetType.AllEnemies:
                    List<ITargetable> enemyTargets = [.. targets];
                    skill.Activate(player, multipleEnemies: enemyTargets);
                    break;
            }
        }
        catch (MonsterDeathException)
        {
            skill.Reset();

            // The skill was successfully used even though it defeated a monster.
            return CombatActionResult.Success();
        }
        catch (Exception ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return CombatActionResult.Failed();
        }

        if (skill.SkillCategory == SkillCategory.Support)
            OfferFollowUpAttack(player, targets);

        return CombatActionResult.Success();
    }

    /// <summary>
    /// Selects a target and executes a follow-up or standalone attack.
    /// </summary>
    private CombatActionResult Attack(Player player, List<Monster> targets)
    {
        var target = targets.Count == 1
            ? targets[0]
            : SelectTarget(targets, "attack");

        if (target == null)
            return CombatActionResult.Cancelled();

        return ExecuteAttack(player, target);
    }

    /// <summary>
    /// Uses a consumable and handles any optional follow-up attack.
    /// </summary>
    private CombatActionResult UseConsumable(
        Player player,
        List<Monster> validTargets)
    {
        var used = _equipmentHelper.UseConsumable(player);

        if (!used)
            return CombatActionResult.Failed();

        OfferFollowUpAttack(player, validTargets);

        return CombatActionResult.Success();
    }

    /// <summary>
    /// Prompts the player to select a monster for a combat action.
    /// </summary>
    private Monster? SelectTarget(List<Monster> validTargets, string purpose)
    {
        return _inputManager.Selector(
            validTargets,
            m => m.Name,
            $"Select a target to {purpose}",
            _ => ConsoleColor.DarkRed
        );
    }

    /// <summary>
    /// Displays available skills and prompts the player to select one.
    /// </summary>
    private Skill? SelectSkill(Player player, List<Skill> usableSkills, List<Skill> waitingSkills)
    {
        _outputManager.WriteLine();
        for (int i = 0; i < usableSkills.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {ColorfulToStringHelper.SkillStatsString(player, usableSkills[i])}",
                ColorfulToStringHelper.GetSkillColor(usableSkills[i]));
        }

        
        foreach (var waitingSkill in waitingSkills)
        {
            _outputManager.WriteLine($"---{ColorfulToStringHelper.SkillStatsString(player, waitingSkill)}",
                ConsoleColor.DarkRed);
        }

        int index = _inputManager.ReadInt("\tSelect a skill to use (-1 to cancel): ", usableSkills.Count, true) - 1;

        if (index < 0)
            return null;

        return usableSkills[index];
    }
    
    /// <summary>
    /// Offers the player an optional follow-up attack after using
    /// a support skill or consumable.
    /// </summary>
    private void OfferFollowUpAttack(Player player, List<Monster> targets)
    {
        bool wantsToAttack =
            _inputManager.ReadString(
                "\nYou may also make an attack if you wish (y/n): ",
                ["y", "n"]) == "y";

        if (wantsToAttack)
        {
            // The preceding action already consumed the player's turn.
            // The follow-up attack does not change that outcome.
            Attack(player, targets);
        }
    }
}