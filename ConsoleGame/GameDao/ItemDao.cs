using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class ItemDao(GameContext context) : IItemDao
{
    private readonly GameContext _context = context;
    public string SortOrder { get; set; } = "ASC"; // Default sort order

    public void AddItem(Item item)
    {
        _context.Items?.Add(item);
        _context.SaveChanges();
    }

    public void UpdateItem(Item item)
    {
        _context.Items?.Update(item);
        _context.SaveChanges();
    }

    public void DeleteItem(Item item)
    {
        _context.Items?.Remove(item);
        _context.SaveChanges();
    }

    public List<Item> GetItemsByName(string name)
    {
        var matchingItems = _context.Items?.Include(i => i.Inventory).ThenInclude(i => i!.Player).ToList()
            .Where(i => i.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? [];

        return SortOrder switch
        {
            "ASC" => [.. matchingItems.OrderBy(i => i.Name)],
            "DESC" => [.. matchingItems.OrderByDescending(i => i.Name)],
            _ => matchingItems
        };
    }

    public List<Item> GetAllItems()
    {
        var items = _context.Items?.Include(i => i.Inventory).ThenInclude(i => i!.Player).ToList() ?? [];

        return SortOrder switch
        {
            "ASC" => [.. items.OrderBy(i => i.Name)],
            "DESC" => [.. items.OrderByDescending(i => i.Name)],
            _ => items
        };
    }

    public List<Item> GetAllNonCoreItems()
    {
        if (_context.Items == null) throw new InvalidOperationException("Items collection is null.");
        return [.. _context.Items.Include(i => i.Inventory).ThenInclude(i => i!.Player).Where(i => i.Id > 178)];
    }

    public List<Item> GetItemsByType(ItemType type)
    {
        if (_context.Items == null)
            throw new InvalidOperationException("Items collection is null.");

        IQueryable<Item> query = type switch
        {
            ItemType.Weapon => _context.Items.OfType<Weapon>(),
            ItemType.Armor => _context.Items.OfType<Armor>(),
            ItemType.Valuable => _context.Items.OfType<Valuable>(),
            ItemType.Consumable => _context.Items.OfType<Consumable>(),
            _ => Enumerable.Empty<Item>().AsQueryable()
        };

        query = type switch
        {
            ItemType.Weapon => SortOrder == "ASC"
                ? query.Cast<Weapon>().OrderBy(w => w.AttackPower)
                : query.Cast<Weapon>().OrderByDescending(w => w.AttackPower),
            ItemType.Armor => SortOrder == "ASC"
                ? query.Cast<Armor>().OrderBy(a => a.DefensePower)
                : query.Cast<Armor>().OrderByDescending(a => a.DefensePower),
            ItemType.Valuable => SortOrder == "ASC"
                ? query.Cast<Valuable>().OrderBy(v => v.Value)
                : query.Cast<Valuable>().OrderByDescending(v => v.Value),
            ItemType.Consumable => SortOrder == "ASC"
                ? query.Cast<Consumable>().OrderBy(c => c.Power)
                : query.Cast<Consumable>().OrderByDescending(c => c.Power),
            _ => query
        };

        return [.. query
            .Include(i => i.Inventory)
            .ThenInclude(inv => inv!.Player)];
    }

    public List<Item> GetItemsByMaxLevel(int level)
    {
        return _context.Items?
            .Where(i => i.RequiredLevel <= level)
            .ToList() ?? [];
    }
}
