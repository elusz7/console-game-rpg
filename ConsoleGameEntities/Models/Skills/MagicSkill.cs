using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MagicSkill : DamageSkill
{
    public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        return ExecuteDamageSkill(caster, singleEnemy, multipleEnemies);
    }

    public override MagicSkill Clone()
    {
        var clone = new MagicSkill { DamageType = DamageType };
        CopyTo(clone);
        return clone;
    }
}