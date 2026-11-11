using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MartialSkill : DamageSkill
{
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null,
        List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        var attack = new AttackDamage(Power, DamageType, 0, null);
        switch (TargetType)
        {
            case TargetType.SingleEnemy:
                if (singleEnemy == null)
                    throw new InvalidTargetException("Single enemy target not provided.");
                break;

            case TargetType.AllEnemies:
                if (multipleEnemies == null || multipleEnemies.Count == 0)
                    throw new InvalidTargetException("Multiple enemies not provided.");
                break;
        }

        var monsterDeath = false;

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException("Your level is too low to use this skill.");

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException)
            {
                throw new SkillResourceException($"You don't have enough {player.Archetype.ResourceName}.");
            }

            player.AddActionItem(this);

            switch (TargetType)
            {
                case TargetType.SingleEnemy:
                    (singleEnemy ?? throw new InvalidTargetException("Single enemy target not provided."))
                        .TakeDamage(attack);
                    break;

                case TargetType.AllEnemies:
                    foreach (var tar in multipleEnemies
                                        ?? throw new InvalidTargetException("Multiple enemies not provided."))
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
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException("This monster's level is too low to use this skill.");

            monster.AddActionItem(this);

            (singleEnemy ?? throw new InvalidTargetException("Single enemy target not provided."))
                .TakeDamage(attack);
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