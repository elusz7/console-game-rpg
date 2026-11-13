using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Exceptions.Skills;
using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Models.Skills;

public class MagicSkill : DamageSkill
{
    public override void Activate(ITargetable caster, ITargetable? singleEnemy = null, List<ITargetable>? multipleEnemies = null)
    {
        ExecuteDamageSkill(caster, singleEnemy, multipleEnemies);
    }

    public override MagicSkill Clone()
    {
        var clone = new MagicSkill { DamageType = DamageType };
        CopyTo(clone);
        return clone;
    }
}