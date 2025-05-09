using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    Dictionary<int, string> ActionItems { get; }
    void TakeDamage(int damage, DamageType? damageType);
    void Heal(int regainedHealth);
    int GetStat(StatType stat);
    void ModifyStat(StatType stat, int amount);
    void AddActionItem(ISkill skill);
    void AddActionItem(string actionItem);
    void ClearActionItems();
}
