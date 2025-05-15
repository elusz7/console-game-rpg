using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.Helpers;

public class CombatHelper(InputManager inputManager, OutputManager outputManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;

    private const int AttackOption = 1;
    private const int SkillOption = 2;
    private const int ConsumableOption = 3;
    private const int ExitOption = 4;

    private bool tryEquipWeapon = true;

    public void CombatRunner(Player player, List<Monster> monsters)
    {
        tryEquipWeapon = true;

        try
        {
            while (true)
            {
                var aliveMonsters = monsters
                    .Where(m => m.CurrentHealth > 0)
                    .ToList();

                if (aliveMonsters == null || aliveMonsters.Count == 0)
                    break;

                bool playerTurnTaken = false;

                // Sort monsters by speed
                aliveMonsters.Sort((m1, m2) => m2.GetStat(StatType.Speed).CompareTo(m1.GetStat(StatType.Speed)));

                for (int i = 0; i < aliveMonsters.Count; i++)
                {
                    var monster = aliveMonsters[i];

                    // Player attacks before first monster faster than monster
                    if (player.GetStat(StatType.Speed) > monster.GetStat(StatType.Speed) && !playerTurnTaken)
                    {
                        PlayerCombatTurn(player, aliveMonsters);
                        OutputActionItems(player, aliveMonsters);
                        playerTurnTaken = true;
                    }

                    ResetSupportSkills(aliveMonsters);
                    
                    if (monster.CurrentHealth > 0)
                    {
                        monster.Attack(player);
                        _outputManager.WriteLine();
                        OutputActionItems(player, aliveMonsters);
                    }
                }

                // Player's turn if not already taken
                if (!playerTurnTaken)
                {
                    PlayerCombatTurn(player, [.. aliveMonsters.Where(m => m.CurrentHealth > 0)]);
                    OutputActionItems(player, aliveMonsters);
                }

                // Reset skills for dead monsters
                ResetSupportSkills([.. aliveMonsters.Where(m => m.CurrentHealth <= 0)]);

                ElapseTime(player, aliveMonsters);
                OutputActionItems(player, aliveMonsters);
            }

            OutputActionItems(player, monsters);
            _outputManager.WriteLine("\nYou have defeated all the monsters in the room!", ConsoleColor.Green);
            _outputManager.WriteLine("You are now free to continue exploring.\n", ConsoleColor.Cyan);
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
            var options = new List<(int, string)>
            {
                (AttackOption, "Attack"),
                (SkillOption, "Use Skill")
            };

            if (player.Inventory.ContainsConsumables())
                options.Add((ConsumableOption, "Use Consumable"));

            options.Add((ExitOption, "Exit Combat"));

            foreach (var (index, label) in options)
                _outputManager.WriteLine($"{index}. {label}");

            int choice = _inputManager.ReadInt("Select an option: ", 4);

            switch (choice)
            {
                case AttackOption:
                    if (Attack(player, validTargets)) return;
                    break;
                case SkillOption:
                    if (UseSkill(player, validTargets)) return;
                    break;
                case ConsumableOption:
                    if (UseConsumable(player)) return;
                    break;
                case ExitOption:
                    _outputManager.WriteLine("\nExiting combat now.\n");
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
        _outputManager.WriteLine();
        var target = targets.Count == 1 ? targets[0]
                    : SelectTarget(targets, "attack");

        if (target == null)
        {
            _outputManager.WriteLine("\nAttacking cancelled.\n");
            return false;
        }

        try
        {
            player.Attack(target);
            return true;
        }
        catch (MonsterDeathException)
        {
            return true;
        }
        catch (EquipmentException)
        {
            _outputManager.WriteLine("You cannot attack with your bare hands!\n", ConsoleColor.Red);

            if (tryEquipWeapon)
                EquipWeapon(player);
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
            if (waitingSkills.Count != 0)
            {
                _inputManager.Viewer(waitingSkills, s => ColorfulToStringHelper.SkillStatsString(player, s), "", _ => ConsoleColor.DarkRed);
            }
            _outputManager.WriteLine("\nNo skills available to use currently.\n", ConsoleColor.Red);
            return false;
        }

        _outputManager.WriteLine();
        var skill = SelectSkill(player, usableSkills, waitingSkills);
        _outputManager.WriteLine();

        if (skill == null)
        {
            _outputManager.WriteLine("Skill selection cancelled.\n");
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
                    var target = targets.Count == 1 ? targets[0]
                        : SelectTarget(targets, $"use {skill.Name} on");

                    if (target == null)
                    {
                        _outputManager.WriteLine("\nSkill use cancelled.\n");
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

        if(skill.SkillCategory == SkillCategory.Support)
        {
            if (_inputManager.ReadString($"You may also make an attack if you wish (y/n): ", ["y", "n"]) == "y")
            {
                Attack(player, targets);
                return true;
            }
            _outputManager.WriteLine();
        }

        return true;
    }
    private bool UseConsumable(Player player)
    {
        var availableConsumables = player.Inventory.Items.Where(i => i is Consumable).ToList();

        if (availableConsumables.Count == 0)
        {
            _outputManager.WriteLine("\nNo consumables in your inventory.\n", ConsoleColor.Red);
            return false;
        }

        _outputManager.WriteLine();
        var item = _inputManager.SelectItem("Select a consumable to use", availableConsumables);

        if (item == null)
        {
            _outputManager.WriteLine("\nConsumable selection cancelled.\n");
            return false;
        }
        else if (item is Consumable consumable)
        {
            switch (consumable.ConsumableType)
            {
                case ConsumableType.Health:
                    consumable.UseOn(player);
                    break;
                case ConsumableType.Resource:
                    consumable.UseOn(player);
                    break;
                case ConsumableType.Durability:
                    _outputManager.WriteLine();
                    var target = _inputManager.SelectItem("Select an item to increase durability", [.. player.Inventory.Items.Where(i => i.IsEquipped())]);

                    if (target == null)
                    {
                        _outputManager.WriteLine("Item selection cancelled.");
                        return false;
                    }

                    consumable.UseOn(target);
                    break;
            }
        }
        else
        {
            _outputManager.WriteLine("\nInvalid consumable type selected.\n", ConsoleColor.Red);
            return false;
        }

        return true;
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
        for (int i = 0; i < usableSkills.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {ColorfulToStringHelper.SkillStatsString(player, usableSkills[i])}", ColorfulToStringHelper.GetSkillColor(usableSkills[i]));
        }
        for (int k = 0; k < waitingSkills.Count; k++)
        {
            _outputManager.WriteLine($"---{ColorfulToStringHelper.SkillStatsString(player, waitingSkills[k])}", ConsoleColor.DarkRed);
        }

        int index = _inputManager.ReadInt("\n\tSelect a skill to use (-1 to cancel): ", usableSkills.Count, true) - 1;

        if (index < 0)
            return null;
        
        return usableSkills[index];
    }
    private void EquipWeapon(Player player)
    {
        var availableWeapons = player.Inventory.Items
                .Where(i => i is Weapon && i.RequiredLevel <= player.Level && i.Durability > 0)
                .ToList();

        if (availableWeapons.Count == 0)
        {
            _outputManager.WriteLine("You have no weapons to equip.\n", ConsoleColor.Red);
            tryEquipWeapon = false;
            return;
        }

        _outputManager.WriteLine();
        var weapon = _inputManager.SelectItem("Select a weapon to equip", availableWeapons);

        if (weapon == null)
        {
            _outputManager.WriteLine("\nNo weapon selected.\n", ConsoleColor.Red);
            return;
        }

        try
        {
            player.Equip(weapon);
            _outputManager.WriteLine($"\nYou have equipped {weapon.Name}. Try attacking again.\n", ConsoleColor.Green);
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine($"\n{ex.Message}\n", ConsoleColor.Red);

        }
    }    
}