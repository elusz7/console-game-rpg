using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Runes;
using Microsoft.EntityFrameworkCore;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Exceptions;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;


public class RuneDao(GameContext context) : IRuneDao
{
    private readonly GameContext _context = context;

    public Rune? FindRune(ElementType element, RarityLevel rarity, string type)
    {
        return _context.Runes
                .SingleOrDefault(r => r.Element == element 
                                      && r.Rarity == rarity
                                      && r.RuneType == type);
    }

    public List<InventoryRune> GetRunesByPlayerId(int playerId)
    {
        return 
        [
            .. _context.InventoryRunes
                .Include(ir => ir.Inventory)
                .Include(ir => ir.Rune)
                .Include(ir => ir.EquippedRunes)
                .Where(ir => ir.Inventory.PlayerId == playerId)
        ];
    }
    
    public List<InventoryRune> GetFuseableRunesByPlayerId(int playerId)
    {
        return _context.InventoryRunes
            .Include(ir => ir.Rune)
            .Include(ir => ir.Inventory)
            .Include(ir => ir.EquippedRunes)
            .Where(ir => ir.Inventory.PlayerId == playerId)
            .Where(ir => ir.Quantity - ir.EquippedRunes.Count >= 3)
            .Where(ir => !(ir.Rune.Rarity == RarityLevel.Mythic && ir.Tier == 3))
            .ToList();
    }
    
    public List<InventoryRune> GetDestroyableRunesByPlayerId(int playerId)
    {
        return _context.InventoryRunes
            .Include(ir => ir.Rune)
            .Include(ir => ir.Inventory)
            .Include(ir => ir.EquippedRunes)
            .Where(ir => ir.Inventory.PlayerId == playerId)
            .Where(ir => ir.Quantity > ir.EquippedRunes.Count)
            .ToList();
    }

    public Rune? GetRuneDefinitionById(int runeId)
    {
        return _context.Runes.Find(runeId);
    }

    public void AddRuneToPlayer(int runeId, int playerId, int tier, int quantity = 1)
    {
        if (tier is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(tier));
        
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var inventory = _context.Inventories
                            .FirstOrDefault(i => i.PlayerId == playerId)
                        ?? throw new InventoryException(
                            $"No inventory found for player {playerId}.");

        var runeExists = _context.Runes.Any(r => r.Id == runeId);

        if (!runeExists)
            throw new InvalidOperationException($"Rune {runeId} does not exist.");

        var inventoryRune = _context.InventoryRunes.Find(
            inventory.Id, runeId, tier);

        if (inventoryRune != null)
        {
            //If a player already has 2, the quantity is 3, now they have 5
            //No overwriting
            inventoryRune.Quantity += quantity; 
        }
        else
        {
            _context.InventoryRunes.Add(new InventoryRune
            {
                InventoryId = inventory.Id,
                RuneId = runeId,
                Tier = tier,
                Quantity = quantity
            });
        }

        _context.SaveChanges();
    }

    public void UpdatePlayerRuneQuantity(int runeId, int playerId, int tier, int quantity = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        var inventoryRune = _context.InventoryRunes
                                .Include(ir => ir.EquippedRunes)
                                .FirstOrDefault(ir =>
                                    ir.RuneId == runeId &&
                                    ir.Tier == tier &&
                                    ir.Inventory.PlayerId == playerId)
                            ?? throw new InventoryException(
                                $"Player {playerId} does not own rune {runeId} at tier {tier}.");

        var equippedQuantity = inventoryRune.EquippedRunes.Count;

        if (quantity < equippedQuantity)
            throw new InventoryException(
                "Cannot reduce rune quantity below the number of equipped copies.");

        if (quantity == 0)
            _context.InventoryRunes.Remove(inventoryRune);
        else
            inventoryRune.Quantity = quantity;

        _context.SaveChanges();
    }

    public void RemoveRuneFromPlayer(int runeId, int playerId, int tier)
    {
        var inventoryRune = _context.InventoryRunes
                                .Include(ir => ir.EquippedRunes)
                                .FirstOrDefault(ir =>
                                    ir.RuneId == runeId &&
                                    ir.Tier == tier &&
                                    ir.Inventory.PlayerId == playerId)
                            ?? throw new InventoryException(
                                $"Player {playerId} does not own rune {runeId} at tier {tier}.");

        if (inventoryRune.EquippedRunes.Count > 0)
            throw new InventoryException(
                "Cannot remove a rune while copies are equipped.");

        _context.InventoryRunes.Remove(inventoryRune);
        _context.SaveChanges();
    }
}
