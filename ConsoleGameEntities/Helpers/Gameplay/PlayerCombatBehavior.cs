using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers.Gameplay;

public class PlayerCombatBehavior
{
    private static readonly Random _rng = Random.Shared;
    public void Attack(Player self, ITargetable target)
    {
        var weapon = EquippedWeapon(self) ?? throw new EquipmentException("You need to equip a weapon before attacking.");

        var damage = self.Archetype.ArchetypeType == ArchetypeType.Martial ?
            GetStat(self, StatType.Attack) :
            GetStat(self, StatType.Magic);

        self.Logger.Log($"You attack with {weapon.Name} for {damage} damage!");
        target.TakeDamage(damage, DamageType.Martial);

        if (weapon.Rune != null && target is Monster monster)
        {
            monster.RuneAttack(weapon.Rune);
        }

        CheckWeaponDurability(self);
    }
    public void TakeDamage(Player self, int damage, DamageType? type)
    {
        var chance = Math.Min(33, self.DodgeChance + (GetStat(self, StatType.Speed) * 0.01)); //cap of 33% dodge chance
        bool dodged = _rng.Next(0, 100) < chance;

        if (dodged)
        {
            self.Logger.Log($"You dodged the attack and took no damage!");
            return;
        }

        int damageTaken = CalculateDamageTaken(self, damage, type);

        self.CurrentHealth -= damageTaken;
        self.Logger.Log($"You've taken {damageTaken} damage! [{self.CurrentHealth} / {self.MaxHealth}]");

        if (self.CurrentHealth <= 0)
            throw new PlayerDeathException();

        CheckArmorDurability(self, type);
    }
    private int CalculateDamageTaken(Player self, int damage, DamageType? type)
    {
        return type switch
        {
            DamageType.Martial => Math.Max(1, damage - GetStat(self, StatType.Defense)),
            DamageType.Magical => Math.Max(1, damage - GetStat(self, StatType.Resistance)),
            DamageType.Hybrid => Math.Max(1, damage - (Math.Max(GetStat(self, StatType.Defense), GetStat(self, StatType.Resistance)) / 2)),
            _ => damage
        };
    }
    public void TakeElementalDamage(Player self, int damage, ElementType element)
    {
        int damageTaken = damage;
        foreach (var armor in EquippedArmor(self))
        {
            if (armor.Rune != null && armor.Rune.Element == element)
            {
                damageTaken -= armor.Rune.Power;
                break;
            }
        }

        damageTaken = Math.Max(0, damageTaken);
        self.CurrentHealth -= damageTaken;

        self.Logger.Log($"You've taken {damageTaken} {element} damage! [{self.CurrentHealth} / {self.MaxHealth}]");
    }
    public void Heal(Player self, int amount)
    {
        if (self.CurrentHealth + amount > self.MaxHealth)
        {
            amount = self.MaxHealth - self.CurrentHealth;
            self.CurrentHealth = self.MaxHealth;
        }
        else
        {
            self.CurrentHealth += amount;
        }
        self.Logger.Log($"You healed for {amount} health! [{self.CurrentHealth} / {self.MaxHealth}]");
    }

    public int EquippedDefense(Player self)
    {
        var equippedArmor = EquippedArmor(self);

        if (equippedArmor.Count == 0)
            return 0;

        var totalDefense = equippedArmor.Sum(a => a.DefensePower);
        var averageDefense = totalDefense / equippedArmor.Count;

        return averageDefense;
    }
    public int EquippedResistance(Player self)
    {
        var equippedArmor = EquippedArmor(self);

        if (equippedArmor.Count == 0)
            return 0;

        var totalResistance = equippedArmor.Sum(a => a.Resistance);
        var averageResistance = totalResistance / equippedArmor.Count;

        return averageResistance;
    }
    public List<Armor> EquippedArmor(Player self)
    {
        return self.Equipment.OfType<Armor>().ToList() ?? new List<Armor>();
    }
    public int EquippedAttack(Player self)
    {
        return EquippedWeapon(self)?.AttackPower ?? 0;
    }
    public Weapon? EquippedWeapon(Player self)
    {
        return self.Equipment.OfType<Weapon>().FirstOrDefault();
    }

    private void CheckArmorDurability(Player self, DamageType? damageType)
    {
        //randomly select an armor piece to use durability
        var damagedArmor = damageType == DamageType.Martial ?
            EquippedArmor(self).Where(a => a.DefensePower > 0).OrderBy(_ => _rng.Next()).FirstOrDefault()
            : EquippedArmor(self).Where(a => a.Resistance > 0).OrderBy(_ => _rng.Next()).FirstOrDefault();

        if (damagedArmor != null)
        {
            try
            {
                damagedArmor.Use();
            }
            catch (ItemDurabilityException)
            {
                self.Unequip(damagedArmor);
                self.Logger.Log($"Your {damagedArmor.Name} broke and can't be used anymore.");
            }
        }
    }
    private void CheckWeaponDurability(Player self)
    {
        var weapon = self.Equipment.OfType<Weapon>().FirstOrDefault();
        if (weapon != null)
        {
            try
            {
                weapon.Use();
            }
            catch (ItemDurabilityException)
            {
                self.Unequip(weapon);
                self.Logger.Log($"Your {weapon.Name} broke and can't be used anymore.");
            }
        }
    }
    public int GetStat(Player self, StatType stat)
    {
        return stat switch
        {
            StatType.Defense => Math.Max(0, EquippedDefense(self) + self.Archetype.DefenseBonus + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Defense)),
            StatType.Speed => Math.Max(0, self.Archetype.Speed + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Speed)),
            StatType.Attack => Math.Max(0, EquippedAttack(self) + self.Archetype.AttackBonus + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Attack)),
            StatType.Resistance => Math.Max(0, EquippedResistance(self) + self.Archetype.ResistanceBonus + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Resistance)),
            StatType.Health => self.CurrentHealth,
            StatType.Magic => Math.Max(0, EquippedAttack(self) + self.Archetype.MagicBonus + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Magic)),
            _ => throw new StatTypeException("Monster Get Stat")
        };
    }
}
