namespace ConsoleGame.Helpers;

public class RoomManager
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly MapManager _mapManager;
    private readonly RoomDisplay _roomDisplay;
    private readonly RoomManagement _roomManagement;
    private readonly RoomConnectionManagement _roomConnectionManagement;

    public RoomManager(InputManager inputManager, OutputManager outputManager, MapManager mapManager, 
        RoomDisplay roomDisplay, RoomManagement roomManagement, RoomConnectionManagement roomConnectionManagement)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _mapManager = mapManager;
        _roomDisplay = roomDisplay;
        _roomManagement = roomManagement;
        _roomConnectionManagement = roomConnectionManagement;
    }
    public void RoomMainMenu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Room Main Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View Rooms"
                + "\n2. View Map"
                + "\n3. Manage Rooms"
                + "\n4. Update Room Connections"
                + "\n5. Return to Main Menu");
            
            var input = _inputManager.ReadMenuKey(5);

            switch (input)
            {
                case 1:
                    _roomDisplay.ViewAllRooms();
                    break;
                case 2:
                    _mapManager.TraverseMap();
                    break;
                case 3:
                    _roomManagement.Menu();
                    break;
                case 4:
                    _roomConnectionManagement.Menu();
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
            }
        }
    }
}


