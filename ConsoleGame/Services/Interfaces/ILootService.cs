using ConsoleGame.Models;
using ConsoleGameEntities.Interfaces;

namespace ConsoleGame.Services.Interfaces;

public interface ILootService
{
    void LootMonster(GameSession session, IMonster monster);
}