namespace ConsoleGame.Helpers;

public class PlayerManager
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly PlayerDisplay _playerDisplayMenu;
    private readonly PlayerManagement _playerManagementMenu;
    public PlayerManager(InputManager inputManager, OutputManager outputManager, InventoryManager inventoryManager, PlayerDisplay playerDisplayMenu, PlayerManagement playerManagementMenu)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _playerDisplayMenu = playerDisplayMenu;
        _playerManagementMenu = playerManagementMenu;
    }

    public void PlayerMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Player Main Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. View Players"
                + "\n2. Manage Players"
                + "\n3. Return to Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);

            switch (choice)
            {
                case "1":
                    _playerDisplayMenu.Menu();
                    break;
                case "2":
                    _playerManagementMenu.Menu();
                    break;
                case "3":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }    
}
