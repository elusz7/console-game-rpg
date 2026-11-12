using System.Text;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Models.Items;
using System.Numerics;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Runes;

namespace ConsoleGameEntities.Models.Entities;

public class Player : IPlayer
{
    [NotMapped] public ActiveEffectManager ActiveEffectManager { get; } = new();
    [NotMapped] public Dictionary<long, string> ActionItems { get; } = new();
    [NotMapped] private static readonly Random Random = Random.Shared;
    [NotMapped] private static readonly DamageResolver DamageResolver = new();
    public int Level { get; set; }
    public int Id { get; set; }
    public string Name { get; set; }
    [NotMapped] public int CurrentHealth { get; set; }
    [NotMapped] public double DodgeChance { get; set; } = 0.01;
    public int MaxHealth { get; set; }
    public int ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public virtual Inventory Inventory { get; set; }
    public int? RoomId { get; set; }
    public virtual Room? CurrentRoom { get; set; }
    [NotMapped] public List<IEquippable> Equipment { get; set; } = [];

    [NotMapped] private readonly Dictionary<StatType, int> _activeEffects = new()
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

        var damage = Archetype.ArchetypeType == ArchetypeType.Martial
            ? GetStat(StatType.Attack)
            : GetStat(StatType.Magic);

        var elementalDamage = weapon.UseRune();
        ElementType? element = null;
        if (elementalDamage != 0)
            element = weapon.EquippedRune!.InventoryRune.Rune.Element;
        
        var attack = new AttackDamage(damage, weapon.DamageType, elementalDamage, element);

        AddActionItem($"You attack with {weapon.Name}!");
        if (element != null)
            AddActionItem($"Your weapon is charged with primordial energy!\nIt deals an extra {elementalDamage} points of {element} damage!");
        
        target.TakeDamage(attack);

