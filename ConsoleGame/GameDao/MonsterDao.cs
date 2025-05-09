using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class MonsterDao
{
    private readonly GameContext _context;

    public MonsterDao(GameContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

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

    public List<Monster> GetMonstersByThreatLevel(int threatLevel)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => (int)m.ThreatLevel == threatLevel)];
    }

    public List<Monster> GetMonstersByDamageType(int damageType)
    {
        if (_context.Monsters == null) throw new InvalidOperationException("Monsters DbSet is null.");
        return [.. _context.Monsters.Where(m => (int)m.DamageType == damageType)];
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
