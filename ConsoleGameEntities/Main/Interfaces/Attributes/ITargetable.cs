using ConsoleGameEntities.Main.Models.Skills;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Main.Interfaces.Attributes;

public interface ITargetable
{
    Dictionary<long, string> ActionItems { get; }
    void TakeDamage(int damage, DamageType? damageType);
    void Heal(int regainedHealth);
    int GetStat(StatType stat);
    void ModifyStat(StatType stat, int amount);
    void AddActionItem(Skill skill);
    void AddActionItem(string action);
    void ClearActionItems();
}
