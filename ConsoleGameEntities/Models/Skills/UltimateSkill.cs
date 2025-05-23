using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace ConsoleGameEntities.Models.Skills;

public class UltimateSkill : DamageSkill
{
    // "Cooldown" here means the charge-up time before the ultimate can be used again.
    // ElapsedTime must meet or exceed Cooldown.
    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;
    private int previousScaledLevel;

    public override void InitializeSkill(int level)
    {
        previousScaledLevel = RequiredLevel;
        Reset();
        ScalePowerWithLevel(level);
    }
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (!IsReady) throw new SkillCooldownException();

        if (singleEnemy == null && (multipleEnemies == null || multipleEnemies.Count == 0))
            throw new InvalidTargetException("No suitable target(s) provided.");

        bool monsterDeath = false;

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            ScalePowerWithLevel(player.Level);

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException("Ultimate"); }

            player.AddActionItem(this);            

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
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            ScalePowerWithLevel(monster.Level);

            monster.AddActionItem(this);

            singleEnemy.TakeDamage(Power, DamageType);
        }

        Reset();

        if (monsterDeath)
        {
            throw new MonsterDeathException();
        }
    }
    public void ScalePowerWithLevel(int level)
    {
        if (level != previousScaledLevel)
        {
            var levelScale = Math.Max(1, level - 3);
            Power = Power * (int)(1 + (Math.Pow((levelScale), 1.2) / 5));

            Cooldown += level % 2;

            previousScaledLevel = level;
        }
    }
    public override Skill Clone()
    {
        return new UltimateSkill
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
