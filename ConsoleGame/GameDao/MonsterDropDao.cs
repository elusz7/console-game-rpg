using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Runes.Recipes;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class MonsterDropDao(GameContext context) : IMonsterDropDao
{
    private readonly GameContext _context = context;

    public List<MonsterDrop> GetDropsByElementType(ElementType element)
    {
        return
        [
            .. _context.MonsterDrops
                .Include(md => md.Ingredient)
                .Where(md => md.Element == element)
        ];
    }

    public List<MonsterDrop> GetDropsByThreatLevel(ThreatLevel level)
    {
        return
        [
            .. _context.MonsterDrops
                .Include(md => md.Ingredient)
                .Where(md => md.ThreatLevel == level)
        ];
    }

    public List<MonsterDrop> GetDropsByElementTypeAndThreatLevel(ElementType element, ThreatLevel level)
    {
        return
        [
            .. _context.MonsterDrops
                .Include(md => md.Ingredient)
                .Where(md => md.Element == element 
                        && md.ThreatLevel == level)
        ];
    }
}