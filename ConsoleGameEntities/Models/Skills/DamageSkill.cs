using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public abstract class DamageSkill : Skill
{
    public DamageType DamageType { get; set; }
    
    protected SkillResolutionResult ExecuteDamageSkill(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException("This skill is still on cooldown.");
        
        ValidateCasterLevel(caster);
        ValidateDamageTargets(caster, singleEnemy, multipleEnemies);
        
        List<AttackResolutionResult> results = new();
        
        var attack = new AttackDamage(Power, DamageType, 0, null);
        switch (caster)
        {
            case Player player:
                ConsumePlayerResources(player);

                try
                {
                    player.AddActionItem(this);

                    switch (TargetType)
                    {
                        case TargetType.SingleEnemy:
                            results.Add(singleEnemy!.TakeDamage(attack));
                            break;

                        case TargetType.AllEnemies:
                            foreach (var target in multipleEnemies!)
                            {
                                results.Add(target.TakeDamage(attack));
                            }
                            break;
                    }
                }
                finally
                {
                    Reset();
                }
                break;

            case Monster monster:
                try
                {
                    monster.AddActionItem(this);
                    results.Add(singleEnemy!.TakeDamage(attack));
                }
                finally
                {
                    Reset();
                }
                break;

            default:
                throw new InvalidOperationException("Invalid caster type.");
        }

        return new SkillResolutionResult(this, results);
    }

    protected void ValidateDamageTargets(ITargetable caster, ITargetable? singleEnemy, List<ITargetable>? multipleEnemies)
    {
        ValidateTargets(caster, singleEnemy, multipleEnemies);
        
        if (caster is Player && TargetType == TargetType.Self)
            throw new InvalidSkillConfigurationException("Player damage skills cannot target self!");
        
        if (caster is Monster && TargetType != TargetType.SingleEnemy)
            throw new InvalidSkillConfigurationException("Monster damage skills can only target a single enemy");
    }
}