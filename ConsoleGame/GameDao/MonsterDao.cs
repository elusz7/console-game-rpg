using ConsoleGameEntities.Main.Data;
using ConsoleGameEntities.Main.Models.Monsters;
using static ConsoleGameEntities.Main.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class MonsterDao(GameContext context)
{
    private readonly GameContext _context = context;

    public void AddMonster(Monster monster)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        _context.Monsters.Add(monster);
        _context.SaveChanges();
    }

    public List<Monster> GetAllMonsters()
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters];
    }

    public List<Monster> GetMonstersByName(string name)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.ToList().Where(m => m.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }

    public void UpdateMonster(Monster monster)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        _context.Monsters.Update(monster);
        _context.SaveChanges();
    }

    public void DeleteMonster(Monster monster)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        _context.Monsters.Remove(monster);
        _context.SaveChanges();
    }

    public List<Monster> GetAllNonCoreMonsters()
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => m.Id > 28)];
    }

    public List<Monster> GetMonstersByThreatLevel(ThreatLevel threatLevel)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => m.ThreatLevel == threatLevel)];
    }

    public List<Monster> GetMonstersByDamageType(DamageType damageType)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => m.DamageType == damageType)];
    }

    public List<Monster> GetNonBossMonstersByMaxLevel(int level)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => m.Level <= level && m.ThreatLevel != ThreatLevel.Boss)];
    }

    public List<Monster> GetMonstersByMaxLevelAndThreatLevel(int level, int threatLevel)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => m.Level <= level && (int)m.ThreatLevel == threatLevel)];
    }
}
