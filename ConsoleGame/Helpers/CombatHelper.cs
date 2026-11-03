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
    CombatRoundCoordinator roundCoordinator)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;
    private readonly CombatRoundCoordinator _roundCoordinator = roundCoordinator;

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
                    () => OutputActionItems(player, aliveMonsters)
                );
            }

            OutputActionItems(player, monsters);
        }
        catch (PlayerDeathException)
        {
            OutputActionItems(player, monsters);
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

            // A null action means the player cancelled or could not select an action.
            if (action == null)
                continue;

            var result = ExecuteCombatAction(player, action, validTargets);

            if (result.TurnConsumed)
                return;
        }
    }
    
    /// <summary>
    /// Displays the combat menu and creates the action selected by the player.
    /// Returns null when the selection is cancelled or unavailable.
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

        var index = 1;
        foreach (var option in options)
        {
            _outputManager.WriteLine($"{index}. {option.Key}", ConsoleColor.Yellow);
            index++;
        }

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
        var allSkills = player.Archetype.Skills
            .Where(s => s.RequiredLevel <= player.Level)
            .ToList();

        var usableSkills = allSkills
            .Where(s => s.Cost <= player.Archetype.CurrentResource)
            .Where(s => !s.IsOnCooldown)
            .Where(s => s is not UltimateSkill)
            .ToList();

        usableSkills.AddRange(
            allSkills.Where(s => s is UltimateSkill ult && ult.IsReady)
                .Where(s => s.Cost <= player.Archetype.CurrentResource));

        var waitingSkills = allSkills
            .Where(s =>
                (s.Cost > player.Archetype.CurrentResource || s.IsOnCooldown) &&
                s.Cost <= player.Archetype.MaxResource &&
                s is not UltimateSkill)
            .ToList();

        waitingSkills.AddRange(
            allSkills.Where(s => s is UltimateSkill ult && !ult.IsReady));

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
    /// Collects, orders, and displays pending combat messages from the player
    /// and monsters, then clears their action message collections.
    /// </summary>
    private void OutputActionItems(Player player, List<Monster> monsters)
    {
        // SortedList auto-orders by key and avoids Dictionary's duplicate key issue
        var actions = new SortedList<long, string>();

        // Add player actions
        foreach (var kvp in player.ActionItems)
        {
            long key = kvp.Key;
            while (!actions.TryAdd(key, kvp.Value)) key++;
        }

        // Add monster actions
        foreach (var monster in monsters ?? [])
        {
            foreach (var kvp in monster.ActionItems)
            {
                long key = kvp.Key;
                while (!actions.TryAdd(key, kvp.Value)) key++;
            }
        }

        // Output ordered actions
        foreach (var action in actions)
        {
            _outputManager.WriteLine(action.Value, ConsoleColor.Cyan);
        }

        player.ClearActionItems();
        foreach (var monster in monsters ?? [])
        {
            monster.ClearActionItems();
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
                    List<ITargetable> enemyTargets = [.. targets.Cast<ITargetable>()];
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
        {
            bool wantsToAttack =
                _inputManager.ReadString(
                    "\nYou may also make an attack if you wish (y/n): ",
                    ["y", "n"]) == "y";

            if (wantsToAttack)
            {
                // The skill already consumed the turn, regardless of the follow-up attack.
                Attack(player, targets);
            }
        }

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

        bool wantsToAttack =
            _inputManager.ReadString(
                "\nYou may also make an attack if you wish (y/n): ",
                ["y", "n"]) == "y";

        if (wantsToAttack)
        {
            // Using the consumable already consumed the turn.
            Attack(player, validTargets);
        }

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

        for (int k = 0; k < waitingSkills.Count; k++)
        {
            _outputManager.WriteLine($"---{ColorfulToStringHelper.SkillStatsString(player, waitingSkills[k])}",
                ConsoleColor.DarkRed);
        }

        int index = _inputManager.ReadInt("\tSelect a skill to use (-1 to cancel): ", usableSkills.Count, true) - 1;

        if (index < 0)
            return null;

        return usableSkills[index];
    }
}