using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Models.Items;
using System.Numerics;
using ConsoleGameEntities.Interfaces.ItemAttributes;

namespace ConsoleGameEntities.Models.Entities;

public class Player : IPlayer
{
    [NotMapped]
    public Dictionary<long, string> ActionItems { get; } = new();
    [NotMapped]

    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    [NotMapped]
    public int CurrentHealth { get; set; }
    private double DodgeChance { get; set; } = 0.01;
    public int MaxHealth { get; set; }
    public int ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }
    [NotMapped]
    public List<IEquippable> Equipment { get; set; } = new();
    [NotMapped]
    private readonly Dictionary<StatType, int> ActiveEffects = new()
    {
        { StatType.Defense, 0 },
        { StatType.Resistance, 0 },
        { StatType.Speed, 0 },
        { StatType.Attack, 0 },
        { StatType.Magic, 0 },
    };
    public void Attack(ITargetable target)
    {
        var weapon = EquippedWeapon() ?? throw new EquipmentException("You need to equip a weapon before attacking.");

        var damage = Archetype.ArchetypeType == ArchetypeType.Martial ?
            GetStat(StatType.Attack) :
            GetStat(StatType.Magic);
        
        AddActionItem($"You attack with {weapon.Name} for {damage} damage!");
        target.TakeDamage(damage, DamageType.Martial);
        
        CheckWeaponDurability();
    }
    public void TakeDamage(int damage, DamageType? damageType)
    {
        var chance = Math.Min(33, DodgeChance + (GetStat(StatType.Speed) * 0.01)); //cap of 33% dodge chance
        bool dodged = _rng.Next(0, 100) < chance;

        if (dodged)
        {
            AddActionItem($"You dodged the attack and took no damage!");
            return;
        }

        int damageTaken = damageType switch
        {
            DamageType.Martial => Math.Max(1, damage - GetStat(StatType.Defense)),
            DamageType.Magical => Math.Max(1, damage - GetStat(StatType.Resistance)),
            DamageType.Hybrid => Math.Max(1, damage - (Math.Max(GetStat(StatType.Defense), GetStat(StatType.Resistance)) / 2)),
            _ => damage
        };

        
        CurrentHealth -= damageTaken;
        AddActionItem($"You've taken {damageTaken} damage! [{CurrentHealth} / {MaxHealth}]");

        if (CurrentHealth <= 0)
            throw new PlayerDeathException();

        CheckArmorDurability(damageType);
    }
    public virtual void Heal(int regainedHealth)
    {
        int healedAmount;
        if (CurrentHealth + regainedHealth > MaxHealth)
        {
            healedAmount = MaxHealth - CurrentHealth;
            CurrentHealth = MaxHealth;
        }
        else
        {
            healedAmount = regainedHealth;
            CurrentHealth += regainedHealth;
        }
        AddActionItem($"You healed for {healedAmount} health! [{CurrentHealth} / {MaxHealth}]");
    }
    public void LevelUp()
    {
        Level++;
        var boost = Archetype.LevelUp(Level);
        MaxHealth += boost;
        CurrentHealth = MaxHealth;
        Inventory.Gold += Level * _rng.Next(8,13);
        Inventory.Capacity += boost * 0.8M;
        DodgeChance += 0.002;

        foreach (var kvp in ActiveEffects)
        {
            var key = kvp.Key;
            ActiveEffects[key] = 0;
        }
    }
    public int GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.Defense => Math.Max(0, EquippedDefense() + Archetype.DefenseBonus + ActiveEffects[StatType.Defense]),
            StatType.Speed => Math.Max(0, Archetype.Speed + ActiveEffects[StatType.Speed]),
            StatType.Attack => Math.Max(0, EquippedAttack() + Archetype.AttackBonus + ActiveEffects[StatType.Attack]),
            StatType.Resistance => Math.Max(0, EquippedResistance() + Archetype.ResistanceBonus + ActiveEffects[StatType.Resistance]),
            StatType.Health => CurrentHealth,
            StatType.Magic => Math.Max(0, EquippedAttack() + Archetype.MagicBonus + ActiveEffects[StatType.Magic]),
            _ => throw new StatTypeException("Monster Get Stat")
        };
    }
    public void Equip(Item item)
    {
        if (item.Inventory?.Player != this)
            throw new ItemNotFoundException("This item is not in your inventory.");

        if (item.RequiredLevel > Level)
            throw new EquipmentException("Your level is too low to equip this item");

        if (item.Durability < 1)
            throw new EquipmentException("This item is broken and cannot be equipped");

        if (item is Weapon newWeapon)
        {
            if ((newWeapon.DamageType == DamageType.Martial && Archetype.ArchetypeType == ArchetypeType.Magical)
                || (newWeapon.DamageType == DamageType.Magical && Archetype.ArchetypeType == ArchetypeType.Martial))
                throw new EquipmentException($"You cannot equip weapons that deal {newWeapon.DamageType} damage");

            //can only have 1 weapon equipped
            var weapon = Equipment.OfType<Weapon>().FirstOrDefault();

            if (weapon != null)
            {
                weapon.Unequip();
                Equipment.Remove(weapon);
                AddActionItem($"You unequipped {weapon.Name}.");
            }

            newWeapon.Equip();
            Equipment.Add(newWeapon);
            AddActionItem($"You equipped {newWeapon.Name}.");
        }
        else if (item is Armor newArmor)
        {
            var armorPiece = Equipment.OfType<Armor>().Where(a => a.ArmorType == newArmor.ArmorType).FirstOrDefault();

            if (armorPiece != null)
            {
                armorPiece.Unequip();
                Equipment.Remove(armorPiece);
                AddActionItem($"You unequipped {armorPiece.Name}.");
            }

            newArmor.Equip();
            Equipment.Add(newArmor);
            AddActionItem($"You equipped {newArmor.Name}.");
        }
        else
        {
            throw new EquipmentException($"Cannot equip {item.Name}");
        }

        if (item is ICursable c && c.IsCursed())
            EnactCursedEffect(c);
    }
    private void EnactCursedEffect(ICursable cursedItem)
    {
        var cursedDeduction = Math.Max(1, (int)Math.Floor(MaxHealth * 0.1));

        CurrentHealth -= cursedDeduction;
        MaxHealth -= cursedDeduction;

        AddActionItem($"You have been cursed! Your max health has been lowered by {cursedDeduction}!");

        cursedItem.TargetCursed();
    }
    public void ModifyStat(StatType stat, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else if (ActiveEffects.ContainsKey(stat))
        {
            ActiveEffects[stat] += amount;
            AddActionItem($"Your {stat} has been modified by {amount}.");
        }
        else
            throw new StatTypeException("Invalid stat to modify");
    }
    private int EquippedDefense()
    {
        var equippedArmor = EquippedArmor();

        if (equippedArmor.Count == 0)
            return 0;

        var totalDefense = equippedArmor.Sum(a => a.DefensePower);
        var averageDefense = totalDefense / equippedArmor.Count;

        return averageDefense;
    }
    private int EquippedResistance()
    {
        var equippedArmor = EquippedArmor();

        if (equippedArmor.Count == 0)
            return 0;

        var totalResistance = equippedArmor.Sum(a => a.Resistance);
        var averageResistance = totalResistance / equippedArmor.Count;

        return averageResistance;
    }
    private List<Armor> EquippedArmor()
    {
        return Equipment.OfType<Armor>().ToList() ?? new List<Armor>();
    }
    private int EquippedAttack()
    {
        return EquippedWeapon()?.AttackPower ?? 0;
    }
    private Weapon? EquippedWeapon()
    {
        return Equipment.OfType<Weapon>().FirstOrDefault();
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

        AddActionItem($"You found {loot.Name} on {monster.Name}!");
    }
    public void Unequip(Item item)
    {
        if (item is IEquippable equippableItem)
        {
            Equipment.Remove(equippableItem);
            equippableItem.Unequip();
            AddActionItem($"You unequipped {item.Name}.");
        }        
    }
    private void CheckArmorDurability(DamageType? damageType)
    {
        //randomly select an armor piece to use durability
        var damagedArmor = damageType == DamageType.Martial ? 
            Equipment.OfType<Armor>().Where(a => a.DefensePower > 0).OrderBy(_ => _rng.Next()).FirstOrDefault()
            : Equipment.OfType<Armor>().Where(a => a.Resistance > 0).OrderBy(_ => _rng.Next()).FirstOrDefault();

        if (damagedArmor != null)
        {
            try
            {
                damagedArmor.Use();
            }
            catch (ItemDurabilityException)
            {
                Equipment.Remove(damagedArmor);
                damagedArmor.Unequip();
                AddActionItem($"Your {damagedArmor.Name} broke and can't be used anymore.");
            }
        }
    }
    private void CheckWeaponDurability()
    {
        var weapon = Equipment.OfType<Weapon>().FirstOrDefault();
        if (weapon != null)
        {
            try
            {
                weapon.Use();
            }
            catch (ItemDurabilityException)
            {
                Equipment.Remove(weapon);
                weapon.Unequip();
                AddActionItem($"Your {weapon.Name} broke and can't be used anymore.");
            }
        }
    }
    public void ClearActionItems()
    {
        ActionItems.Clear();
    }
    public virtual void AddActionItem(string action)
    {
        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, action);
    }
    public void AddActionItem(Skill skill)
    {
        long key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, $"You use {skill.Name}!");
    }
    public virtual void Sell(Item item)
    {
        if (item is IEquippable e && e.IsEquipped())
            throw new InvalidOperationException("Cannot sell an equipped item.");

        Inventory.Sell(item);
    }
    public virtual void Buy(Item item)
    {
        Inventory.Buy(item);
    }
    public virtual void Initialize()
    {
        Level = 1;
        MaxHealth = (int)Math.Round(Archetype.HealthBase * 1.5);

        Inventory = new Inventory
        {
            Gold = _rng.Next(15, 24),
            Capacity = (decimal)Math.Round((_rng.NextDouble() * 10 + 15), 2),
            Items = new List<Item>(),
            Player = this
        };

        foreach (var skill in Archetype.Skills)
        {
            skill.InitializeSkill(Level);
        }

        CurrentHealth = MaxHealth;
        Archetype.CurrentResource = Archetype.MaxResource;
    }
    public virtual void RecoverResource(int recoveryPower)
    {
        Archetype.RecoverResource(recoveryPower);
    }
}