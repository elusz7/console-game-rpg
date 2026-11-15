using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    ActiveEffectManager ActiveEffectManager { get; }
    
    AttackResolutionResult TakeDamage(AttackDamage attack);
    DoTEffectResult ApplyElementalDamage(int damage, ElementType element);
    
    void Heal(int regainedHealth);
    
    int GetStat(StatType stat);
    
    Dictionary<long, string> ActionItems { get; }
    void AddActionItem(Skill skill);
    void AddActionItem(string action);
    void ClearActionItems();
}