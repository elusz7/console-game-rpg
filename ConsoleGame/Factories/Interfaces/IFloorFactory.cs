using ConsoleGame.Models;

namespace ConsoleGame.Factories.Interfaces;

public interface IFloorFactory
{
    Floor CreateFloor(int level, bool campaign, bool randomMap);
}
