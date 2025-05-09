using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public class BossMonster : Monster
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());

    private int CurrentPhase = 1;
    private const int MaxPhases = 3;
    private readonly int BaseHealth;
    private readonly int BaseAttack;
    private double DodgeChance { get; set; } = 0.01;

    [NotMapped]
    public override IMonsterStrategy Strategy { get; set; } = new BossStrategy();    

    public BossMonster()
    {
        BaseHealth = MaxHealth;
        BaseAttack = AttackPower;
    }
    public override void Attack(IPlayer target)
    {
        Strategy.ExecuteAttack(this, target);
    }

    private void CheckPhaseChange()
    {
        CurrentPhase++;
        if (CurrentPhase > MaxPhases)
            throw new MonsterDeathException();

        foreach (var skill in Skills?.OfType<BossSkill>() ?? new List<BossSkill>())
            skill.Phase = CurrentPhase;

        double scale = Math.Pow(0.75, CurrentPhase - 1);
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
    }

    public override void TakeDamage(int damage, DamageType? damageType)
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
            CheckPhaseChange();
    }
    public override string ToString()
    {
        return base.ToString();
    }
}


