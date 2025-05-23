using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.Extensions.Logging;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class SupportSkill : Skill
{
    public int Duration { get; set; } // Duration MUST be less than cooldown
    public StatType StatAffected { get; set; }
    public int SupportEffect { get; set; }

    // Track how many turns each target has left under the effect
    [NotMapped]
    public Dictionary<ITargetable, int> TargetsAffected { get; set; } = new();

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

            self.AddActionItem(this);

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

            ElapsedTime = 0;
        }
        else if (caster is Monster monster)
        {
            if (TargetType == TargetType.Self)
            {
                monster.AddActionItem(this);
                ApplyEffect(monster);
            }
            else if (TargetType == TargetType.SingleEnemy)
            {
                if (singleEnemy == null)
                    throw new InvalidTargetException("Support");

                monster.AddActionItem(this);
                ApplyEffect(singleEnemy);
            }

            ElapsedTime = 0;
        }
    }

    public override void UpdateElapsedTime()
    {
        var expiredTargets = new List<ITargetable>();

        foreach (var kvp in TargetsAffected)
        {
            var target = kvp.Key;
            int turnsRemaining = kvp.Value - 1;

            if (turnsRemaining <= 0)
            {
                RevertEffect(target);
                expiredTargets.Add(target);
            }
            else
            {
                TargetsAffected[target] = turnsRemaining;
            }
        }

        foreach (var target in expiredTargets)
            TargetsAffected.Remove(target);

        ElapsedTime++;
    }

    public override void Reset()
    {
        foreach (var target in TargetsAffected.Keys)
            RevertEffect(target);

        TargetsAffected.Clear();
        ElapsedTime = 0;
    }

    public void ApplyEffect(ITargetable target)
    {
        if (TargetsAffected.ContainsKey(target))
            throw new InvalidTargetException("Cannot apply multiple effects to same target with same skill");

        int modifier = SupportEffect == (int)SupportEffectType.Boost ? Power : -Power;
        target.ModifyStat(StatAffected, modifier);

        if (StatAffected != StatType.Health)
            TargetsAffected[target] = Duration;
    }

    private void RevertEffect(ITargetable target)
    {
        target.AddActionItem($"{Name} effect expired");
        int modifier = SupportEffect == (int)SupportEffectType.Boost ? -Power : Power;
        target.ModifyStat(StatAffected, modifier);
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
