using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;

namespace ConsoleGame.GameDao.Interfaces;

public interface IIngredientDao
{
    List<InventoryIngredient> GetIngredientsByPlayerId(int playerId);
    Ingredient? GetIngredientDefinitionById(int ingredientId);
    void AddIngredientToPlayer(int ingredientId, int playerId, int quantity = 1);
    void UpdatePlayerIngredientQuantity(int ingredientId, int playerId, int quantity);
    void RemoveIngredientFromPlayer(int ingredientId, int playerId);
    List<Ingredient> GetAllIngredients();
}