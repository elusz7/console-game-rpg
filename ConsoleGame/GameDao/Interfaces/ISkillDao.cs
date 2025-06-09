using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.GameDao.Interfaces;

public interface ISkillDao
{
    public void AddSkill(Skill skill);
    public void UpdateSkill(Skill skill);
    public void DeleteSkill(Skill skill);
    public List<Skill> GetAllSkills();
    public List<Skill> GetSkillsByName(string name);
    public List<Skill> GetAllNonCoreSkills();
    public List<Archetype> GetArchetypes();
    public List<Monster> GetMonsters();
    public List<Skill> GetSkillsByLevel(int level);
    public List<Skill> GetSkillsByArchetype(Archetype archetype);
    public List<Skill> GetSkillsByMonster(Monster monster);
    public List<Skill> GetUnassignedSkills();
    public List<Skill> GetUnassignedNonCoreSkills();
    public List<Skill> GetAllAssignedNonCoreSkills();
    public List<Skill> GetUnassignedSkillsByMaxLevel(int level);
}
