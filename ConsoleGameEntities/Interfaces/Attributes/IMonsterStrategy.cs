namespace ConsoleGameEntities.Interfaces.Attributes;

public interface IMonsterStrategy
{
    void ExecuteAttack(IMonster monster, IPlayer target);
}
