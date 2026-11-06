using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGame.GameDao;

public class ArchetypeDao(GameContext context) : IArchetypeDao
{
    private readonly GameContext _context = context;

    public void AddArchetype(Archetype archetype)
    {
        _context.Archetypes.Add(archetype);
        _context.SaveChanges();
    }

    public void UpdateArchetype(Archetype archetype)
    {
        _context.Archetypes.Update(archetype);
        _context.SaveChanges();
    }

    public void DeleteArchetype(Archetype archetype)
    {
        if (archetype.IsCore)
            throw new InvalidOperationException("Core archetype can't be deleted");
        
        _context.Archetypes.Remove(archetype);
        _context.SaveChanges();
    }
    

    public List<Archetype> GetArchetypesByType(ArchetypeType archetypeType)
    {
        return
        [
            .. _context.Archetypes
                .Where(a => a.ArchetypeType == archetypeType)
        ];
    }

    public List<Archetype> GetAllArchetypes()
    {
        return [.. _context.Archetypes];
    }

    public List<Archetype> GetAllNonCoreArchetypes()
    {
        return [.. _context.Archetypes.Where(a => !a.IsCore)];
    }
}