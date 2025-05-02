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
    public SupportEffectType Effect { get; set; }

    // Track how many turns each target has left under the effect
    [NotMapped]
    public Dictionary<ITargetable, int> TargetsAffected { get; set; } = new();

    public override void Activate(ITargetable? caster, ITargetable? target = null, List<ITargetable>? targets = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        if (caster is Player self)
        {
            try
            {
                self.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException)
            {
                throw new SkillResourceException();
            }

            if (TargetType == TargetType.Self)
            {
                ApplyEffect(self);
            }
            else if (TargetType == TargetType.SingleEnemy || TargetType == TargetType.RandomEnemy)
            {
                if (target == null)
                    throw new InvalidTargetException("Support");

                ApplyEffect(target);
            }
            else if (TargetType == TargetType.AllEnemies)
            {
                if (targets == null || targets.Count == 0)
                    throw new InvalidTargetException("Support");

                foreach (var tar in targets)
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
                ApplyEffect(monster);
            }
            else if (TargetType == TargetType.SingleEnemy)
            {
                if (target == null)
                    throw new InvalidTargetException("Support");

                ApplyEffect(target);
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
    }

    public override void Reset()
    {
        foreach (var target in TargetsAffected.Keys)
            RevertEffect(target);

        TargetsAffected.Clear();
        ElapsedTime = 0;
    }

    private void ApplyEffect(ITargetable target)
    {
        if (TargetsAffected.ContainsKey(target))
            throw new InvalidTargetException("Cannot apply multiple effects to same target with same skill");

        int modifier = Effect == SupportEffectType.Boost ? Power : -Power;
        target.ModifyStat(StatAffected, modifier);
        TargetsAffected[target] = Duration;
    }

    private void RevertEffect(ITargetable target)
    {
        int modifier = Effect == SupportEffectType.Boost ? -Power : Power;
        target.ModifyStat(StatAffected, modifier);
    }
}
