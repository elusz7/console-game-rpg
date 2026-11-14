using ConsoleGameEntities.Models.Combat;

namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IMonsterStrategy
{
    AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target);
}