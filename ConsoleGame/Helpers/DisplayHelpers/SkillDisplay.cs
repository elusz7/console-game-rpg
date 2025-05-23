using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGame.Helpers.DisplayHelpers;

public class SkillDisplay(IInputManager inputManager, IOutputManager outputManager, ISkillDao skillDao)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly ISkillDao _skillDao = skillDao;

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
                + "\n6. Search Skills By Name"
                + "\n7. Return to Skill Menu");
            
            var input = _inputManager.ReadMenuKey(7);
            
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
                    ListSkills("Search");
                    break;
                case 7:
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
            "Search" => SelectName(),
            _ => _skillDao.GetAllSkills()
        };

        if (skills.Count == 0)
        {
            _outputManager.WriteLine("No skills found.", ConsoleColor.Red);
            return;
        }

        _inputManager.Viewer(skills, s => ColorfulToStringHelper.SkillToString(s), "", s => ColorfulToStringHelper.GetSkillColor(s));
    }

    private List<Skill> SelectLevel()
    {
        var level = _inputManager.ReadInt("Enter the level of the skills you want to see: ");

        return _skillDao.GetSkillsByLevel(level);
    }
    private List<Skill> SelectName()
    {
        var name = _inputManager.ReadString("Enter the name of the skill you want to see: ");
        return _skillDao.GetSkillsByName(name);
    }

    private List<Skill> SelectArchetype()
    {
        var archetypes = _skillDao.GetArchetypes();

        if (archetypes.Count == 0)
        {
            _outputManager.WriteLine("No archetypes found.", ConsoleColor.Red);
            return [];
        }

        var selectedArchetype = _inputManager.Selector(archetypes, a => ColorfulToStringHelper.ArchetypeToString(a), "Select an archetype to see their skills", a => ColorfulToStringHelper.GetArchetypeColor(a));

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

        var selectedMonster = _inputManager.Selector(monsters, a => ColorfulToStringHelper.MonsterToString(a), "Select a monster to see their skills", m => ColorfulToStringHelper.GetMonsterColor(m));

        if (selectedMonster == null)
        {
            _outputManager.WriteLine("No monster selected.", ConsoleColor.Red);
            return [];
        }

        return _skillDao.GetSkillsByMonster(selectedMonster);
    }
}
