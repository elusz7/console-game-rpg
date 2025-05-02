namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IMonsterStrategy
{
    void ExecuteAttack(IMonster monster, ITargetable target);
}
