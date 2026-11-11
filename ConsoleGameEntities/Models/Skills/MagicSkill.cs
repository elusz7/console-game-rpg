using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MagicSkill : DamageSkill
{
    public override void Activate(ITargetable? caster, ITargetable? singleEnemy = null,
        List<ITargetable>? multipleEnemies = null)
    {
        bool monsterDeath = false;

        if (IsOnCooldown)
            throw new SkillCooldownException("This skill is still on cooldown.");

        if (singleEnemy == null && (multipleEnemies == null || multipleEnemies.Count == 0))
            throw new InvalidTargetException("No suitable target(s) provided.");
        
        var attack = new AttackDamage(Power, DamageType, 0, null);
        switch (caster)
        {
            case Player player:
                if (player.Level < RequiredLevel)
                    throw new InvalidSkillLevelException("You are too low a level to use this skill.");

                try
                {
                    player.Archetype.UseResource(Cost);
                }
                catch (InvalidOperationException)
                {
                    throw new SkillResourceException(
                        $"Not enough {player.Archetype.ResourceName} available to use this skill.");
                }

                player.AddActionItem(this);

                switch (TargetType)
                {
                    case TargetType.SingleEnemy:
                        singleEnemy?.TakeDamage(attack);
                        break;

                    case TargetType.AllEnemies:
                        foreach (var tar in multipleEnemies!)
                        {
                            try
                            {
                                tar.TakeDamage(attack);
                            }
                            catch (MonsterDeathException)
                            {
                                monsterDeath = true;
                            }
                        }

                        break;
                }

                Reset();

                if (monsterDeath)
                    throw new MonsterDeathException();

                break;

            case Monster monster:
                if (monster.Level < RequiredLevel)
                    throw new InvalidSkillLevelException("The monster's level is too low to use this skill.");

                if (singleEnemy == null)
                    throw new InvalidTargetException("Monster skill requires a single enemy target.");
    
                monster.AddActionItem(this);
                singleEnemy.TakeDamage(attack);

                Reset();
                break;

            default:
                throw new InvalidOperationException("Invalid caster type.");
        }
    }

    public override Skill Clone()
    {
        return new MagicSkill
        {
            Name = this.Name,
            Description = this.Description,
            RequiredLevel = this.RequiredLevel,
            Cost = this.Cost,
            Power = this.Power,
            Cooldown = this.Cooldown,
            TargetType = this.TargetType,
            SkillCategory = this.SkillCategory,
            DamageType = this.DamageType
        };
    }
}