using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Helpers.Gameplay;

public class MonsterCombatBehavior
{
    private static readonly Random _rng = Random.Shared;

    public void Attack(Monster self, IPlayer target, IMonsterStrategy strategy)
    {
        strategy.ExecuteAttack(self, target);
    }

    public void TakeDamage(Monster self, int damage, DamageType? type)
    {
        double dodgeChance = Math.Min(33, 0.002 * self.DodgeChance + (GetStat(self, StatType.Speed) * 0.01));
        if (_rng.NextDouble() * 100 < dodgeChance)
        {
            self.Logger.Log($"{self.Name} dodged the attack and took no damage!");
            return;
        }

        int damageTaken = CalculateDamageTaken(self, damage, type);
        damageTaken += self.Effects.SumByCondition(StatusRecordType.ElementalStatus, (int)ElementalStatusEffectType.Blinded);

        self.CurrentHealth -= damageTaken;
        self.Logger.Log($"{self.Name} takes {damageTaken} damage!");

        if (self.CurrentHealth <= 0 && self is not BossMonster)
        {
            self.Logger.Log($"{self.Name} is now dead.");
            throw new MonsterDeathException();
        }
        else if (self.CurrentHealth <= 0 && self is BossMonster boss)
        {
            boss.CheckPhaseChange();
        }
    }

    private int CalculateDamageTaken(Monster self, int damage, DamageType? type)
    {
        return type switch
        {
            DamageType.Martial => Math.Max(1, damage - GetStat(self, StatType.Defense)),
            DamageType.Magical => Math.Max(1, damage - GetStat(self, StatType.Resistance)),
            DamageType.Hybrid => Math.Max(1, damage - (Math.Max(GetStat(self, StatType.Defense), GetStat(self, StatType.Resistance)) / 2)),
            _ => damage
        };
    }

    public void TakeElementalDamage(Monster self, int damage, ElementType element)
    {
        if (self.AttackElement == element)
            damage = (int)(damage * 0.5);
        else if (self.Vulnerability == element)
            damage = (int)(damage * 1.5);

        damage = Math.Max(0, damage);
        self.CurrentHealth -= damage;

        self.Logger.Log($"{self.Name} takes {damage} {element} damage!");

        if (self.CurrentHealth <= 0 && self is not BossMonster)
        {
            self.Logger.Log($"{self.Name} is now dead.");
            throw new MonsterDeathException();
        }
        else if (self.CurrentHealth <= 0 && self is BossMonster boss)
        {
            boss.CheckPhaseChange();
        }
    }

    public void Heal(Monster self, int amount)
    {
        int frozenPenalty = self.Effects.SumByCondition(StatusRecordType.ElementalStatus, (int)ElementalStatusEffectType.Frozen);
        int actualHeal = Math.Max(0, amount - frozenPenalty);

        if (self.CurrentHealth + actualHeal > self.MaxHealth)
        {
            self.Logger.Log($"{self.Name} heals for {self.MaxHealth - self.CurrentHealth} health!");
            self.CurrentHealth = self.MaxHealth;
        }
        else
        {
            self.Logger.Log($"{self.Name} heals for {actualHeal} health!");
            self.CurrentHealth += actualHeal;
        }
    }

    public void ApplyRuneEffect(Monster self, WeaponRune rune)
    {
        (int damage, bool statusEffect) = rune.Use();

        self.Effects.AddEffect(StatusRecordType.ElementalDamage, (int)rune.Element, rune.Duration, damage);
        self.Logger.Log($"{self.Name} is now {rune.Description}!");

        if (statusEffect)
        {
            int power = (int)rune.Rarity + rune.Tier;
            self.Effects.AddEffect(StatusRecordType.ElementalStatus, (int)rune.StatusEffect, rune.Duration, power);
            self.Logger.Log($"{self.Name} has been {rune.StatusEffect}!");
        }
    }

    public virtual int GetStat(Monster self, StatType stat)
    {
        //ElementalStatusEffects
        //charred, frozen, shocked, snared, blinded, corrupted
        //charred reduces defense
        // snared reduces speed
        // shocked reduces resistance
        // blinded increases damage taken
        // corrupted reduces damage output
        // frozen reduces health regain

        var damageEffects = self.DamageType switch
        {
            DamageType.Martial => self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Attack),
            DamageType.Magical => self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Magic),
            DamageType.Hybrid => (int)Math.Round(self.Effects.SumByCondition(StatusRecordType.Skill, ((int)StatType.Attack) + self.Effects.SumByCondition(StatusRecordType.Skill, (int)StatType.Magic)) / 2.0),
            _ => throw new InvalidDataException("Damage Type not properly mapped to a enum")
        };

        //corrupted status effect should reduce damage effects
        damageEffects -= self.Effects.SumByCondition(StatusRecordType.ElementalStatus, (int)ElementalStatusEffectType.Corrupted);

        return stat switch
        {
            StatType.Defense => Math.Max(0, self.DefensePower + self.Effects.SumByEffect(stat)),
            StatType.Speed => Math.Max(0, self.AggressionLevel + self.Effects.SumByEffect(stat)),
            StatType.Attack or StatType.Magic => Math.Max(0, self.AttackPower + damageEffects),
            StatType.Resistance => Math.Max(0, self.Resistance + self.Effects.SumByEffect(stat)),
            StatType.Health => self.CurrentHealth,
            _ => throw new StatTypeException("Monster Get Stat")
        };
    }
}
