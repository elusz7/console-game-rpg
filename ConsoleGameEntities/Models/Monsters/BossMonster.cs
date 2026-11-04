using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public class BossMonster : Monster
{
    private static readonly Random _rng = Random.Shared;

    private int CurrentPhase = 1;
    private const int MaxPhases = 3;
    private int BaseHealth = 0;
    private int BaseAttack = 0;
    public override double DodgeChance { get; set; } = 0.01;

    [NotMapped] public override IMonsterStrategy Strategy { get; set; }

    public BossMonster()
    {
    }

    public BossMonster(IMonsterSkillSelector skillSelector)
    {
        Strategy = new BossStrategy(skillSelector);
    }

    public override void Attack(IPlayer target)
    {
        Strategy.ExecuteAttack(this, target);
    }

    private void CheckPhaseChange()
    {
        if (BaseHealth == 0)
            BaseHealth = MaxHealth;

        if (BaseAttack == 0)
            BaseAttack = AttackPower;

        CurrentPhase++;
        if (CurrentPhase > MaxPhases)
            throw new MonsterDeathException();

        foreach (var skill in Skills.OfType<BossSkill>())
            skill.Phase = CurrentPhase;

        var scale = CurrentPhase switch
        {
            1 => 1.0,
            2 => 0.75,
            _ => 0.50
        };
        MaxHealth = (int)(BaseHealth * scale);
        AttackPower = (int)(BaseAttack * scale);
        CurrentHealth = MaxHealth;

        AggressionLevel += 5;
        if (DamageType == DamageType.Martial)
        {
            DefensePower += 2;
            Resistance += 1;
        }
        else
        {
            DefensePower += 1;
            Resistance += 2;
        }

        AddActionItem($"Boss [{Name}] has entered phase {CurrentPhase}!");
    }

    public override void TakeDamage(int damage, DamageType? damageType)
    {
        // Validate and calculate damage before the dodge check.
        var damageTaken = DamageCalculator.CalculateDamage(
            damage,
            damageType,
            GetStat(StatType.Defense),
            GetStat(StatType.Resistance));

        var chance = Math.Min(33, DodgeChance + (GetStat(StatType.Speed) * 0.01));
        var dodged = _rng.NextDouble() * 100 < chance;

        if (dodged)
        {
            AddActionItem($"{Name} dodged the attack!");
            return;
        }

        AddActionItem($"{Name} takes {damageTaken} damage!");
        CurrentHealth -= damageTaken;

        // Bosses transition phases instead of dying immediately.
        if (CurrentHealth <= 0)
            CheckPhaseChange();
    }

    public override void SetLevel(int newLevel)
    {
        CurrentPhase = 1;
        base.SetLevel(newLevel);
    }
}