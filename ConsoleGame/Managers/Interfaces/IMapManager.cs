using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Managers.Interfaces;

public interface IMapManager
{
    void DisplayMap();
    void UpdateCurrentRoom(Room currentRoom);
    void TraverseMap(Room currentRoom);
}
