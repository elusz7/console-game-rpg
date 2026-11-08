using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRuneDao
{
    Rune? FindRune(ElementType element, RarityLevel rarity, string type);
    List<InventoryRune> GetRunesByPlayerId(int playerId);
    List<InventoryRune> GetFuseableRunesByPlayerId(int playerId);
    List<InventoryRune> GetDestroyableRunesByPlayerId(int playerId);
    Rune? GetRuneDefinitionById(int runeId);
    void AddRuneToPlayer(int runeId, int playerId, int tier, int quantity = 1);
    void UpdatePlayerRuneQuantity(int runeId, int playerId, int tier, int quantity = 1);
    void RemoveRuneFromPlayer(int runeId, int playerId, int tier);
}