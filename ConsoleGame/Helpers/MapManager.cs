using System.Collections.Concurrent;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Rooms;

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

    private Dictionary<(int x, int y), Room> _gridCache;
    public bool DirtyGrid { get; set; }
    private record RoomPosition(Room Room, int X, int Y);

    public MapManager(OutputManager outputManager, InputManager inputManager, RoomDao roomDao)
    {
        _outputManager = outputManager;
        _inputManager = inputManager;
        _roomDao = roomDao;
        mapGrid = new string[gridRows, gridCols];
        _currentRoom = _roomDao.GetEntrance();
        DirtyGrid = true;
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
            PlaceRoom(_currentRoom, startRow, startCol, new HashSet<Room>());
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
    private void PlaceRoom(Room room, int row, int col, HashSet<Room> visitedRooms)
    {
        if (visitedRooms.Contains(room))
            return; // Already placed and processed, don't do it again!

        visitedRooms.Add(room); // Mark this room as visited

        var roomName = room.Name.Length > RoomNameLength
            ? room.Name.Substring(0, RoomNameLength)
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
    private Dictionary<(int x, int y), Room> GetRoomGrid()
    {
        if (DirtyGrid)
        {
            var root = _roomDao.GetEntrance();
            var visited = new HashSet<Room>();
            _gridCache = new Dictionary<(int x, int y), Room>();
            var queue = new Queue<RoomPosition>();

            queue.Enqueue(new RoomPosition(root, 0, 0));

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!visited.Add(current.Room)) continue;

                _gridCache[(current.X, current.Y)] = current.Room;

                if (current.Room.North != null)
                    queue.Enqueue(new RoomPosition(current.Room.North, current.X, current.Y - 1)); // Y-1 for north
                if (current.Room.South != null)
                    queue.Enqueue(new RoomPosition(current.Room.South, current.X, current.Y + 1)); // Y+1 for south
                if (current.Room.East != null)
                    queue.Enqueue(new RoomPosition(current.Room.East, current.X + 1, current.Y));
                if (current.Room.West != null)
                    queue.Enqueue(new RoomPosition(current.Room.West, current.X - 1, current.Y));
            }
        }

        DirtyGrid = false;
        return _gridCache;
    }
    private (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid)
    {
        foreach (var kvp in grid)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        throw new InvalidOperationException("Room not found in grid.");
    }   
    public void CheckForDisconnectedRooms()
    {
        var list = new List<Room>();

        var grid = GetRoomGrid();

        foreach (var room in _roomDao.GetAllEditableRooms())
        {
            try
            {
                FindRoomCoordinates(room, grid);
            }
            catch (InvalidOperationException ex1)
            {
                //set disconnected room connects to null
                room.North = room.South = room.East = room.West = null;

                list.Add(room);
            }
        }

        _roomDao.UpdateAllRooms(list);
        DirtyGrid = true;
    }
    public List<Room> FindUnconnectedRooms()
    {
        var list = new List<Room>();

        var grid = GetRoomGrid();

        foreach (var room in _roomDao.GetAllEditableRooms())
        {
            try
            {
                FindRoomCoordinates(room, grid);
            }
            catch (InvalidOperationException ex1)
            {
                list.Add(room);
            }
        }

        return list;
    }
    private int CountPathsToEntranceThroughRoom(Room startRoom, Room requiredRoom, Room entrance)
    {
        int pathCount = 0;

        var queue = new Queue<Room>();
        var visitedRooms = new HashSet<Room>();

        // Start BFS from the startRoom
        queue.Enqueue(startRoom);

        // Keep track of the rooms that are visited as part of the current path
        var roomParents = new Dictionary<Room, Room>();

        while (queue.Count > 0)
        {
            var currentRoom = queue.Dequeue();

            if (currentRoom == entrance)
            {
                // Check if the requiredRoom is part of the path by walking back from entrance
                var pathContainsRequired = false;
                var parent = currentRoom;

                // Walk backwards through parent map to see if requiredRoom is part of the path
                while (parent != null)
                {
                    if (parent == requiredRoom)
                    {
                        pathContainsRequired = true;
                        break;
                    }

                    parent = roomParents.GetValueOrDefault(parent);
                }

                if (pathContainsRequired)
                {
                    pathCount++;
                }
                continue; // If reached entrance, no need to explore further
            }

            if (visitedRooms.Contains(currentRoom)) continue;

            visitedRooms.Add(currentRoom);

            // Enqueue neighbors (only unvisited rooms)
            if (currentRoom.North != null && !visitedRooms.Contains(currentRoom.North))
            {
                roomParents[currentRoom.North] = currentRoom;
                queue.Enqueue(currentRoom.North);
            }
            if (currentRoom.South != null && !visitedRooms.Contains(currentRoom.South))
            {
                roomParents[currentRoom.South] = currentRoom;
                queue.Enqueue(currentRoom.South);
            }
            if (currentRoom.East != null && !visitedRooms.Contains(currentRoom.East))
            {
                roomParents[currentRoom.East] = currentRoom;
                queue.Enqueue(currentRoom.East);
            }
            if (currentRoom.West != null && !visitedRooms.Contains(currentRoom.West))
            {
                roomParents[currentRoom.West] = currentRoom;
                queue.Enqueue(currentRoom.West);
            }
        }

        return pathCount;
    }
    public List<Room> SinglePathThrough(Room requiredRoom)
    {
        var rooms = _roomDao.GetAllEditableRooms();
        var entrance = _roomDao.GetEntrance();

        var singlePathRooms = new ConcurrentBag<Room>();

        Parallel.ForEach(rooms, room =>
        {
            int pathCount = CountPathsToEntranceThroughRoom(room, requiredRoom, entrance);
            if (pathCount == 1)
            {
                singlePathRooms.Add(room);
            }
        });

        return singlePathRooms.ToList();
    }
    public Dictionary<string, string[]> GetAvailableDirections(Room? editingRoom = null)
    {
        var availableRooms = new Dictionary<string, string[]>();
        var allRooms = _roomDao.GetAllRooms();
        var singlePathRooms = editingRoom == null ? null : SinglePathThrough(editingRoom);

        _outputManager.WriteLine();
        for (int i = 0; i < allRooms.Count; i++)
        {
            var room = allRooms[i];

            if (room == editingRoom) continue;
            if (editingRoom != null && singlePathRooms.Contains(room)) continue;

            string? availableDirections = "";

            if (room.North == null && !_roomDao.HasReverseConflict(room, "North")) availableDirections += "North ";
            if (room.South == null && !_roomDao.HasReverseConflict(room, "South")) availableDirections += "South ";
            if (room.East == null && !_roomDao.HasReverseConflict(room, "East")) availableDirections += "East ";
            if (room.West == null && !_roomDao.HasReverseConflict(room, "West")) availableDirections += "West ";

            if (string.IsNullOrEmpty(availableDirections)) continue;

            availableDirections = RemoveLogicalConflicts(room, availableDirections);

            if (availableDirections?.Length == 0 || string.IsNullOrEmpty(availableDirections)) continue;

            _outputManager.WriteLine($"{availableRooms.Count + 1}. {room.Name} - Available directions: {availableDirections}");

            availableRooms.Add(room.Name, availableDirections.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries));
        }

        return availableRooms;
    }
    private string RemoveLogicalConflicts(Room baseRoom, string availableDirections)
    {
        string finalDirections = string.Empty;

        var grid = GetRoomGrid();
        try
        {
            var baseCoords = FindRoomCoordinates(baseRoom, grid);

            var directions = availableDirections.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            var validDirections = new List<string>();

            foreach (var dir in directions)
            {
                var (dx, dy) = GetDirectionOffset(dir);

                var targetCoords = (baseCoords.x + dx, baseCoords.y + dy);

                if (!grid.ContainsKey(targetCoords))
                {
                    validDirections.Add(dir);
                }
            }

            if (validDirections.Count > 0)
            {
                finalDirections = string.Join(" ", validDirections);
            }
        }
        catch (InvalidOperationException)
        {
            finalDirections = string.Empty;
        }

        return finalDirections;
    }
    private (int dx, int dy) GetDirectionOffset(string direction)
    {
        return direction switch
        {
            "North" => (0, -1),
            "South" => (0, 1),
            "East" => (1, 0),
            "West" => (-1, 0),
            _ => (0, 0)
        };
    }
    public Dictionary<string, string> FindUnlinkedNeighbors()
    {
        Dictionary<string, string> unlinkedNeighbors = [];

        var rooms = _roomDao.GetAllRooms();
        var grid = GetRoomGrid();

        foreach (var room in rooms)
        {
            try
            {
                var (x, y) = FindRoomCoordinates(room, grid);

                string[] directions = { "North", "South", "East", "West" };

                foreach (var direction in directions)
                {
                    var (dx, dy) = GetDirectionOffset(direction);

                    // First, check if THIS room has a connection in that direction already
                    var neighbor = GetNeighbor(room, direction);
                    if (neighbor != null) continue; // already linked

                    // Next, check if there is a room at the target location
                    var targetCoords = (x + dx, y + dy);
                    if (!grid.ContainsKey(targetCoords)) continue; // no room there

                    var neighborRoom = grid[targetCoords];

                    // Check if neighborRoom already links back toward this room
                    var reverseDirection = GetReverseDirection(direction);
                    var reverseNeighbor = GetNeighbor(neighborRoom, reverseDirection);
                    if (reverseNeighbor != null) continue; // already linked

                    // Configure key
                    string key = $"{room.Name}-{direction}";

                    // Check if the reverse is already in dictionary
                    string reverseKey = $"{neighborRoom.Name}-{reverseDirection}";
                    if (unlinkedNeighbors.ContainsKey(reverseKey)) continue;

                    //Add unlinked neighbor to dictionary
                    unlinkedNeighbors[key] = neighborRoom.Name;
                }
            }
            catch (InvalidOperationException)
            {
                //room not on grid
                continue;
            }
        }

        return unlinkedNeighbors;
    }
    private Room? GetNeighbor(Room room, string direction)
    {
        return direction switch
        {
            "North" => room.North,
            "South" => room.South,
            "East" => room.East,
            "West" => room.West,
            _ => null
        };
    }
    private string GetReverseDirection(string direction)
    {
        return direction switch
        {
            "North" => "South",
            "South" => "North",
            "East" => "West",
            "West" => "East",
            _ => ""
        };
    }
}