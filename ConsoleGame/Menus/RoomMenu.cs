using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;

namespace ConsoleGame.Menus;

public class RoomMenu(InputManager inputManager, OutputManager outputManager, RoomDisplay roomDisplay, 
    RoomManagement roomManagement, RoomConnectionManagement roomConnectionManagement)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly RoomDisplay _roomDisplay = roomDisplay;
    private readonly RoomManagement _roomManagement = roomManagement;
    private readonly RoomConnectionManagement _roomConnectionManagement = roomConnectionManagement;

    public void RoomMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Room Main Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View Rooms"
                + "\n2. Manage Rooms"
                + "\n3. Update Room Connections"
                + "\n4. Return to Admin Menu");
            
            var input = _inputManager.ReadMenuKey(4);

            switch (input)
            {
                case 1:
                    _roomDisplay.Menu();
                    break;
                case 2:
                    _roomManagement.Menu();
                    break;
                case 3:
                    _roomConnectionManagement.Menu();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }
}


