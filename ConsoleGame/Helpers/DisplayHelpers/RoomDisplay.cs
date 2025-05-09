using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;

namespace ConsoleGame.Helpers.DisplayHelpers;

public class RoomDisplay(OutputManager outputManager, InputManager inputManager, RoomDao roomDao)
{
    private readonly OutputManager _outputManager = outputManager;
    private readonly InputManager _inputManager = inputManager;
    private readonly RoomDao _roomDao = roomDao;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("=== Room Display Menu ===", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. View All Rooms"
                + "\n2. Return to Room Menu");

            var input = _inputManager.ReadMenuKey(2);

            switch (input)
            {
                case 1:
                    ViewAllRooms();
                    break;
                case 2:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    public void ViewAllRooms()
    {
        var rooms = _roomDao.GetAllRooms();
        
        _inputManager.PaginateList(rooms);

        _outputManager.Clear();
    }
}
