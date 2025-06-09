using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public abstract class DamageSkill : Skill
{
    public DamageType DamageType { get; set; }
}
