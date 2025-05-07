using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.Helpers;

public class SkillDisplay
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly SkillDao _skillDao;

    public SkillDisplay(InputManager inputManager, OutputManager outputManager, SkillDao skillDao)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _skillDao = skillDao;
    }

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Skill Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. List All Skills"
                + "\n2. List Skills By Level"
                + "\n3. List Skills By Archetype"
                + "\n4. List Skills By Monster"
                + "\n5. List Unassigned Skills"
                + "\n6. Return to Main Menu");
            
            var input = _inputManager.ReadMenuKey(6);
            
            switch (input)
            {
                case 1:
                    ListSkills();
                    break;
                case 2:
                    ListSkills("Level");
                    break;
                case 3:
                    ListSkills("Archetype");
                    break;
                case 4:
                    ListSkills("Monster");
                    break;
                case 5:
                    ListSkills("Unassigned");
                    break;
                case 6:
                    _outputManager.Clear();
                    return;
            }
        }
    }

    private void ListSkills(string? criteria = null)
    {
        _outputManager.Clear();
        _outputManager.WriteLine("Skills List", ConsoleColor.Cyan);

        var skills = criteria switch
        {
            "Level" => SelectLevel(),
            "Archetype" => SelectArchetype(),
            "Monster" => SelectMonster(),
            "Unassigned" => _skillDao.GetUnassignedSkills(),
            _ => _skillDao.GetAllSkills()
        };

        if (skills.Count == 0)
        {
            _outputManager.WriteLine("No skills found.", ConsoleColor.Red);
            return;
        }

        _inputManager.PaginateList(skills);
    }

    private List<Skill> SelectLevel()
    {
        var level = _inputManager.ReadInt("Enter the level of the skills you want to see: ");

        return _skillDao.GetSkillsByLevel(level);
    }

    private List<Skill> SelectArchetype()
    {
        var archetypes = _skillDao.GetArchetypes();

        if (archetypes.Count == 0)
        {
            _outputManager.WriteLine("No archetypes found.", ConsoleColor.Red);
            return [];
        }

        var selectedArchetype = _inputManager.PaginateList(archetypes, "archetype", "skills to view", true, false);

        if (selectedArchetype == null)
        {
            _outputManager.WriteLine("No archetype selected.", ConsoleColor.Red);
            return [];
        }

        return _skillDao.GetSkillsByArchetype(selectedArchetype);
    }

    private List<Skill> SelectMonster()
    {
        var monsters = _skillDao.GetMonsters();

        if (monsters.Count == 0)
        {
            _outputManager.WriteLine("No monsters found.", ConsoleColor.Red);
            return [];
        }

        var selectedMonster = _inputManager.PaginateList(monsters, "monster", "skills to view", true, false);

        if (selectedMonster == null)
        {
            _outputManager.WriteLine("No monster selected.", ConsoleColor.Red);
            return [];
        }

        return _skillDao.GetSkillsByMonster(selectedMonster);
    }
}
