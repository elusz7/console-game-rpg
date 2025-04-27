using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGameEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ConsoleGame.Helpers;

public class MapManager
{
    private const int RoomNameLength = 5;
    private const int gridRows = 8;
    private const int gridCols = 8;

    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly RoomDao _roomDao;

    private readonly string[,] mapGrid;
    private Room _currentRoom;

    private record RoomPosition(Room Room, int X, int Y);

    public MapManager(OutputManager outputManager, InputManager inputManager, RoomDao roomDao)
    {
        _outputManager = outputManager;
        _inputManager = inputManager;
        _roomDao = roomDao;
        mapGrid = new string[gridRows, gridCols];
        _currentRoom = _roomDao.GetEntrance();
    }
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
            PlaceRoom(_currentRoom, startRow, startCol);
        }

        for (var i = 0; i < gridRows; i++)
        {
            for (var j = 0; j < gridCols; j++)
            {
                if (mapGrid[i, j].Contains("[") && mapGrid[i, j].Contains("]"))
                {
                    if (mapGrid[i, j] == $"[{_currentRoom.Name.Substring(0, RoomNameLength)}]")
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
    private void PlaceRoom(Room room, int row, int col)
    {
        if (mapGrid[row, col] != "       ")
        {
            return;
        }

        var roomName = room.Name.Length > RoomNameLength
            ? room.Name.Substring(0, RoomNameLength)
            : room.Name.PadRight(RoomNameLength);

        if (room == _currentRoom)
        {
            mapGrid[row, col] = $"[{roomName}]";
        }
        else
        {
            mapGrid[row, col] = $"[{roomName}]";
        }

        if (room.North != null && row > 1)
        {
            mapGrid[row - 1, col] = "   |   ";
            PlaceRoom(room.North, row - 2, col);
        }

        if (room.South != null && row < gridRows - 2)
        {
            mapGrid[row + 1, col] = "   |   ";
            PlaceRoom(room.South, row + 2, col);
        }

        if (room.East != null && col < gridCols - 2)
        {
            mapGrid[row, col + 1] = "  ---  ";
            PlaceRoom(room.East, row, col + 2);
        }

        if (room.West != null && col > 1)
        {
            mapGrid[row, col - 1] = "  ---  ";
            PlaceRoom(room.West, row, col - 2);
        }
    }
    public void TraverseMap()
    {
        _outputManager.Clear();
        _currentRoom = _roomDao.GetEntrance();
        UpdateCurrentRoom(_currentRoom);
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

        _currentRoom = _roomDao.GetEntrance();
        _outputManager.Clear();
    }
    public Dictionary<(int x, int y), Room> GetRoomGrid()
    {
        var root = _roomDao.GetEntrance();
        var visited = new HashSet<Room>();
        var grid = new Dictionary<(int x, int y), Room>();
        var queue = new Queue<RoomPosition>();

        queue.Enqueue(new RoomPosition(root, 0, 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current.Room)) continue;

            grid[(current.X, current.Y)] = current.Room;

            if (current.Room.North != null)
                queue.Enqueue(new RoomPosition(current.Room.North, current.X, current.Y + 1));
            if (current.Room.South != null)
                queue.Enqueue(new RoomPosition(current.Room.South, current.X, current.Y - 1));
            if (current.Room.East != null)
                queue.Enqueue(new RoomPosition(current.Room.East, current.X + 1, current.Y));
            if (current.Room.West != null)
                queue.Enqueue(new RoomPosition(current.Room.West, current.X - 1, current.Y));
        }

        return grid;
    }
    public (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid)
    {
        foreach (var kvp in grid)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        throw new InvalidOperationException("Room not found in grid.");
    }
}