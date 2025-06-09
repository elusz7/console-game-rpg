using ConsoleGameEntities.Helpers.Gameplay;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
namespace ConsoleGameEntities.Interfaces.Attributes;

public interface ITargetable
{
    ActionLogger Logger { get; }
    void TakeDamage(int damage, DamageType? damageType);
    void TakeDamage(int damage, ElementType element);
    void Heal(int regainedHealth);
    void ModifyStat(StatType stat, int duration, int amount);

}
