using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Skills;

public class BossSkill : DamageSkill
{
    [NotMapped]
    public int Phase { get; set; }
    [NotMapped]
    public bool IsReady => ElapsedTime >= Cooldown;

    public override void InitializeSkill(int level)
    {
        Phase = 1;
        Reset();
    }
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (!IsReady) throw new SkillCooldownException("This skill is still on cooldown.");

        double multiplier = Phase switch
        {
            1 => 1.0,
            2 => 1.5,
            3 => 2.0,
            _ => 1.0 // fallback in case Phase is invalid
        };

        if (caster is not BossMonster)
            throw new InvalidOperationException("Only monsters can use boss skills.");
        else if (caster is BossMonster boss)
            boss.Logger.Log($"{boss.Name} uses {Name}!");

        int damage = (int)(Power * multiplier);
        singleEnemy?.TakeDamage(damage, DamageType);
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
