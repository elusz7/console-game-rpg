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
        if (item.IsCore)
            throw new InvalidOperationException("Core game items cannot be deleted.");

        _context.Items?.Remove(item);
        _context.SaveChanges();
    }

    public List<Item> GetItemsByName(string name)
    {
        var matchingItems = _context.Items!
            .Where(i => EF.Functions.Like(i.Name, $"%{name}%"))
            .Include(i => i.Inventory)
            .ThenInclude(i => i!.Player)
            .ToList();

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
        var query = _context.Items
            .Where(i => !i.IsCore);

        query = SortOrder switch
        {
            "ASC" => query.OrderBy(i => i.Name),
            "DESC" => query.OrderByDescending(i => i.Name),
            _ => query
        };

        return
        [
            .. query
                .Include(i => i.Inventory)
                .ThenInclude(i => i!.Player)
        ];
    }

    public List<Item> GetItemsByType(ItemType type)
    {
        if (_context.Items == null)
            throw new InvalidOperationException("Items collection is null.");

        var ascending = SortOrder == "ASC";

        var query = type switch
        {
            ItemType.Weapon => ascending
                ? _context.Items.OfType<Weapon>().OrderBy(w => w.AttackPower)
                : _context.Items.OfType<Weapon>().OrderByDescending(w => w.AttackPower),

            ItemType.Armor => ascending
                ? _context.Items.OfType<Armor>().OrderBy(a => a.DefensePower)
                : _context.Items.OfType<Armor>().OrderByDescending(a => a.DefensePower),

            ItemType.Valuable => ascending
                ? _context.Items.OfType<Valuable>().OrderBy(v => v.Value)
                : _context.Items.OfType<Valuable>().OrderByDescending(v => v.Value),

            ItemType.Consumable => ascending
                ? _context.Items.OfType<Consumable>().OrderBy(c => c.Power)
                : _context.Items.OfType<Consumable>().OrderByDescending(c => c.Power),

            _ => Enumerable.Empty<Item>().AsQueryable()
        };

        return
        [
            .. query
                .Include(i => i.Inventory)
                .ThenInclude(inv => inv!.Player)
        ];
    }

    public List<Item> GetItemsAvailableAtLevel(int level)
    {
        return _context.Items?
            .Where(i => i.RequiredLevel <= level)
            .ToList() ?? [];
    }
}