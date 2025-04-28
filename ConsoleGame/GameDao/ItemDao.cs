using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class ItemDao
{
    private readonly GameContext _context;
    public string SortOrder { get; set; }

    public ItemDao(GameContext context)
    {
        _context = context;
        SortOrder = "ASC"; // Default sort order
    }
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

    public Item GetItemByName(string name)
    {        
        return _context.Items
            .Include(i => i.Inventory)
            .ThenInclude(i => i.Player)
            .Where(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    }

    public List<Item> GetAllItems(string name)
    {
        var matchingItems = _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).ToList()
            .Where(i => i.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return SortOrder switch
        {
            "ASC" => matchingItems.OrderBy(i => i.Name).ToList(),
            "DESC" => matchingItems.OrderByDescending(i => i.Name).ToList()
        };
    }

    public List<Item> GetAllItems()
    {
        var items = _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).ToList();
        
        return SortOrder switch
        {
            "ASC" => items.OrderBy(i => i.Name).ToList(),
            "DESC" => items.OrderByDescending(i => i.Name).ToList()
        };
    }

    public List<Item> GetItemsByType(string type)
    {
        type = type.ToLower();
        List<Item> items = type switch
        {
            "weapon" => _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Weapon>().ToList<Item>(),
            "armor" => _context.Items.Include(i => i.Inventory).ThenInclude(i => i.Player).OfType<Armor>().ToList<Item>()
        };

        items = (type, SortOrder) switch
        {
            ("weapon", "ASC") => items.OrderBy(i => ((Weapon)i).AttackPower).ToList(),
            ("weapon", "DESC") => items.OrderByDescending(i => ((Weapon)i).AttackPower).ToList(),
            ("armor", "ASC") => items.OrderBy(i => ((Armor)i).DefensePower).ToList(),
            ("armor", "DESC") => items.OrderByDescending(i => ((Armor)i).DefensePower).ToList()
        };

        return items;
    }
}
