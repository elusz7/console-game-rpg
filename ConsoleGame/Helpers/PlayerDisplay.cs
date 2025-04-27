using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Characters;

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
            string menuPrompt = "1. List All Players"
                + "\n2. Search Player By Name"
                + "\n3. Return to Player Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

            switch (choice)
            {
                case "1":
                    ListPlayersFullInfo();
                    break;
                case "2":
                    FindPlayerByName();
                    break;
                case "3":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void ListPlayersFullInfo(List<Player>? playerList = null)
    {
        if (playerList == null)
            playerList = _playerDao.GetAllPlayers();

        _outputManager.WriteLine();
        foreach (var p in playerList)
        {
            _outputManager.WriteLine(p.ToString());
        }
        _outputManager.WriteLine();
        _outputManager.Display();
    }
    private void FindPlayerByName()
    {
        string name = _inputManager.ReadString("\nEnter name of Player: ");

        var matchingPlayers = _playerDao.GetAllPlayers(name);

        if (matchingPlayers.Count == 0)
        {
            _outputManager.WriteLine($"\nNo Players found matching [{name}]\n");
        }
        else
        {
            _outputManager.WriteLine($"\nFound {matchingPlayers.Count} Player(s) matching [{name}]:");
            ListPlayersFullInfo(matchingPlayers);
        }
    }
}
