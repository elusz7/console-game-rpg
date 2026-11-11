using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Interfaces.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class BossSkill : DamageSkill
{
    [NotMapped] public int Phase { get; set; }
    [NotMapped] public bool IsReady => ElapsedTime >= Cooldown;

    public override void InitializeSkill(int level)
    {
        Phase = 1;
        Reset();
    }

    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null,
        List<ITargetable>? multipleEnemies = null)
    {
        if (!IsReady) throw new SkillCooldownException("This skill is still on cooldown.");

        // Start the cooldown when the skill is activated.
        Reset();
        
        var multiplier = Phase switch
        {
            1 => 1.0,
            2 => 1.5,
            3 => 2.0,
            _ => 1.0 // fallback in case Phase is invalid
        };

        caster.AddActionItem(this);

        var damage = (int)(Power * multiplier);
        var attack = new AttackDamage(damage, DamageType, 0, null);
        singleEnemy?.TakeDamage(attack);
    }

    public override Skill Clone()
    {
        return new BossSkill
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