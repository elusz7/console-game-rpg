using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    Dictionary<long, string> ActionItems { get; }
    ActiveEffectManager ActiveEffectManager { get; }
    void TakeDamage(AttackDamage attack);
    void Heal(int regainedHealth);
    int GetStat(StatType stat);
    void AddActionItem(Skill skill);
    void AddActionItem(string action);
    void ClearActionItems();
}