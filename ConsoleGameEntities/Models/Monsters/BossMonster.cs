using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
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
    [NotMapped]
    public override double DodgeChance { get; set; } = 0.03;

    [NotMapped]
    public override IMonsterStrategy Strategy { get; set; }

    public BossMonster() { }
    public BossMonster(IMonsterSkillSelector skillSelector)
    {
        Strategy = new BossStrategy(skillSelector);
    }
    public void CheckPhaseChange()
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
        Logger.Log($"Boss [{Name}] has entered phase {CurrentPhase}!");
    }
    public override void TakeDamage(int damage, DamageType? damageType)
    {
        Combat.TakeDamage(this, damage, damageType);
    }
    public override void SetLevel(int newLevel)
    {
        CurrentPhase = 1;
        base.SetLevel(newLevel);
    }
}


