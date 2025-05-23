using ConsoleGame.Helpers.CrudHelpers;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Managers.Interfaces;

namespace ConsoleGame.Menus;

public class RoomMenu(IInputManager inputManager, IOutputManager outputManager, RoomDisplay roomDisplay, 
    RoomManagement roomManagement, RoomConnectionManagement roomConnectionManagement)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
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


