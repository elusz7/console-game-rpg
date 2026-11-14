using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters;

public class BossMonster : Monster
{
    private int _currentPhase = 1;
    private const int MaxPhases = 3;
    private int _baseHealth;
    private int _baseAttack;
    [NotMapped] public override double DodgeChance { get; set; } = 0.01;

    [NotMapped] public override IMonsterStrategy Strategy { get; set; }

    public BossMonster()
    {
    }

    public BossMonster(IMonsterSkillSelector skillSelector)
    {
        Strategy = new BossStrategy(skillSelector);
    }

    public override AttackResolutionResult Attack(IPlayer target)
    {
        return Strategy.ExecuteAttack(this, target);
    }

    private void CheckPhaseChange()
    {
        if (_baseHealth == 0)
            _baseHealth = MaxHealth;

        if (_baseAttack == 0)
            _baseAttack = AttackPower;

        _currentPhase++;
        if (_currentPhase > MaxPhases)
            throw new MonsterDeathException();

        foreach (var skill in Skills.OfType<BossSkill>())
            skill.Phase = _currentPhase;

        var scale = _currentPhase switch
        {
            1 => 1.0,
            2 => 0.75,
            _ => 0.50
        };
        MaxHealth = (int)(_baseHealth * scale);
        AttackPower = (int)(_baseAttack * scale);
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

        AddActionItem($"Boss [{Name}] has entered phase {_currentPhase}!");
    }

    public override AttackResolutionResult TakeDamage(AttackDamage attack)
    {
        var result = base.TakeDamage(attack);

        if (result.TargetDied)
        {
            try
            {
                CheckPhaseChange();

                // The boss survived by transitioning to another phase.
                result = result with { TargetDied = false };
            }
            catch (MonsterDeathException)
            {
                // No phases remain. The boss is actually defeated.
            }
        }

        return result;
    }

    public override void SetLevel(int newLevel)
    {
        _currentPhase = 1;
        base.SetLevel(newLevel);
    }
}