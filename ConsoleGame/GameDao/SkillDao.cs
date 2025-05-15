using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.GameDao;

public class SkillDao(GameContext context)
{
    private readonly GameContext _context = context;

    public void AddSkill(Skill skill)
    {
        _context.Skills.Add(skill);
        _context.SaveChanges();
    }

    public void UpdateSkill(Skill skill)
    {
        _context.Skills.Update(skill);
        _context.SaveChanges();
    }

    public void DeleteSkill(Skill skill)
    {
        _context.Skills.Remove(skill);
        _context.SaveChanges();
    }

    public List<Skill> GetAllSkills()
    {
        return [.. _context.Skills];
    }
    public List<Skill> GetSkillsByName(string name)
    {
        return [.. _context.Skills.ToList()
            .Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase))];
    }
    public List<Skill> GetAllNonCoreSkills()
    {
        return [.. _context.Skills.Where(s => s.Id > 212)];
    }

    public List<Archetype> GetArchetypes()
    {
        return [.. _context.Skills
            .Where(s => s.Archetype != null)
            .Select(s => s.Archetype)
            .Distinct()];
    }

    public List<Monster> GetMonsters()
    {
        return [.. _context.Skills
            .Where(s => s.Monster != null)
            .Select(s => s.Monster)
            .Distinct()];
    }

    public List<Skill> GetSkillsByLevel(int level)
    {
        return [.. _context.Skills
            .Where(s => s.RequiredLevel == level)];
    }

    public List<Skill> GetSkillsByArchetype(Archetype archetype)
    {
        return [.. _context.Skills
            .Where(s => s.ArchetypeId == archetype.Id)];
    }

    public List<Skill> GetSkillsByMonster(Monster monster)
    {
        return [.. _context.Skills
            .Where(s => s.MonsterId == monster.Id)];
    }

    public List<Skill> GetUnassignedSkills()
    {
        return [.. _context.Skills
            .Where(s => s.ArchetypeId == null && s.MonsterId == null)];
    }

    public List<Skill> GetUnassignedNonCoreSkills()
    {
        return [.. _context.Skills
            .Where(s => s.ArchetypeId == null && s.MonsterId == null)
            .Where(s => s.Id > 212)];
    }

    public List<Skill> GetAllAssignedNonCoreSkills()
    {
        return [.. _context.Skills
            .Where(s => s.ArchetypeId != null || s.MonsterId != null)
            .Where(s => s.Id > 212)];
    }
    public List<Skill> GetUnassignedSkillsByMaxLevel(int level)
    {
        return [.. _context.Skills
            .Where(s => s.RequiredLevel <= level && s.ArchetypeId == null && s.MonsterId == null)];
    }
}