        CheckWeaponDurability();
    }

    public virtual void TakeDamage(AttackDamage attack)
    {
        var equippedResilience = Equipment
            .OfType<Armor>()
            .Where(a => a.EquippedRune != null &&
                        a.EquippedRune.InventoryRune.Rune.Element == attack.Element)
            .Sum(a => a.UseRune());
        
        var modifiers = new PlayerDamageModifiers(
            GetStat(StatType.Defense),
            GetStat(StatType.Resistance),
            equippedResilience);
            
        var result = DamageResolver.ResolvePlayerDamage(attack, modifiers, DodgeChance + (GetStat(StatType.Speed) * 0.01));

        if (result.IsDodged)
        {
            AddActionItem("You dodged the attack and took no damage!");
            return;
        }
        AddActionItem($"You've taken {result.FinalStandardDamage} damage! [{CurrentHealth} / {MaxHealth}]");

        if (result.FinalElementalDamage != 0)
        {
            AddActionItem($"You've taken {result.FinalElementalDamage} {result.Element} damage! [{CurrentHealth} / {MaxHealth}]");
        }

        if (result.IncomingElementalDamage != 0 && result.FinalElementalDamage == 0)
        {
            AddActionItem($"You shrug off the {result.Element} damage! [{CurrentHealth} / {MaxHealth}]");
        }
        
        CurrentHealth -= (result.FinalStandardDamage + result.FinalElementalDamage);
        if (CurrentHealth <= 0)
            throw new PlayerDeathException();

        CheckArmorDurability(result.StandardDamageType);
    }

    public virtual void Heal(int regainedHealth)
    {
        if (regainedHealth < 0)
            throw new ArgumentOutOfRangeException(
                nameof(regainedHealth),
                "Healing amount cannot be negative.");

        var frozenReduction = ActiveEffectManager.GetHealingReduction();
        var actualHealing = Math.Max(0, regainedHealth - frozenReduction);

        var healedAmount = Math.Min(actualHealing, MaxHealth - CurrentHealth);

        CurrentHealth += healedAmount;

        AddActionItem($"You healed for {healedAmount} health! [{CurrentHealth} / {MaxHealth}]");
    }

    public void LevelUp()
    {
        Level++;

        var boost = Archetype.LevelUp(Level);
        MaxHealth += boost;
        CurrentHealth = MaxHealth;

        Inventory.Gold += (Level * Random.Next(8, 13));
        Inventory.Capacity += Math.Round(boost * 0.8M, 2);
        DodgeChance += 0.003;
        
        foreach (var kvp in _activeEffects)
        {
            var key = kvp.Key;
            _activeEffects[key] = 0;
        }
    }

    public int GetStat(StatType stat)
    {
        return stat switch
        {
            StatType.Defense => Math.Max(0,
                EquippedDefense() + Archetype.DefenseBonus + _activeEffects[StatType.Defense] + ActiveEffectManager.GetStatModifier(StatType.Defense)),
            StatType.Speed => Math.Max(0, Archetype.Speed + _activeEffects[StatType.Speed] + ActiveEffectManager.GetStatModifier(StatType.Speed)),
            StatType.Attack => Math.Max(0, EquippedAttack() + Archetype.AttackBonus + _activeEffects[StatType.Attack] + ActiveEffectManager.GetStatModifier(StatType.Attack)),
            StatType.Resistance => Math.Max(0, EquippedResistance() + Archetype.ResistanceBonus + _activeEffects[StatType.Resistance] + ActiveEffectManager.GetStatModifier(StatType.Resistance)),
            StatType.Health => CurrentHealth,
            StatType.Magic => Math.Max(0, EquippedAttack() + Archetype.MagicBonus + _activeEffects[StatType.Magic] + ActiveEffectManager.GetStatModifier(StatType.Magic)),
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

    public void RebuildEquipment()
    {
        Equipment.Clear();

        var equippedItems = Inventory.Items.OfType<IEquippable>()
            .Where(i => i.IsEquipped());

        Equipment.AddRange(equippedItems);
    }

    private void EnactCursedEffect(ICursable cursedItem)
    {
        var cursedDeduction = Math.Max(1, (int)Math.Floor(MaxHealth * 0.1));

        CurrentHealth -= cursedDeduction;
        MaxHealth -= cursedDeduction;

        AddActionItem($"You have been cursed! Your max health has been lowered by {cursedDeduction}!");

        cursedItem.TargetCursed();
        
        if (CurrentHealth <= 0)
            throw new PlayerDeathException();
    }

    public void ModifyStat(StatType stat, int amount)
    {
        if (stat == StatType.Health)
            Heal(amount);
        else if (_activeEffects.ContainsKey(stat))
        {
            _activeEffects[stat] += amount;
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
        return [..Equipment.OfType<Armor>()];
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
            throw new TreasureException(
                $"You think it might be better to defeat {monster.Name} before trying to take its treasure.");
        }

        var loot = monster.Loot() ?? throw new TreasureException($"{monster.Name} has no treasure to loot.");

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
        if (item is IEquippable equippableItem && Equipment.Contains(equippableItem))
        {
            Equipment.Remove(equippableItem);
            equippableItem.Unequip();
            AddActionItem($"You unequipped {item.Name}.");
        }
    }

    private void CheckArmorDurability(DamageType? damageType)
    {
        var armor = Equipment.OfType<Armor>();

        var damagedArmor = damageType switch
        {
            DamageType.Martial => armor
                .Where(a => a.DefensePower > 0)
                .OrderBy(_ => Random.Next())
                .FirstOrDefault(),

            DamageType.Magical => armor
                .Where(a => a.Resistance > 0)
                .OrderBy(_ => Random.Next())
                .FirstOrDefault(),

            DamageType.Hybrid => GetHybridDamageArmor(armor),

            _ => null
        };

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
    
    /// <summary>
    /// Selects armor that contributes to the higher defense or resistance
    /// value used when mitigating hybrid damage.
    /// </summary>
    private Armor? GetHybridDamageArmor(IEnumerable<Armor> armor)
    {
        var defense = GetStat(StatType.Defense);
        var resistance = GetStat(StatType.Resistance);

        var relevantArmor = defense > resistance
            ? armor.Where(a => a.DefensePower > 0)
            : resistance > defense
                ? armor.Where(a => a.Resistance > 0)
                : armor.Where(a => a.DefensePower > 0 || a.Resistance > 0);

        return relevantArmor
            .OrderBy(_ => Random.Next())
            .FirstOrDefault();
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
        var key = DateTime.Now.Ticks;
        while (ActionItems.ContainsKey(key)) key++;

        ActionItems.Add(key, action);
    }

    public void AddActionItem(Skill skill)
    {
        var key = DateTime.Now.Ticks;
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
            Gold = Random.Next(15, 24),
            Capacity = (decimal)Math.Round((Random.NextDouble() * 10 + 15), 2),
            Items = [],
            Runes = [],
            Ingredients = [],
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