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
    [NotMapped]
    public Dictionary<int, string> ActionItems { get; } = new();
    [NotMapped]

    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
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
    public List<Item> Equipment { get; set; } = new();
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
    public void Heal(int regainedHealth)
    {
        if (CurrentHealth + regainedHealth > MaxHealth)
        {            
            CurrentHealth = MaxHealth;
            AddActionItem($"You healed for {MaxHealth - CurrentHealth} health! [{CurrentHealth} / {MaxHealth}]");
        }
        else
        {
            CurrentHealth += regainedHealth;
            AddActionItem($"You healed for {regainedHealth} health! [{CurrentHealth} / {MaxHealth}]");
        }
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

        if (item.IsCursed)
            EnactCursedEffect(item);
    }
    private void EnactCursedEffect(IItem item)
    {
        var cursedDeduction = Math.Max(1, (int)Math.Floor(MaxHealth * 0.05));

        CurrentHealth -= cursedDeduction;
        MaxHealth -= cursedDeduction;

        AddActionItem($"You have been cursed! Your max health has been lowered by {cursedDeduction}!");

        item.IsCursed = false;
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

        try
        {
            var loot = monster.Loot();
            if (loot != null)
            {
                Inventory.AddItem(loot);
                AddActionItem($"You found {loot.Name} on {monster.Name}!");
            }

        }
        catch (InventoryException ex)
        {
            throw new TreasureException(ex.Message);
        }
    }
    public void Unequip(Item item)
    {
        Equipment.Remove(item);
        item.Unequip();
        AddActionItem($"You unequipped {item.Name}.");
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
    public void AddActionItem(string actionItem)
    {
        ActionItems.Add(DateTime.Now.Millisecond, actionItem);
    }
    public void AddActionItem(ISkill skill)
    {
        ActionItems.Add(DateTime.Now.Millisecond, $"You use {skill.Name}!");
    }
    public virtual void Sell(Item item)
    {
        if (item.IsEquipped())
            throw new InvalidOperationException("Cannot sell an equipped item.");

        if (Inventory != null)
            Inventory.Gold += (int)Math.Floor(item.Value * 0.75M);

        Inventory?.RemoveItem(item);
    }
    public virtual void Buy(Item item)
    {
        var price = (int)Math.Floor(item.Value * 1.25M);

        if (Inventory.Gold < price)
            throw new ItemPurchaseException($"You are short by {price - Inventory.Gold} gold.");

        try
        {
            Inventory.AddItem(item);
        }
        catch (InventoryException ex)
        {
            throw new ItemPurchaseException(ex.Message);
        }

        Inventory.Gold -= price;
    }
}