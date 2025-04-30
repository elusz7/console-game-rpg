using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    string Name { get; set; }

    void TakeDamage(int damage, SkillTypeEnum? skillType = null);
    void Heal(int regainedHealth);

    int GetStat(StatType stat);
    void BoostStat(StatType stat, int power, int? original = null);
    void ReduceStat(StatType stat, int power, int? original = null);
}
