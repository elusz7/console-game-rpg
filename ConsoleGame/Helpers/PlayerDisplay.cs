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
            _outputManager.WriteLine("1. List All Players"
                + "\n2. Search Player By Name"
                + "\n3. Return to Player Main Menu");

            var choice = _inputManager.ReadMenuKey(3);

            switch (choice)
            {
                case 1:
                    ListAllPlayers();
                    break;
                case 2:
                    FindPlayerByName();
                    break;
                case 3:
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

        _inputManager.PaginateList(playerList, i => i.ToString());
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
            _inputManager.PaginateList(matchingPlayers, i => i.ToString());
        }
    }
}
