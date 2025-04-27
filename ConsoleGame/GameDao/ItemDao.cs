using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class ItemDao
{
    private readonly GameContext _context;
    public string SortOrder;

    public ItemDao(GameContext context)
    {
        _context = context;
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
        return _context.Items.Where(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    }

    public List<Item> GetAllItems(string name)
    {
        List<Item> matchingItems = _context.Items
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
        return SortOrder switch
        {
            "ASC" => _context.Items.OrderBy(i => i.Name).ToList(),
            "DESC" => _context.Items.OrderByDescending(i => i.Name).ToList()
        };
    }

    public List<Item> GetItemsByType(string type)
    {
        List<Item> items = type switch
        {
            "Weapon" => _context.Items.OfType<Weapon>().ToList<Item>(),
            "Armor" => _context.Items.OfType<Armor>().ToList<Item>()
        };

        items = (type, SortOrder) switch
        {
            ("Weapon", "ASC") => items.OrderBy(i => ((Weapon)i).AttackPower).ToList(),
            ("Weapon", "DESC") => items.OrderByDescending(i => ((Weapon)i).AttackPower).ToList(),
            ("Armor", "ASC") => items.OrderBy(i => ((Armor)i).DefensePower).ToList(),
            ("Armor", "DESC") => items.OrderByDescending(i => ((Armor)i).DefensePower).ToList()
        };

        return items;
    }
}
