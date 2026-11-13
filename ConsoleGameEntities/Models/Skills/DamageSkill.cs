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
    
    protected void ExecuteDamageSkill(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException("This skill is still on cooldown.");
        
        ValidateCasterLevel(caster);
        ValidateDamageTargets(caster, singleEnemy, multipleEnemies);
        
        bool monsterDeath = false;
        
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
                            try
                            {
                                singleEnemy!.TakeDamage(attack);
                            }
                            catch (MonsterDeathException)
                            {
                                monsterDeath = true;
                            }
                            break;

                        case TargetType.AllEnemies:
                            foreach (var target in multipleEnemies!)
                            {
                                try
                                {
                                    target.TakeDamage(attack);
                                }
                                catch (MonsterDeathException)
                                {
                                    monsterDeath = true;
                                }
                            }
                            break;
                    }
                }
                finally
                {
                    Reset();
                }

                if (monsterDeath)
                    throw new MonsterDeathException();

                break;

            case Monster monster:
                try
                {
                    monster.AddActionItem(this);
                    singleEnemy!.TakeDamage(attack);
                }
                finally
                {
                    Reset();
                }
                break;

            default:
                throw new InvalidOperationException("Invalid caster type.");
        }
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