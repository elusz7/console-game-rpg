using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers;

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
                    FindPlayerByName();
                    break;
                case 2:
                    ListAllPlayers(); 
                    break;
                case 3:
                    ListArchetypePlayers();
                    break;
                case 4:
                    ListLevelPlayers();
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void ListAllPlayers()
    {
        var playerList = _playerDao.GetAllPlayers();

        if (playerList.Count == 0)
        {
            _outputManager.WriteLine("\nNo Players found.\n");
            return;
        }

        _inputManager.PaginateList(playerList);
    }
    private void FindPlayerByName()
    {
        var name = _inputManager.ReadString("\nEnter name of Player: ");

        var matchingPlayers = _playerDao.GetAllPlayers(name);

        if (matchingPlayers.Count == 0)
        {
            _outputManager.WriteLine($"\nNo Players found matching [{name}]\n");
        }
        else
        {
            _outputManager.WriteLine($"\nFound {matchingPlayers.Count} Player(s) matching [{name}]:");
            _inputManager.PaginateList(matchingPlayers, clearScreen: false);
        }
    }
    public void ListArchetypePlayers()
    {
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

        var players = _playerDao.GetAllPlayersByArchetype(archetype);

        if (players.Count == 0)
        {
            _outputManager.WriteLine($"\nNo Players found for Archetype [{archetype}]\n");
            return;
        }

        _outputManager.WriteLine($"\nFound {players.Count} Player(s) for Archetype [{archetype}]:");
        _inputManager.PaginateList(players, clearScreen: false);
    }
    public void ListLevelPlayers()
    {
        var level = _inputManager.ReadInt("\nEnter level of Players you wish to see: ");

        var players = _playerDao.GetAllPlayersByLevel(level);

        if (players.Count == 0)
        {
            _outputManager.WriteLine($"\nNo Players found for Level [{level}]\n");
            return;
        }
        _outputManager.WriteLine($"\nFound {players.Count} Player(s) for Level [{level}]:");
        _inputManager.PaginateList(players, clearScreen: false);
    }
}
