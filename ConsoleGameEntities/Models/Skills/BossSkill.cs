using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Skills;

public class BossSkill : Skill
{
    public int Phase { get; set; }

    public override void Activate(ITargetable? caster, ITargetable? target = null, List<ITargetable>? targets = null)
    {
        if (IsOnCooldown) throw new SkillCooldownException();

        double multiplier = Phase switch
        {
            1 => 1.0,
            2 => 1.5,
            3 => 2.0,
            _ => 1.0 // fallback in case Phase is invalid
        };

        int damage = (int)(Power * multiplier);
        target?.TakeDamage(damage, DamageType);
    }
}
