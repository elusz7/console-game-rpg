using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class UltimateSkill : DamageSkill
{
    // "Cooldown" here means the charge-up time before the ultimate can be used again.
    // ElapsedTime must meet or exceed Cooldown.
    [NotMapped] public bool IsReady => ElapsedTime >= Cooldown;
    private int _previousScaledLevel;
    private int _basePower;
    private int _baseCooldown;

    public override void InitializeSkill(int level)
    {
        _basePower = Power;
        _baseCooldown = Cooldown;
        _previousScaledLevel = int.MinValue;

        Reset();
        ScalePowerWithLevel(level);
    }

    public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        ValidateCasterLevel(caster);
        ValidateDamageTargets(caster, singleEnemy, multipleEnemies);

        if (caster is Player p)
            ScalePowerWithLevel(p.Level);
        else if (caster is Monster m)
            ScalePowerWithLevel(m.Level);

        if (!IsReady)
            throw new SkillCooldownException();
        
        var attack = new AttackDamage(Power, DamageType, 0, null);
        List<AttackResolutionResult> results = new();
        
        if (caster is Player player)
        {
            results.AddRange(PlayerCast(player, attack, singleEnemy, multipleEnemies));
        }
        else if (caster is Monster monster)
        {
            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                    monster.AddActionItem(this);
                    results.Add(singleEnemy!.TakeDamage(attack));
                    Reset();
                    break;
            }
        }

        return new SkillResolutionResult(this, results);
    }

    private List<AttackResolutionResult> PlayerCast(Player player, AttackDamage attack, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        ConsumePlayerResources(player);

        List<AttackResolutionResult> results = new();

        switch (TargetType)
        {
            case TargetType.SingleEnemy when singleEnemy is Monster monster:
                player.AddActionItem(this);
                results.Add(monster.TakeDamage(attack));
                break;

            case TargetType.AllEnemies:
                player.AddActionItem(this);

                foreach (var target in multipleEnemies!)
                {
                    results.Add(target.TakeDamage(attack));
                }
                break;
        }
        
        Reset();
        
        return results;
    }

    private void ScalePowerWithLevel(int level)
    {
        if (level == _previousScaledLevel)
            return;
    
        var levelScale = Math.Max(1, level - 3);
        var powerMultiplier = (int)(1 + (Math.Pow(levelScale, 1.2) / 5));
    
        Power = _basePower * powerMultiplier;
        Cooldown = _baseCooldown + level % 2;
    
        _previousScaledLevel = level;
    }

    public override UltimateSkill Clone()
    {
        var clone = new UltimateSkill
        {
            DamageType = DamageType,
            _previousScaledLevel = _previousScaledLevel,
            _basePower = _basePower,
            _baseCooldown = _baseCooldown
        };

        CopyTo(clone);

        return clone;
    }
}