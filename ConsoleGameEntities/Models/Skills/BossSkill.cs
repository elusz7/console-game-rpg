using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Models.Skills;

public class BossSkill : Skill
{
    public int Phase { get; set; }

    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        if (IsOnCooldown) throw new SkillCooldownException("This skill is still on cooldown.");

        double multiplier = Phase switch
        {
            1 => 1.0,
            2 => 1.5,
            3 => 2.0,
            _ => 1.0 // fallback in case Phase is invalid
        };

        caster.AddActionItem(this);

        int damage = (int)(Power * multiplier);
        singleEnemy?.TakeDamage(damage, DamageType);
    }
}
