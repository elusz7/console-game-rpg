using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Helpers.Gameplay;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Entities;

public class Player : IPlayer
{
    [NotMapped]

    private static readonly Random _rng = Random.Shared;
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    [NotMapped]
    public double DodgeChance { get; set; } = 0.015;
    public int MaxHealth { get; set; }
    public int ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }

    [NotMapped]
    public EffectManager Effects { get; } = new();
    [NotMapped]
    public PlayerCombatBehavior Combat { get; } = new();
    [NotMapped]
    public EquipmentManager EquipmentManager { get; } = new();
    [NotMapped]
    public IReadOnlyCollection<IEquippable> Equipment => EquipmentManager.GetEquippedItems();
    [NotMapped]
    public ActionLogger Logger { get; } = new();


    public void Attack(ITargetable target) => Combat.Attack(this, target);
    public virtual void TakeDamage(int damage, DamageType? damageType) => Combat.TakeDamage(this, damage, damageType);
    public virtual void TakeDamage(int damage, ElementType element) => Combat.TakeElementalDamage(this, damage, element);
    public virtual void Heal(int regainedHealth) => Combat.Heal(this, regainedHealth);
    public virtual void MaxHeal()
    {
        Heal(MaxHealth);
        Logger.Clear();
    }
    
    public void LevelUp()
    {
        Level++;

        var boost = Archetype.LevelUp(Level);
        MaxHealth += (boost * 2);//boost;
        CurrentHealth = MaxHealth;

        Inventory.Gold += 500;//RESET (Level * _rng.Next(8, 13));
        Inventory.Capacity += Math.Round(boost * 0.8M, 2);
        DodgeChance += 0.003;

        Effects.ClearEffects();
    }

    public void Equip(Item item) => EquipmentManager.Equip(this, item);
    public void Unequip(Item item) => EquipmentManager.Unequip(this, item);
    public void ApplyRune(WeaponRune rune) => EquipmentManager.ApplyRune(this, rune);
    public void ApplyRune(ArmorRune rune, Armor armor) => EquipmentManager.ApplyRune(this, rune, armor);

    public void ModifyStat(StatType stat, int duration, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else
        {
            Logger.Log($"Your {stat} has been modified by {amount} for {duration} turns!");
            Effects.AddEffect(StatusRecordType.Skill, (int)stat, duration, amount);
        }
    }

    public void Loot(IMonster monster)
    {

        if (monster.CurrentHealth >= 1)
        {
            throw new TreasureException($"You think it might be better to defeat {monster.Name} before trying to take its treasure.");
        }

        Item loot = monster.Loot() ?? throw new TreasureException($"{monster.Name} has no treasure to loot.");

        try
        {
            Inventory.AddItem(loot);

        }
        catch (Exception ex)
        {
            monster.SetLoot(loot); //return loot to monster
            throw new TreasureException(ex.Message);
        }

        Logger.Log($"You found {loot.Name} on {monster.Name}!");
    }
    public void Loot(List<Ingredient> monsterDrop)
    {
        foreach (var ingredient in monsterDrop)
        {
            try
            {
                Inventory.AddIngredient(ingredient);
            }
            catch (Exception ex)
            {
                throw new TreasureException(ex.Message);
            }
        }
    }
    public virtual void Sell(Item item) => Inventory.Sell(item);
    public virtual void Buy(Item item) => Inventory.Buy(item);

    public virtual void Initialize()
    {
        Level = 1;
        MaxHealth = Archetype.HealthBase * 2;

        Inventory = new Inventory
        {
            Gold = 1000, //RESET _rng.Next(15, 24),
            Capacity = 500, //(decimal)Math.Round((_rng.NextDouble() * 10 + 15), 2),
            Player = this
        };

        foreach (var skill in Archetype.Skills)
        {
            skill.InitializeSkill(Level);
        }

        CurrentHealth = MaxHealth;
        Archetype.CurrentResource = Archetype.MaxResource;
    }
    public virtual void RecoverResource(int recoveryPower) => Archetype.RecoverResource(recoveryPower);

    public virtual void ElapseTime()
    {
        Effects.ElapseTime(
            onExpire: e =>
            {
                switch (e.Source)
                {
                    case StatusRecordType.Skill:
                        Logger.Log($"An effect affecting your {(StatType)e.Type} has ended!");
                        break;
                    case StatusRecordType.ElementalStatus:
                        Logger.Log($"An elemental status causing you to be {(ElementalStatusEffectType)e.Type} has ended!");
                        break;
                    default:
                        Logger.Log($"A(n) {(ElementDamageType)e.Type} effect on you has ended!");
                        break;
                }
            },
            onTick: e =>
            {
                if (e.Source == StatusRecordType.ElementalDamage)
                {
                    TakeDamage(e.Power, (ElementType)e.Type);
                }
            }
        );
        Archetype.RecoverResource();

        foreach (var skill in Archetype.Skills)
        {
            skill.UpdateElapsedTime();
        }
    }
}