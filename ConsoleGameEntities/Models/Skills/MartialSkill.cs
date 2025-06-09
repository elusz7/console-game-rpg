using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class MartialSkill : DamageSkill
{
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        if (singleEnemy == null && (multipleEnemies == null || multipleEnemies.Count == 0))
            throw new InvalidTargetException("No suitable target(s) provided.");

        var monsterDeath = false;

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException("Your level is too low to use this skill.");

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException($"You don't have enough {player.Archetype.ResourceName}."); }

            player.Logger.Log($"You use {Name}!");

            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                    singleEnemy.TakeDamage(Power, DamageType);
                    break;

                case TargetType.AllEnemies:
                    foreach (var tar in multipleEnemies)
                    {
                        try
                        {
                            tar.TakeDamage(Power, DamageType);
                        }
                        catch (MonsterDeathException) { monsterDeath = true; }
                    }

                    break;
            }

            Reset();
            if (monsterDeath)
                throw new MonsterDeathException();
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException("This monster's level is too low to use this skill.");

            monster.Logger.Log($"{monster.Name} uses {Name}!");

            singleEnemy.TakeDamage(Power, DamageType);
            Reset();
        }
    }
    public override Skill Clone()
    {
        return new MartialSkill
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
