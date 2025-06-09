using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Factories.Interfaces;

public interface IMapFactory
{
    List<Room> GenerateMap(int level, bool campaign, bool randomMap);
}
