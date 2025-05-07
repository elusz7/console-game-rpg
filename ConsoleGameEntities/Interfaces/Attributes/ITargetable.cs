using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    void TakeDamage(int damage, DamageType? damageType);
    void Heal(int regainedHealth);
    int GetStat(StatType stat);
    void ModifyStat(StatType stat, int amount);
}
