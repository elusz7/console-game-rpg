using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRuneDao
{
    List<InventoryRune> GetRunesByPlayerId(int playerId);
    Rune? GetRuneDefinitionById(int runeId);
    void AddRuneToPlayer(int runeId, int playerId, int tier, int quantity = 1);
    void UpdatePlayerRuneQuantity(int runeId, int playerId, int tier, int quantity = 1);
    void RemoveRuneFromPlayer(int runeId, int playerId, int tier);
}