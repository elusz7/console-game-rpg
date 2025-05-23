using ConsoleGameEntities.Models.Entities;
using ConsoleGame.Managers.Interfaces;

namespace ConsoleGame.Managers;

public class MapManager(IOutputManager outputManager, IInputManager inputManager) : IMapManager
{
    private const int RoomNameLength = 5;
    private const int gridRows = 8;
    private const int gridCols = 8;

    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;

    private readonly string[,] mapGrid = new string[gridRows, gridCols];
    private Room _currentRoom;

    public void DisplayMap()
    {
        for (var i = 0; i < gridRows; i++)
        {
            for (var j = 0; j < gridCols; j++)
            {
                mapGrid[i, j] = "       ";
            }
        }

        if (_currentRoom != null)
        {
            var startRow = gridRows / 2;
            var startCol = gridCols / 2;
            PlaceRoom(_currentRoom, startRow, startCol, []);
        }

        for (var i = 0; i < gridRows; i++)
        {
            for (var j = 0; j < gridCols; j++)
            {
                if (mapGrid[i, j].Contains('[') && mapGrid[i, j].Contains(']'))
                {
                    if (mapGrid[i, j] == $"[{_currentRoom?.Name[..RoomNameLength]}]")
                    {
                        _outputManager.Write($"{mapGrid[i, j],-7}", ConsoleColor.Green);
                    }
                    else
                    {
                        _outputManager.Write($"{mapGrid[i, j],-7}");
                    }
                }
                else
                {
                    _outputManager.Write($"{mapGrid[i, j],-7}");
                }
            }

            _outputManager.WriteLine("");
        }
        _outputManager.Display();
    }
    public void UpdateCurrentRoom(Room currentRoom)
    {
        _currentRoom = currentRoom;
    }
    private void PlaceRoom(Room room, int row, int col, HashSet<Room> visitedRooms)
    {
        if (visitedRooms.Contains(room))
            return; // Already placed and processed, don't do it again!

        visitedRooms.Add(room); // Mark this room as visited

        var roomName = room.Name.Length > RoomNameLength
            ? room.Name[..RoomNameLength]
            : room.Name.PadRight(RoomNameLength);

        mapGrid[row, col] = $"[{roomName}]";

        if (room.North != null && row > 1)
        {
            mapGrid[row - 1, col] = "   |   ";
            PlaceRoom(room.North, row - 2, col, visitedRooms);
        }

        if (room.South != null && row < gridRows - 2)
        {
            mapGrid[row + 1, col] = "   |   ";
            PlaceRoom(room.South, row + 2, col, visitedRooms);
        }

        if (room.East != null && col < gridCols - 2)
        {
            mapGrid[row, col + 1] = "  ---  ";
            PlaceRoom(room.East, row, col + 2, visitedRooms);
        }

        if (room.West != null && col > 1)
        {
            mapGrid[row, col - 1] = "  ---  ";
            PlaceRoom(room.West, row, col - 2, visitedRooms);
        }
    }
    public void TraverseMap(Room currentRoom)
    {
        _outputManager.Clear();
        _currentRoom = currentRoom;
        DisplayMap();

        _outputManager.WriteLine("\nPress direction keys to traverse map. Any other key returns to the menu.");
        _outputManager.Display();

        while (true)
        {

            ConsoleKeyInfo key = _inputManager.ReadKey();

            if (key.Key == ConsoleKey.UpArrow)
            {
                if (_currentRoom.North != null)
                {
                    _currentRoom = _currentRoom.North;
                    _outputManager.Clear();
                    UpdateCurrentRoom(_currentRoom);
                    DisplayMap();
                    _outputManager.WriteLine("\nPress direction keys to change room or any other key to return to the menu.");
                    _outputManager.Display();
                }
                else
                {
                    _outputManager.WriteLine("No room to the North.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }
            else if (key.Key == ConsoleKey.DownArrow)
            {
                if (_currentRoom.South != null)
                {
                    _currentRoom = _currentRoom.South;
                    _outputManager.Clear();
                    UpdateCurrentRoom(_currentRoom);
                    DisplayMap();
                    _outputManager.WriteLine("\nPress direction keys to change room or any other key to return to the menu.");
                    _outputManager.Display();
                }
                else
                {
                    _outputManager.WriteLine("No room to the South.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }
            else if (key.Key == ConsoleKey.LeftArrow)
            {
                if (_currentRoom.West != null)
                {
                    _currentRoom = _currentRoom.West;
                    _outputManager.Clear();
                    UpdateCurrentRoom(_currentRoom);
                    DisplayMap();
                    _outputManager.WriteLine("\nPress direction keys to change room or any other key to return to the menu.");
                    _outputManager.Display();
                }
                else
                {
                    _outputManager.WriteLine("No room to the West.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }
            else if (key.Key == ConsoleKey.RightArrow)
            {
                if (_currentRoom.East != null)
                {
                    _currentRoom = _currentRoom.East;
                    _outputManager.Clear();
                    UpdateCurrentRoom(_currentRoom);
                    DisplayMap();
                    _outputManager.WriteLine("\nPress direction keys to change room or any other key to return to the menu.");
                    _outputManager.Display();
                }
                else
                {
                    _outputManager.WriteLine("No room to the East.", ConsoleColor.Red);
                    _outputManager.Display();
                }
            }
            else
            {
                break; // any other key exits the map view
            }
        }

        _outputManager.Clear();
    }
}