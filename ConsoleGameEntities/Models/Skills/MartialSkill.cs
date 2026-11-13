using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MartialSkill : DamageSkill
{
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        ExecuteDamageSkill(caster, singleEnemy, multipleEnemies);
    }

    public override MartialSkill Clone()
    {
        var clone = new MartialSkill { DamageType = DamageType };
        CopyTo(clone);
        return clone;
    }
}