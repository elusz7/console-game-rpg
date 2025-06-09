using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class SupportSkill : Skill
{
    public int Duration { get; set; } // Duration MUST be less than cooldown
    public StatType StatAffected { get; set; }
    public int SupportEffect { get; set; }

    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        if (caster is Player self)
        {
            if (self.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            try
            {
                self.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException)
            {
                throw new SkillResourceException();
            }

            self.Logger.Log($"You use {Name}!");

            if (TargetType == TargetType.Self)
            {
                ApplyEffect(self);
            }
            else if (TargetType == TargetType.SingleEnemy)
            {
                if (singleEnemy == null)
                    throw new InvalidTargetException("Support");

                ApplyEffect(singleEnemy);
            }
            else if (TargetType == TargetType.AllEnemies)
            {
                if (multipleEnemies == null || multipleEnemies.Count == 0)
                    throw new InvalidTargetException("Support");

                foreach (var tar in multipleEnemies)
                {
                    try
                    {
                        ApplyEffect(tar);
                    }
                    catch (InvalidTargetException) { /*do nothing, the stat has been effected*/ }
                }
            }

            Reset();
        }
        else if (caster is Monster monster)
        {
            if (TargetType == TargetType.Self)
            {
                ApplyEffect(monster);
            }
            else if (TargetType == TargetType.SingleEnemy)
            {
                if (singleEnemy == null)
                    throw new InvalidTargetException("Support");

                ApplyEffect(singleEnemy);
            }

            monster.Logger.Log($"{monster.Name} uses {Name}!");
            Reset();
        }
    }

    public override void UpdateElapsedTime()
    {
        ElapsedTime++;
    }

    public override void Reset()
    {
        ElapsedTime = 0;
    }

    public void ApplyEffect(ITargetable target)
    {
        int modifier = SupportEffect == (int)SupportEffectType.Boost ? Power : -Power;
        target.ModifyStat(StatAffected, Duration, modifier);
    }

    public override Skill Clone()
    {
        return new SupportSkill
        {
            Name = this.Name,
            Description = this.Description,
            RequiredLevel = this.RequiredLevel,
            Cost = this.Cost,
            Power = this.Power,
            Cooldown = this.Cooldown,
            TargetType = this.TargetType,
            SkillCategory = this.SkillCategory,
            Duration = this.Duration,
            StatAffected = this.StatAffected,
            SupportEffect = this.SupportEffect
        };
    }
}
