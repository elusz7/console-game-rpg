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

public class CombatHelper(IInputManager inputManager, IOutputManager outputManager, EquipmentHelper equipmentHelper)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly EquipmentHelper _equipmentHelper = equipmentHelper;

    public void CombatRunner(Player player, List<Monster> monsters)
    {
        try
        {
            while (true)
            {
                var aliveMonsters = monsters
                    .Where(m => m.CurrentHealth > 0)
                    .OrderByDescending(m => m.Combat.GetStat(m, StatType.Speed))
                    .ToList();

                if (aliveMonsters.Count == 0)
                    break;

                bool playerTurnTaken = false;

                foreach (var monster in aliveMonsters)
                {
                    System.Diagnostics.Debug.WriteLine($"[{monster.Name}] ({monster.CurrentHealth}/{monster.MaxHealth}) {monster.DamageType}: {monster.Combat.GetStat(monster, StatType.Attack)}, DEF: {monster.Combat.GetStat(monster, StatType.Defense)}, RES: {monster.Combat.GetStat(monster, StatType.Resistance)}");

                    if (player.Combat.GetStat(player, StatType.Speed) > monster.Combat.GetStat(monster, StatType.Speed) && !playerTurnTaken)
                    {
                        _outputManager.Display();
                        PlayerCombatTurn(player, aliveMonsters);

                        OutputActionItems(player, aliveMonsters);
                        playerTurnTaken = true;
                    }

                    ResetSupportSkills(aliveMonsters);

                    if (monster.CurrentHealth > 0)
                    {
                        monster.Attack(player);

                        OutputActionItems(player, aliveMonsters);
                    }
                }

                if (!playerTurnTaken)
                {
                    var stillAlive = aliveMonsters.Where(m => m.CurrentHealth > 0).ToList();
                    PlayerCombatTurn(player, stillAlive);
                    OutputActionItems(player, aliveMonsters);
                }

                ResetSupportSkills(aliveMonsters.Where(m => m.CurrentHealth <= 0).ToList());

                ElapseTime(player, aliveMonsters);
                OutputActionItems(player, aliveMonsters);
            }

            OutputActionItems(player, monsters);
        }
        catch (PlayerDeathException)
        {
            OutputActionItems(player, monsters);
            _outputManager.Display();
        }
    }
    private void PlayerCombatTurn(Player player, List<Monster> validTargets)
    {
        _outputManager.WriteLine($"\nYour turn! You have {player.CurrentHealth} health and {player.Archetype.CurrentResource} {player.Archetype.ResourceName}.\n", ConsoleColor.Green);
        while (true)
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

            var choice = options.ElementAt(_inputManager.ReadInt("\tSelect an option: ", options.Count) - 1);

            switch (choice.Value)
            {
                case CombatOptions.Attack:
                    if (Attack(player, validTargets)) return;
                    break;
                case CombatOptions.UseSkill:
                    if (UseSkill(player, validTargets)) return;
                    break;
                case CombatOptions.UseConsumable:
                    if (UseConsumable(player, validTargets)) return;
                    break;
                case CombatOptions.SkipTurn:
                    _outputManager.WriteLine("\nSkipping Turn...", ConsoleColor.DarkRed);
                    return;
                default:
                    _outputManager.WriteLine("Invalid choice. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
    private void OutputActionItems(Player player, List<Monster> monsters)
    {
        // SortedList auto-orders by key and avoids Dictionary's duplicate key issue
        var actions = new SortedList<long, string>();

        // Add player actions
        foreach (var kvp in player.Logger.GetOrderedLog())
        {
            actions.Add(kvp.Key, kvp.Value);
        }

        // Add monster actions
        foreach (var monster in monsters ?? [])
        {
            foreach (var kvp in monster.Logger.GetOrderedLog())
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

        player.Logger.Clear();
        foreach (var monster in monsters ?? [])
        {
            monster.Logger.Clear();
        }
    }
    private static void ElapseTime(Player player, List<Monster> monsters)
    {
        /*foreach (var monster in monsters)
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

        player.Archetype.RecoverResource();*/
        player.ElapseTime();
        foreach (var monster in monsters)
            monster.ElapseTime();
    }
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
    private bool Attack(Player player, List<Monster> targets)
    {
        var target = targets.Count == 1 ? targets[0]
                    : SelectTarget(targets, "attack");

        if (target == null) return false;

        try
        {
            player.Attack(target);
            _outputManager.WriteLine();
            return true;
        }
        catch (MonsterDeathException)
        {
            return true;
        }
        catch (EquipmentException)
        {
            _outputManager.WriteLine("\nYou cannot attack with your bare hands!", ConsoleColor.Red);

            _equipmentHelper.EquipItem(player, true);
            _outputManager.WriteLine();
            return false;
        }
    }
    private bool UseSkill(Player player, List<Monster> targets)
    {
        var allSkills = player.Archetype.Skills
                .Where(s => s.RequiredLevel <= player.Level)
                .ToList() ?? [];

        var usableSkills = allSkills
                .Where(s => s.Cost <= player.Archetype.CurrentResource)
                .Where(s => !s.IsOnCooldown)
                .Where(s => s is not UltimateSkill).ToList();
        usableSkills.AddRange(allSkills.Where(s => s is UltimateSkill ult && ult.IsReady)
            .Where(s => s.Cost <= player.Archetype.CurrentResource));

        var waitingSkills = allSkills
            .Where(s =>
                (s.Cost > player.Archetype.CurrentResource || s.IsOnCooldown) &&
                s.Cost <= player.Archetype.MaxResource &&
                s is not UltimateSkill)
            .ToList();
        waitingSkills.AddRange(allSkills.Where(s => s is UltimateSkill ult && !ult.IsReady));

        if (usableSkills.Count == 0)
        {
            _outputManager.WriteLine();
            if (waitingSkills.Count != 0)
            {

                _inputManager.Viewer(waitingSkills, s => ColorfulToStringHelper.SkillStatsString(player, s), "", _ => ConsoleColor.DarkRed);
            }
            _outputManager.WriteLine("No skills available to use currently.\n", ConsoleColor.Red);
            return false;
        }

        var skill = SelectSkill(player, usableSkills, waitingSkills);

        if (skill == null)
        {
            _outputManager.WriteLine("\nSkill selection cancelled.\n", ConsoleColor.Red);
            return false;
        }

        try
        {
            switch (skill.TargetType)
            {
                case TargetType.Self:
                    skill.Activate(player);
                    break;
                case TargetType.SingleEnemy:
                    _outputManager.WriteLine();
                    var target = targets.Count == 1 ? targets[0]
                        : SelectTarget(targets, $"use {skill.Name} on");

                    if (target == null)
                    {
                        _outputManager.WriteLine("\nSkill use cancelled.\n", ConsoleColor.Red);
                        return false;
                    }

                    skill.Activate(player, singleEnemy: target);
                    break;
                case TargetType.AllEnemies:
                    List<ITargetable> enemyTargets = [.. targets.Cast<ITargetable>()];
                    skill.Activate(player, multipleEnemies: enemyTargets);
                    break;
            }
        }
        catch (MonsterDeathException)
        {
            skill.Reset(); //if monster died, the skill was still used
            return true;
        }
        catch (Exception ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return false;
        }

        if (skill.SkillCategory == SkillCategory.Support)
        {
            bool wantsToAttack = _inputManager.ReadString("\nYou may also make an attack if you wish (y/n): ", ["y", "n"]) == "y";
            if (wantsToAttack)
            {
                Attack(player, targets);
                return true;
            }
        }
        else
        {
            _outputManager.WriteLine();
        }

        return true;
    }
    private bool UseConsumable(Player player, List<Monster> validTargets)
    {
        var used = _equipmentHelper.UseConsumable(player);

        if (used)
        {
            bool wantsToAttack = _inputManager.ReadString("\nYou may also make an attack if you wish (y/n): ", ["y", "n"]) == "y";
            if (wantsToAttack)
            {
                _outputManager.WriteLine();
                Attack(player, validTargets);
                return used;
            }
        }


        return used;
    }
    private Monster? SelectTarget(List<Monster> validTargets, string purpose)
    {
        return _inputManager.Selector(
            validTargets,
            m => m.Name,
            $"Select a target to {purpose}",
            _ => ConsoleColor.DarkRed
        );
    }
    private Skill? SelectSkill(Player player, List<Skill> usableSkills, List<Skill> waitingSkills)
    {
        _outputManager.WriteLine();
        for (int i = 0; i < usableSkills.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {ColorfulToStringHelper.SkillStatsString(player, usableSkills[i])}", ColorfulToStringHelper.GetSkillColor(usableSkills[i]));
        }
        for (int k = 0; k < waitingSkills.Count; k++)
        {
            _outputManager.WriteLine($"---{ColorfulToStringHelper.SkillStatsString(player, waitingSkills[k])}", ConsoleColor.DarkRed);
        }

        int index = _inputManager.ReadInt("\tSelect a skill to use (-1 to cancel): ", usableSkills.Count, true) - 1;

        if (index < 0)
            return null;

        return usableSkills[index];
    }
}