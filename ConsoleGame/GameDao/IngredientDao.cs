using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

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
    
    public List<InventoryIngredient> GetTransmutableEssencesByPlayerId(int playerId)
    {
        return [
            .. _context.InventoryIngredients
                .Include(ii => ii.Ingredient)
                .Where(ii => ii.Inventory.PlayerId == playerId)
                .Where(ii => ii.Quantity >= 5)
                .Where(ii => ii.Ingredient.ComponentType == ComponentType.Essence)
                .Where(ii => ii.Ingredient.EssenceRarity != null)
                .Where(ii => ii.Ingredient.EssenceRarity != RarityLevel.Mythic)
                .Where(ii => _context.Ingredients.Any(next =>
                    next.ComponentType == ComponentType.Essence &&
                    (int?)next.EssenceRarity ==
                    (int?)ii.Ingredient.EssenceRarity + 1))
        ];
    }

    public Ingredient? GetIngredientDefinitionById(int ingredientId)
    {
        return _context.Ingredients.Find(ingredientId);
    }
    
    public Ingredient? GetEssenceByRarity(RarityLevel rarity)
    {
        return _context.Ingredients.SingleOrDefault(i =>
            i.ComponentType == ComponentType.Essence &&
            i.EssenceRarity == rarity);
    }

    public void AddIngredientToPlayer(int ingredientId, int playerId, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        
        var inventory = _context.Inventories
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
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        var inventoryIngredient = _context.InventoryIngredients
                                      .FirstOrDefault(ii =>
                                          ii.IngredientId == ingredientId &&
                                          ii.Inventory.PlayerId == playerId)
                                  ?? throw new InventoryException(
                                      $"Player {playerId} does not own ingredient {ingredientId}.");

        if (quantity == 0)
            _context.InventoryIngredients.Remove(inventoryIngredient);
        else
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
