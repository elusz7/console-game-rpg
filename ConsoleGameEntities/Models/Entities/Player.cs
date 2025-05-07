using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGameEntities.Models.Entities;

public class Player : IPlayer
{
    private static readonly Random _rng = new(DateTime.Now.Millisecond);
    public int Experience { get; set; }
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
    public List<IItem> Equipment { get; set; } = new();
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
        if (Archetype.ArchetypeType == ArchetypeType.Martial)
        {
            target.TakeDamage(GetStat(StatType.Attack), DamageType.Martial);
        }
        else if (Archetype.ArchetypeType == ArchetypeType.Magical)
        {
            target.TakeDamage(GetStat(StatType.Magic), DamageType.Magical);
        }
        else
        {
            throw new NotImplementedException();
        }

        CheckWeaponDurability();
    }
    public void TakeDamage(int damage, DamageType? damageType) 
    {
        var chance = Math.Min(33, DodgeChance + (GetStat(StatType.Speed) * 0.01)); //cap of 33% dodge chance
        bool dodged = _rng.Next(0, 100) < chance;

        if (dodged) return;

        int damageTaken = damageType switch
        {
            DamageType.Martial => Math.Max(1, damage - GetStat(StatType.Defense)),
            DamageType.Magical => Math.Max(1, damage - GetStat(StatType.Resistance)),
            DamageType.Hybrid => Math.Max(1, damage - (Math.Max(GetStat(StatType.Defense), GetStat(StatType.Resistance)) / 2)),
            _ => damage
        };

        CurrentHealth -= damageTaken;        

        if (CurrentHealth <= 0)
            throw new PlayerDeathException();

        CheckArmorDurability();
    }
    public void Heal(int regainedHealth) 
    {
        if (CurrentHealth + regainedHealth > MaxHealth)
            CurrentHealth = MaxHealth;
        else
            CurrentHealth += regainedHealth;
    }
    public void LevelUp()
    {
        Level++;
        var boost = Archetype.LevelUp(Level);
        MaxHealth += boost;
        CurrentHealth = MaxHealth;
        Inventory.Capacity += (decimal)boost;
        DodgeChance += 0.002;
        
        foreach (var kvp in ActiveEffects) 
        { 
            var key = kvp.Key;
            ActiveEffects[key] = 0;
        }
    }
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append("Name: ");
        builder.Append(Name);

        builder.Append(", Health: ");
        builder.Append(MaxHealth);

        builder.Append(", Experience: ");
        builder.Append(Experience);

        builder.Append(", Capacity: ");
        builder.Append(Inventory?.Capacity.ToString() ?? "N/A");

        builder.Append(", Gold: ");
        builder.Append(Inventory?.Gold.ToString() ?? "N/A");

        builder.Append("\n\tInventory: ");
        builder.Append(Inventory?.Items != null && Inventory.Items.Any()
            ? string.Join(", ", Inventory.Items.Select(i => i.Name))
            : "Inventory is empty currently.");

        return builder.ToString();
    }
    public int GetStat(StatType stat) {
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
    public void Equip(IItem item)
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
            }

            newWeapon.Equip();
            Equipment.Add(newWeapon);
        }
        else if (item is Armor newArmor)
        {
            var armorPiece = Equipment.OfType<Armor>().Where(a => a.ArmorType == newArmor.ArmorType).FirstOrDefault();

            if (armorPiece != null)
            {
                armorPiece.Unequip();
                Equipment.Remove(armorPiece);
            }

            newArmor.Equip();
            Equipment.Add(newArmor);            
        }
        else
        {
            throw new EquipmentException($"Cannot equip {item.Name}");
        }

        if (item.IsCursed)
            EnactCursedEffect(item);
    }
    private void EnactCursedEffect(IItem item)
    {
        var cursedDeduection = Math.Max(1, (int)Math.Floor(MaxHealth * 0.05));

        CurrentHealth -= cursedDeduection;
        MaxHealth -= cursedDeduection;

        item.IsCursed = false;
    }
    public void ModifyStat(StatType stat, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else if (ActiveEffects.ContainsKey(stat))
            ActiveEffects[stat] += amount;
        else
            throw new StatTypeException("Invalid stat to modify");
    }
    private int EquippedDefense()
    {
        var equippedArmor = Equipment.OfType<Armor>().ToList();

        if (equippedArmor.Count == 0)
            return 0;

        var totalDefense = equippedArmor.Sum(a => a.DefensePower);
        var averageDefense = totalDefense / equippedArmor.Count;

        return averageDefense;
    }
    private int EquippedResistance()
    {
        var equippedArmor = Equipment.OfType<Armor>().ToList();

        if (equippedArmor.Count == 0)
            return 0;

        var totalResistance = equippedArmor.Sum(a => a.Resistance);
        var averageResistance = totalResistance / equippedArmor.Count;

        return averageResistance;
    }
    private int EquippedAttack()
    {
        return Equipment.OfType<Weapon>().First()?.AttackPower ?? 0;
    }
    public void Loot(IMonster monster)
    {
        if (monster.CurrentHealth >= 1)
        {
            throw new TreasureException($"You think it might be better to defeat {monster.Name} before trying to take its treasure.");
        }

        if (monster.Treasure == null)
            throw new TreasureException($"You rifle through {monster.Name}'s pockets, but it hasn't got any treasure.");

        try
        {
            Inventory.AddItem(monster.Treasure);            
        }
        catch (InventoryException ex)
        {
            throw new TreasureException(ex.Message);
        }

        if (monster is Monster m)
            m.ItemId = null;

        monster.Treasure = null;
    }
    public void Unequip(IItem item)
    {
        Equipment.Remove(item);
        item.Unequip();
    }
    private void CheckArmorDurability()
    {
        var damagedArmor = Equipment.OfType<Armor>().OrderBy(_ => _rng.Next()).FirstOrDefault();
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
            }
        }
    }
    private void CheckWeaponDurability()
    {
        var weapon = Equipment.OfType<Armor>().FirstOrDefault();
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
            }
        }
    }
}