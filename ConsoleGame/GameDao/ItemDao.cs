using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class ItemDao(GameContext context)
{
    private readonly GameContext _context = context;
    public string SortOrder { get; set; } = "ASC"; // Default sort order

    public void AddItem(Item item)
    {
        _context.Items.Add(item);
        _context.SaveChanges();
    }

    public void UpdateItem(Item item)
    {        
        _context.Items.Update(item);
        _context.SaveChanges();
    }

    public void DeleteItem(Item item)
    {        
        _context.Items.Remove(item);
        _context.SaveChanges();
    }

    public List<Item> GetAllItems(string name)
    {
        var matchingItems = _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).ToList()
            .Where(i => i.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return SortOrder switch
        {
            "ASC" => [.. matchingItems.OrderBy(i => i.Name)],
            "DESC" => [.. matchingItems.OrderByDescending(i => i.Name)]
        };
    }

    public List<Item> GetAllItems()
    {
        var items = _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).ToList();
        
        return SortOrder switch
        {
            "ASC" => [.. items.OrderBy(i => i.Name)],
            "DESC" => [.. items.OrderByDescending(i => i.Name)]
        };
    }

    public List<Item> GetItemsByType(string type)
    {
        List<Item> items = type switch
        {
            "weapon" => [.. _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Weapon>()],
            "armor" => [.. _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Armor>()],
            "valuable" => [.. _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Valuable>()],
            "consumable" => [.. _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Consumable>()]
        };

        items = (type, SortOrder) switch
        {
            ("weapon", "ASC") => [.. items.OrderBy(i => ((Weapon)i).AttackPower)],
            ("weapon", "DESC") => [.. items.OrderByDescending(i => ((Weapon)i).AttackPower)],
            ("armor", "ASC") => [.. items.OrderBy(i => ((Armor)i).DefensePower)],
            ("armor", "DESC") => [.. items.OrderByDescending(i => ((Armor)i).DefensePower)],
            ("valuable", "ASC") => [.. items.OrderBy(i => ((Valuable)i).Value)],
            ("valuable", "DESC") => [.. items.OrderByDescending(i => ((Valuable)i).Value)],
            ("consumable", "ASC") => [.. items.OrderBy(i => ((Consumable)i).Power)],
            ("consumable", "DESC") => [.. items.OrderByDescending(i => ((Consumable)i).Power)],
        };

        return items;
    }

    public List<Item> GetItemsByMaxLevel(int level)
    {
        if (_context.Items == null) throw new InvalidOperationException("Items collection is null.");
        return [.._context.Items.Where(i => i.RequiredLevel <= level)];
    }
}
