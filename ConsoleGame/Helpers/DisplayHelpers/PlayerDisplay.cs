using System.Xml.Linq;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Managers.DisplayHelpers;

public class PlayerDisplay(InputManager inputManager, OutputManager outputManager, PlayerDao playerDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly PlayerDao _playerDao = playerDao;
    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Player Display Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Search Player By Name"
                + "\n2. List All Players"
                + "\n3. List Players By Archetype"
                + "\n4. List Players By Level"
                + "\n5. Return to Player Main Menu");

            var choice = _inputManager.ReadMenuKey(5);

            switch (choice)
            {
                case 1:
                    ListPlayers("Search");
                    break;
                case 2:
                    ListPlayers(); 
                    break;
                case 3:
                    ListPlayers("Archetype");
                    break;
                case 4:
                    ListPlayers("Level");
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void ListPlayers(string? criteria = null)
    {
        var players = new List<Player>();

        switch (criteria)
        {
            case "Search":
                var playerName = _inputManager.ReadString("\nEnter player name to find: ");
                players = _playerDao.GetAllPlayers(playerName);

                _outputManager.WriteLine($"\nFound {players.Count} Player(s) matching [{playerName}]:");
                break;
            case "Archetype":
                var archetypes = _playerDao.GetAllPlayerArchetypes();
                if (archetypes.Count == 0)
                {
                    _outputManager.WriteLine("\nNo Archetypes found.\n");
                    return;
                }

                var archetype = _inputManager.PaginateList(archetypes, "archetype", "view players of", true);

                if (archetype == null)
                {
                    _outputManager.WriteLine("\nNo Archetype selected.\n");
                    return;
                }

                players = _playerDao.GetAllPlayersByArchetype(archetype);
                break;
            case "Level":
                var level = _inputManager.ReadInt("\nEnter level of Players you wish to see: ");
                players = _playerDao.GetAllPlayersByLevel(level);
                break;
            default:
                players = _playerDao.GetAllPlayers();
                break;
        }

        if (players.Count == 0)
        {
            _outputManager.WriteLine("\nNo Players found.\n");
            return;
        }

        _inputManager.PaginateList(players, clearScreen: false);
    }
}
