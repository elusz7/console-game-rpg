using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;

namespace ConsoleGame.Helpers;

public class RoomDisplay
{
    private readonly OutputManager _outputManager;
    private readonly InputManager _inputManager;
    private readonly RoomDao _roomDao;

    public RoomDisplay(OutputManager outputManager, InputManager inputManager, RoomDao roomDao)
    {
        _outputManager = outputManager;
        _inputManager = inputManager;
        _roomDao = roomDao;
    }
    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            string menuPrompt = "Room Display Menu:"
                + "\n1. View All Rooms"
                + "\n2. Return to Room Menu"
                + "\n\tSelect an option: ";

            var input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    ViewAllRooms();
                    break;
                case "2":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    break;
            }

        }
    }
    private void ViewAllRooms()
    {
        _outputManager.Clear();
        var rooms = _roomDao.GetAllRooms();
        
        foreach (var room in rooms)
        {
            _outputManager.WriteLine(room.ToString());
        }
    }
}
