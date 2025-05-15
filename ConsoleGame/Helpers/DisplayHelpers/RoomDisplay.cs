using ConsoleGame.GameDao;
using ConsoleGame.Helpers.DisplayHelpers;

namespace ConsoleGame.Managers.DisplayHelpers;

public class RoomDisplay(OutputManager outputManager, InputManager inputManager, MapManager mapManager, RoomDao roomDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly MapManager _mapManager = mapManager;
    private readonly RoomDao _roomDao = roomDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Room Display Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View All Rooms"
                + "\n2. View Map"
                + "\n3. Return to Room Menu");

            var input = _inputManager.ReadMenuKey(3);

            switch (input)
            {
                case 1:
                    ViewAllRooms();
                    break;
                case 2:
                    _mapManager.TraverseMap(_roomDao.GetEntrance());
                    break;
                case 3:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void ViewAllRooms()
    {
        var rooms = _roomDao.GetAllRooms();
        
        _inputManager.Viewer(rooms, r => ColorfulToStringHelper.RoomToString(r), "");

        _outputManager.Clear();
    }
}
