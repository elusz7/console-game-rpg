using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class Skill : ISkill
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int RequiredLevel { get; set; }
    public int Cost { get; set; }
    public int Power { get; set; }
    [NotMapped]
    public int ElapsedTime { get; set; } // time since skill was last used
    public int Cooldown { get; set; }
    [NotMapped]
    public bool IsOnCooldown => ElapsedTime < Cooldown;
    public TargetType TargetType { get; set; }
    public string SkillType { get; set; }
    public SkillCategory SkillCategory { get; set; }
    public DamageType DamageType { get; set; }
    public int? ArchetypeId { get; set; }
    public virtual Archetype? Archetype { get; set; }
    public int? MonsterId { get; set; }
    public virtual Monster? Monster { get; set; }

    public virtual void Activate(ITargetable? caster, ITargetable? target = null, List<ITargetable>? targets = null)
    {
        if (IsOnCooldown)
            throw new SkillCooldownException();

        if (caster is Player player)
        {
            if (player.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { throw new SkillResourceException(); }

            switch (TargetType)
            {
                case TargetType.AllEnemies when SkillCategory == SkillCategory.Basic:
                    if (targets == null || targets.Count == 0)
                        throw new InvalidTargetException("Skill");

                    foreach (var t in targets)
                        t.TakeDamage(Power, DamageType);
                    break;
                case TargetType.AllEnemies when SkillCategory == SkillCategory.Support:
                        if (targets == null || targets.Count == 0)
                            throw new InvalidTargetException("Skill");

                        foreach (var t in targets)
                            t.ModifyStat(((SupportSkill)this).StatAffected, Power);
                    break;
                case TargetType.Self when SkillCategory == SkillCategory.Support:
                    player.ModifyStat(((SupportSkill)this).StatAffected, Power);
                    break;

                case TargetType.SingleEnemy:
                    if (target == null)
                        throw new InvalidTargetException("Skill");

                    if (this is SupportSkill supportSkill)
                    {
                        target.ModifyStat(supportSkill.StatAffected, Power);
                    }
                    else
                    {
                        target.TakeDamage(Power, DamageType);
                    }
                    break;
            }

            ElapsedTime = 0;
        }
        else if (caster is Monster monster)
        {
            if (monster.Level < RequiredLevel)
                throw new InvalidSkillLevelException();

            switch (TargetType)
            {
                case TargetType.Self when SkillCategory == SkillCategory.Support:
                    monster.ModifyStat(((SupportSkill)this).StatAffected, Power);
                    break;

                case TargetType.SingleEnemy:
                    if (target == null)
                        throw new InvalidTargetException("Skill");

                    target.TakeDamage(Power, DamageType);
                    break;

                case TargetType.AllEnemies:
                    if (targets == null || targets.Count == 0)
                        throw new InvalidTargetException("Skill");

                    foreach (var t in targets)
                        t.TakeDamage(Power, DamageType);
                    break;
            }

            ElapsedTime = 0;
        }
    }
    public virtual void UpdateElapsedTime()
    {
        ElapsedTime++;
    }
    public virtual void Reset()
    {
        ElapsedTime = 0;
    }
}
