using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class ItemDao
{
    private readonly IDbContextFactory<GameContext> _contextFactory;
    public string SortOrder;
    public ItemDao(IDbContextFactory<GameContext> contextFactory)
    {
        _contextFactory = contextFactory;
        SortOrder = "ASC";
    }
    public void AddItem(Item item)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Items.Add(item);
        context.SaveChanges();
    }

    public void UpdateItem(Item item)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Items.Update(item);
        context.SaveChanges();
    }

    public void DeleteItem(Item item)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Items.Remove(item);
        context.SaveChanges();
    }

    public Item GetItemByName(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Items.Where(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
    }

    public List<Item> GetAllItems(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        
        List<Item> matchingItems = context.Items
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
        using var context = _contextFactory.CreateDbContext();
        
        return SortOrder switch
        {
            "ASC" => context.Items.OrderBy(i => i.Name).ToList(),
            "DESC" => context.Items.OrderByDescending(i => i.Name).ToList()
        };
    }

    public List<Item> GetItemsByType(string type)
    {
        using var context = _contextFactory.CreateDbContext();

        List<Item> items = type switch
        {
            "Weapon" => context.Items.OfType<Weapon>().ToList<Item>(),
            "Armor" => context.Items.OfType<Armor>().ToList<Item>()
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
