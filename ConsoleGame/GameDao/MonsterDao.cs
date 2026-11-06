using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Monsters;
using Microsoft.EntityFrameworkCore;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class MonsterDao(GameContext context) : IMonsterDao
{
    private readonly GameContext _context = context;

    public void AddMonster(Monster monster)
    {
        _context.Monsters.Add(monster);
        _context.SaveChanges();
    }

    public List<Monster> GetAllMonsters()
    {
        return [.. _context.Monsters];
    }

    public List<Monster> GetMonstersByName(string name)
    {
        return [.. _context.Monsters
            .Where(m => EF.Functions.Like(m.Name, $"%{name}%"))];
    }

    public void UpdateMonster(Monster monster)
    {
        _context.Monsters.Update(monster);
        _context.SaveChanges();
    }

    public void DeleteMonster(Monster monster)
    {
        if (monster.IsCore)
            throw new InvalidOperationException("Core game monsters cannot be deleted.");
        
        _context.Monsters.Remove(monster);
        _context.SaveChanges();
    }

    public List<Monster> GetAllNonCoreMonsters()
    {
        return [.. _context.Monsters.Where(m => !m.IsCore)];
    }

    public List<Monster> GetMonstersByThreatLevel(ThreatLevel threatLevel)
    {
        return [.. _context.Monsters.Where(m => m.ThreatLevel == threatLevel)];
    }

    public List<Monster> GetMonstersByDamageType(DamageType damageType)
    {
        return [.. _context.Monsters.Where(m => m.DamageType == damageType)];
    }

    public List<Monster> GetNonBossMonstersByMaxLevel(int level)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);
        
        return [.. _context.Monsters.Where(m => m.Level <= level && m.ThreatLevel != ThreatLevel.Boss)];
    }

    public List<Monster> GetMonstersByMaxLevelAndThreatLevel(int level, ThreatLevel threatLevel)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(level);
        
        return [.. _context.Monsters.Where(m => m.Level <= level && m.ThreatLevel == threatLevel)];
    }
}