using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MartialSkill : DamageSkill
{
    public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        return ExecuteDamageSkill(caster, singleEnemy, multipleEnemies);
    }

    public override MartialSkill Clone()
    {
        var clone = new MartialSkill { DamageType = DamageType };
        CopyTo(clone);
        return clone;
    }
}