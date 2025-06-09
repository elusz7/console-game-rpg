using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class PlayerDao(GameContext context) : IPlayerDao
{
    private readonly GameContext _context = context;

    public void AddPlayer(Player player)
    {
        _context.Players!.Add(player);
        _context.SaveChanges();
    }

    public void UpdatePlayer(Player player)
    {
        _context.Players!.Update(player);
        _context.SaveChanges();
    }

    public void DeletePlayer(Player player)
    {
        _context.Players!.Remove(player);
        _context.SaveChanges();
    }

    public Player? GetPlayerByName(string name)
    {
        return _context.Players!.FirstOrDefault(p => p.Name.ToLower() == name.ToLower());
    }

    public Player GetPlayerNoTracking(Player player)
    {
        var loadedPlayer = _context.Players!
            .Include(p => p.Inventory)
                .ThenInclude(i => i.Items)
            .Include(p => p.Archetype)
                .ThenInclude(a => a.Skills)
            .Include(p => p.CurrentRoom)
                .ThenInclude(r => r.Monsters)
            .First(p => p.Id == player.Id);

        // Detach the root entity and all its related entities
        //DetachEntityGraph(loadedPlayer);

        return loadedPlayer;
    }

    private void DetachEntityGraph(object entity)
    {
        var visited = new HashSet<object>();
        var stack = new Stack<object>();
        stack.Push(entity);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null || visited.Contains(current))
                continue;

            visited.Add(current);

            try
            {
                _context.Entry(current).State = EntityState.Detached;
            }
            catch (InvalidOperationException)
            {
                // Not an entity, just skip
            }

            var properties = current.GetType().GetProperties()
                .Where(p =>
                    p.GetMethod != null &&
                    !p.PropertyType.IsPrimitive &&
                    p.PropertyType != typeof(string) &&
                    p.PropertyType != typeof(DateTime) &&
                    !p.PropertyType.IsEnum &&
                    !typeof(IDictionary<long, string>).IsAssignableFrom(p.PropertyType));

            foreach (var prop in properties)
            {
                var value = prop.GetValue(current);

                if (value is IEnumerable<object> collection && !(value is string))
                {
                    foreach (var item in collection)
                    {
                        if (item != null && !visited.Contains(item))
                            stack.Push(item);
                    }
                }
                else if (value != null && !visited.Contains(value))
                {
                    stack.Push(value);
                }
            }
        }
    }


    public List<Player> GetAllPlayers(string name)
    {
        return [.. _context.Players!
            .ToList()
            .Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Player> GetAllPlayers()
    {
        return [.. _context.Players!];
    }
    public List<Archetype> GetAllPlayerArchetypes()
    {
        return [.. _context.Players!
         .Select(p => p.Archetype)
         .Distinct()];
    }

    public List<Player> GetAllPlayersByArchetype(Archetype archetype)
    {
        return [.. _context.Players!.ToList()
            .Where(p => p.Archetype == archetype)];
    }

    public List<Player> GetAllPlayersByLevel(int level)
    {
        return [.. _context.Players!
            .Where(p => p.Level == level)];
    }
}
