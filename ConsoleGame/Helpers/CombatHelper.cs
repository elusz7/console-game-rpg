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

    private bool tryEquipWeapon = true;

    public void CombatRunner(IPlayer player, List<Monster> monsters)
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

                bool playerTurn = false;

                // Sort monsters by speed
                aliveMonsters.Sort((m1, m2) => m2.GetStat(StatType.Speed).CompareTo(m1.GetStat(StatType.Speed)));

                for (int i = 0; i < aliveMonsters.Count; i++)
                {
                    var monster = aliveMonsters[i];

                    // Player attacks before first monster faster than monster
                    if (player.GetStat(StatType.Speed) > monster.GetStat(StatType.Speed) && !playerTurn)
                    {
                        PlayerCombatTurn(player, aliveMonsters);
                        OutputActionItems(player, aliveMonsters);
                        playerTurn = true;
                    }

                    ResetSupportSkills(aliveMonsters);

                    if (monster.CurrentHealth > 0)
                    {
                        monster.Attack(player);
                        OutputActionItems(player, aliveMonsters);
                    }
                }

                // Player's turn if not already taken
                if (!playerTurn)
                {
                    PlayerCombatTurn(player, [.. aliveMonsters.Where(m => m.CurrentHealth > 0)]);
                    OutputActionItems(player, aliveMonsters);
                }

                // Reset skills for dead monsters
                ResetSupportSkills([.. aliveMonsters.Where(m => m.CurrentHealth <= 0)]);

                ElapseTime(player, aliveMonsters);
                OutputActionItems(player, aliveMonsters);
            }

            // Reset player skills at end of combat
            foreach (var skill in player.Archetype.Skills ?? [])
            {
                skill.Reset();
            }

            _outputManager.WriteLine("You have defeated all the monsters in the room!", ConsoleColor.Green);
            _outputManager.WriteLine("You are now free to continue exploring.", ConsoleColor.Cyan);
        }
        catch (PlayerDeathException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            _outputManager.WriteLine("You have died. Game over.", ConsoleColor.Red);
        }
    }
    private void PlayerCombatTurn(IPlayer player, List<Monster> validTargets)
    {
        _outputManager.WriteLine($"Your turn! You have {player.CurrentHealth} health and {player.Archetype.CurrentResource} {player.Archetype.ResourceName}.", ConsoleColor.Green);
        while (true)
        {
            var options = new List<(int, string)>
            {
                (AttackOption, "Attack"),
                (SkillOption, "Use Skill")
            };

            if (player.Inventory.ContainsConsumables())
                options.Add((ConsumableOption, "Use Consumable"));

            foreach (var (index, label) in options)
                _outputManager.WriteLine($"{index}. {label}");

            int choice = _inputManager.ReadInt("Select an option: ", options.Count);

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
                default:
                    _outputManager.WriteLine("Invalid choice. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
    private void OutputActionItems(IPlayer player, List<Monster> monsters)
    {
        // SortedList auto-orders by key and avoids Dictionary's duplicate key issue
        var actions = new SortedList<long, string>();

        // Add player actions
        foreach (var kvp in player.ActionItems)
        {
            long key = kvp.Key;
            while (actions.ContainsKey(key)) key++; // Avoid duplicate keys
            actions.Add(key, kvp.Value);
        }

        // Add monster actions
        foreach (var monster in monsters ?? [])
        {
            foreach (var kvp in monster.ActionItems)
            {
                long key = kvp.Key;
                while (actions.ContainsKey(key)) key++; // Avoid duplicate keys
                actions.Add(key, kvp.Value);
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
    private static void ElapseTime(IPlayer player, List<Monster> monsters)
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
    private bool Attack(IPlayer player, List<Monster> targets)
    {
        var target = targets.Count == 1 ? targets[0]
                    : SelectTarget(targets, "attack");

        if (target == null)
        {
            _outputManager.WriteLine("Attacking cancelled.");
            return false;
        }

        try
        {
            player.Attack(target);
            return true;
        }
        catch (EquipmentException)
        {
            _outputManager.WriteLine("You cannot attack with your bare hands!", ConsoleColor.Red);

            if (tryEquipWeapon)
                EquipWeapon(player);
            return false;
        }
    }
    private bool UseSkill(IPlayer player, List<Monster> targets)
    {
        var skill = SelectSkill(player.Archetype.Skills?
                .Where(s => s.RequiredLevel <= player.Level)
                .Where(s => s.Cost <= player.Archetype.CurrentResource)
                .Where(s => !s.IsOnCooldown)
                .ToList() ?? []);

        if (skill == null)
        {
            _outputManager.WriteLine("Skill selection cancelled.");
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
                        _outputManager.WriteLine("Skill use cancelled.");
                        return false;
                    }

                    skill.Activate(player, target);
                    break;
                case TargetType.AllEnemies:
                    List<ITargetable> enemyTargets = [.. targets.Cast<ITargetable>()];
                    skill.Activate(player, targets: enemyTargets);
                    break;
            }
        }
        catch (Exception ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
            return false;
        }

        if(skill.SkillCategory == SkillCategory.Support)
        {
            if (_inputManager.ReadString("You have used a support skill. You may make an attack if you wish (y/n): ", ["y", "n"]) == "y")
            {
                Attack(player, targets);
            }
        }

        return true;
    }
    private bool UseConsumable(IPlayer player)
    {
        var consumable = SelectConsumable(player.Inventory.Items.OfType<Consumable>().ToList());

        if (consumable == null)
        {
            _outputManager.WriteLine("Consumable selection cancelled.");
            return false;
        }

        switch (consumable.ConsumableType)
        {
            case ConsumableType.Health:
                consumable.UseOn(player);
                break;
            case ConsumableType.Resource:
                consumable.UseOn(player);
                break;
            case ConsumableType.Durability:
                var target = SelectItem("Select an item to increase durability", player.Inventory.Items.Where(i => i.IsEquipped()).ToList());

                if (target == null)
                {
                    _outputManager.WriteLine("Item selection cancelled.");
                    return false;
                }

                consumable.UseOn(target);
                break;
        }

        return true;
    }
    private Monster? SelectTarget(List<Monster> validTargets, string purpose)
    {
        return _inputManager.SelectFromList(
            validTargets, 
            m => m.Name, 
            $"Select a target to {purpose}",
            _ => ConsoleColor.DarkRed
        );
    }
    private Skill? SelectSkill(List<Skill> skills)
    {
        return _inputManager.SelectFromList(
            skills,
            s => $"{s.Name} [power: {s.Power}, cost: {s.Cost}, skillType: {s.SkillType}, targets: {s.TargetType}]",
            "Select a skill",
            s => ConsoleColor.Green
        );
    }
    private Consumable? SelectConsumable(List<Consumable> consumables)
    {
        return _inputManager.SelectFromList(
            consumables,
            c => $"{c.Name} [power: {c.Power}, type: {c.ConsumableType}]",
            "Select a consumable",
            _ => ConsoleColor.Yellow
        );
    }
    private Item? SelectItem(string prompt, List<Item> equipment)
    {
        return _inputManager.SelectFromList(
            equipment,
            i => $"{i.Name} [durability: {i.Durability}{GetItemStats(i)}]",
            prompt,
            _ => ConsoleColor.Blue
        );
    }
    private static string GetItemStats(Item item)
    {
        return item switch
        {
            Weapon weapon => $", Attack: {weapon.AttackPower}",
            Armor armor => $", Defense: {armor.DefensePower}, Resistance: {armor.Resistance}",
            _ => ""
        };
    }
    private void EquipWeapon(IPlayer player)
    {
        var availableWeapons = player.Inventory.Items
                .Where(i => i.ItemType.Equals(ItemType.Weapon) && i.RequiredLevel <= player.Level && i.Durability > 0)
                .ToList();

        if (availableWeapons.Count == 0)
        {
            _outputManager.WriteLine("You have no weapons to equip.", ConsoleColor.Red);
            tryEquipWeapon = false;
            return;
        }

        var weapon = SelectItem("Select a weapon to equip", availableWeapons);

        if (weapon == null)
        {
            _outputManager.WriteLine("No weapon selected.", ConsoleColor.Red);
            return;
        }

        try
        {
            player.Equip(weapon);
            _outputManager.WriteLine($"You have equipped {weapon.Name}. Try attacking again.", ConsoleColor.Green);
        }
        catch (InvalidOperationException ex)
        {
            _outputManager.WriteLine(ex.Message, ConsoleColor.Red);
        }
    }
}