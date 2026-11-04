using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;

namespace ConsoleGame.GameDao;

public class IngredientDao(GameContext context) : IIngredientDao
{
    private readonly GameContext _context = context;
    
    public List<InventoryIngredient> GetIngredientsByPlayerId(int playerId)
    {
        return 
        [
            .. _context.InventoryIngredients
                .Include(ii => ii.Ingredient)
                .Where(ii => ii.Inventory.PlayerId == playerId)
        ];
    }

    public Ingredient? GetIngredientDefinitionById(int ingredientId)
    {
        return _context.Ingredients.Find(ingredientId);
    }

    public void AddIngredientToPlayer(int ingredientId, int playerId, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        
        var inventory = _context.Inventories!
                            .FirstOrDefault(i => i.PlayerId == playerId)
                        ?? throw new InventoryException(
                            $"No inventory found for player {playerId}.");
        
        var ingredientExists = _context.Ingredients.Any(i => i.Id == ingredientId);

        if (!ingredientExists)
            throw new InvalidOperationException($"Ingredient {ingredientId} does not exist.");
      
        var inventoryIngredient = _context.InventoryIngredients.Find(inventory.Id, ingredientId);

        if (inventoryIngredient != null)
        {
            inventoryIngredient.Quantity += quantity;
        }
        else
        {
            _context.InventoryIngredients.Add(new InventoryIngredient()
            {
                InventoryId = inventory.Id,
                IngredientId = ingredientId,
                Quantity = quantity
            });
        }

        _context.SaveChanges();
    }

    public void UpdatePlayerIngredientQuantity(int ingredientId, int playerId, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var inventoryIngredient = _context.InventoryIngredients
                                      .FirstOrDefault(ii =>
                                          ii.IngredientId == ingredientId &&
                                          ii.Inventory.PlayerId == playerId)
                                  ?? throw new InventoryException(
                                      $"Player {playerId} does not own ingredient {ingredientId}.");

        inventoryIngredient.Quantity = quantity;
        _context.SaveChanges();
    }

    public void RemoveIngredientFromPlayer(int ingredientId, int playerId)
    {
        var inventoryIngredient = _context.InventoryIngredients
                                .FirstOrDefault(ii =>
                                    ii.IngredientId == ingredientId &&
                                    ii.Inventory.PlayerId == playerId)
                            ?? throw new InventoryException(
                                $"Player {playerId} does not own ingredient {ingredientId}.");

        _context.InventoryIngredients.Remove(inventoryIngredient);

        _context.SaveChanges();
    }

    public List<Ingredient> GetAllIngredients()
    {
        return [.. _context.Ingredients];
    }
}
