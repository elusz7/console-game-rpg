using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGame.Factories.Interfaces;

public interface IMonsterFactory
{
    List<Monster> GenerateMonsters(int level, bool campaign);
}
