using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao.Interfaces;

public interface IIngredientDao
{
    List<InventoryIngredient> GetIngredientsByPlayerId(int playerId);
    Ingredient? GetIngredientDefinitionById(int ingredientId);
    List<InventoryIngredient> GetTransmutableEssencesByPlayerId(int playerId);
    Ingredient? GetEssenceByRarity(RarityLevel rarity);
    void AddIngredientToPlayer(int ingredientId, int playerId, int quantity = 1);
    void UpdatePlayerIngredientQuantity(int ingredientId, int playerId, int quantity);
    void RemoveIngredientFromPlayer(int ingredientId, int playerId);
    List<Ingredient> GetAllIngredients();
}