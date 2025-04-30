using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
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
    public int Cost { get; set; }
    public int Power { get; set; }
    public int ElapsedTime { get; set; } // time since skill was last used
    public int Cooldown { get; set; }
    [NotMapped]
    public bool IsOnCooldown => ElapsedTime < Cooldown;
    public TargetType TargetType { get; set; }
    public string SkillType { get; set; }
    public SkillTypeEnum ESkillType { get; set; }
    public int? ArchetypeId { get; set; }
    public virtual Archetype Archetype { get; set; }
    public int? MonsterId { get; set; }
    public virtual Monster Monster { get; set; }

    public virtual bool Activate(ITargetable? caster, List<ITargetable>? targets = null)
    {
        if (IsOnCooldown)
            return false;

        if (caster is Player player)
        {
            try
            {
                player.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { return false; }

            if (TargetType == TargetType.AllEnemies && ESkillType != SkillTypeEnum.Support)
            {
                if (targets == null) return false;

                foreach (var target in targets)
                    target.TakeDamage(Power, ESkillType);

                ElapsedTime = 0;
                return true;
            }

            if (ESkillType == SkillTypeEnum.Support && (TargetType == TargetType.Self || targets == null))
            {
                player.Heal(Power); // or apply buff if needed
                ElapsedTime = 0;
                return true;
            }

            if (targets != null && targets[0] is Monster monsterTarget)
            {
                monsterTarget.TakeDamage(Power, ESkillType);
                ElapsedTime = 0;
                return true;
            }

            return false;
        }
        else if (caster is Monster monster)
        {
            if (ESkillType == SkillTypeEnum.Support && (targets == null || targets.Count == 0))
            {
                monster.Heal(Power);
            }
            else if (targets != null && targets[0] is Player playerTarget)
            {
                playerTarget.TakeDamage(Power, ESkillType);
            }

            ElapsedTime = 0;
            return true;
        }

        return false;
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
