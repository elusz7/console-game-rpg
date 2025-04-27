using System.Net.Sockets;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            string menuPrompt = "1. View Rooms"
                + "\n2. View Map"
                + "\n3. Manage Rooms"
                + "\n4. Update Room Connections"
                + "\n5. Return to Main Menu"
                + "\n\tSelect an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    _roomDisplay.Menu();
                    break;
                case "2":
                    _mapManager.TraverseMap();
                    break;
                case "3":
                    _roomManagement.Menu();
                    break;
                case "4":
                    _roomConnectionManagement.Menu();
                    break;
                case "5":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    RoomMainMenu();
                    break;
            }
        }
    } 
    /*
    private void AddConnectionBetweenRooms()
    {
        bool hasUnlinkedRooms = true;
        bool hasUnconnectedRooms = true;

        //Find rooms that are next to each other but not connected
        Dictionary<string, string> unlinkedNextDoorRooms = FindUnlinkedRooms();
        //Find rooms that don't have a path to the entrance
        List<Room> unconnectedRooms = FindRoomsNotConnectedToEntrance();

        _outputManager.WriteLine();

        while (true)
        {
            if (unlinkedNextDoorRooms.Count == 0)
            {
                hasUnlinkedRooms = false;
                _outputManager.WriteLine("No unlinked rooms found.", ConsoleColor.Green);
            }

            if (unconnectedRooms.Count == 0)
            {
                hasUnconnectedRooms = false;
                _outputManager.WriteLine("All rooms are connected to the entrance.", ConsoleColor.Green);
            }

            if (!hasUnlinkedRooms && !hasUnconnectedRooms)
            {
                _outputManager.WriteLine("No connections can be added between rooms.", ConsoleColor.Green);
                return;
            }
            else if (hasUnlinkedRooms && !hasUnconnectedRooms)
            {


                ChooseUnlinkedRoom(unlinkedNextDoorRooms);
            }
            else if (!hasUnlinkedRooms && hasUnconnectedRooms)
            {

                ChooseUnconnectedRoom(unconnectedRooms);
            }
            else
            {
                string prompt = "\n1. Update Unlinked Room\n2. Update UnconnectedRoom\n\tChoose an option: ";

                int choice = _inputManager.ReadInt(prompt, 2);

                if (choice == 1)
                    ChooseUnlinkedRoom(unlinkedNextDoorRooms);
                else
                    ChooseUnconnectedRoom(unconnectedRooms);
            }

            string input = _inputManager.ReadString("\nUpdate another? (y/n) ", new[] { "y", "n" });

            if (input == "n")
            {
                _outputManager.WriteLine("Exiting update room connection");
                return;
            }
        }

    }
    private void ChooseUnlinkedRoom(Dictionary<string, string> unlinkedNextDoorRooms)
    {
        _outputManager.WriteLine("\nUnlinked Rooms: ", ConsoleColor.Cyan);
        for (int i = 0; i < unlinkedNextDoorRooms.Count; i++)
        {
            string key = unlinkedNextDoorRooms.ElementAt(i).Key;
            string value = unlinkedNextDoorRooms.ElementAt(i).Value;

            _outputManager.WriteLine($"{i + 1}. {key} at {value}-{GetOppositeDirection(value)}");
        }

        int index = _inputManager.ReadInt("\tChoose which unlinked room to update: ", unlinkedNextDoorRooms.Count);

        string keyToRemove = unlinkedNextDoorRooms.ElementAt(index - 1).Key;
        Room[] roomArr = keyToRemove.Split('-').Select(r => rooms.First(room => room.Name == r)).ToArray();

        string direction = unlinkedNextDoorRooms.ElementAt(index - 1).Value;

        UpdateUnlinkedRooms(roomArr, direction);

        unlinkedNextDoorRooms.Remove(keyToRemove);
    }
    private void ChooseUnconnectedRoom(List<Room> unconnectedRooms)
    {
        _outputManager.WriteLine("\nUnconnected Rooms: ", ConsoleColor.Cyan);
        for (int i = 0; i < unconnectedRooms.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {unconnectedRooms[i].Name}");
        }
        int index = _inputManager.ReadInt("\tChoose which unconnected room to update: ", unconnectedRooms.Count);
        Room unconnectedRoom = unconnectedRooms[index - 1];

        Dictionary<string, string[]> availableRooms = GetAvailableDirections();

        index = _inputManager.ReadInt($"\tSelect a room to add {unconnectedRoom.Name} onto by number: ", availableRooms.Count);
        Room connectedRoom = rooms.Where(r => r.Name == availableRooms.ElementAt(index - 1).Key).First();

        string[] availableDirections = availableRooms[connectedRoom.Name];
        _outputManager.WriteLine($"\nYou selected {connectedRoom.Name}. Available directions: {string.Join(", ", availableDirections)}", ConsoleColor.Green);

        string directionToAddOnto = availableDirections[0];
        if (availableDirections.Length > 1)
        {
            directionToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", availableDirections)}): ", availableDirections);
        }

        _outputManager.WriteLine($"\nAdding {unconnectedRoom.Name} to the {directionToAddOnto} of {connectedRoom.Name}.", ConsoleColor.Green);
        LinkRooms(unconnectedRoom, connectedRoom, directionToAddOnto);

        unconnectedRooms.Remove(unconnectedRoom);

        _context.SaveChanges();
    }
    private void UpdateUnlinkedRooms(Room[] unlinkedRooms, string direction)
    {
        _outputManager.WriteLine($"\nAdding connection between {unlinkedRooms[0].Name} and {unlinkedRooms[1].Name} to the {direction}.", ConsoleColor.Green);

        //Entrance-Tunnel North tunnel.North = entrance
        //study-hallway East study.east = hallway

        if (direction.Equals("North"))
            LinkRooms(unlinkedRooms[0], unlinkedRooms[1], direction);
        else
            LinkRooms(unlinkedRooms[1], unlinkedRooms[0], direction);

        _context.SaveChanges();
    }
    private Dictionary<string, string> FindUnlinkedRooms()
    {
        Dictionary<string, string> unlinkedNextDoorRooms = [];

        foreach (var room in rooms)
        {
            var grid = GetRoomGrid(room);
            var (x, y) = FindRoomCoordinates(grid, room);

            var directions = new Dictionary<string, (int dx, int dy)>
        {
            { "North", (0, -1) },
            { "South", (0, 1) },
            { "East", (1, 0) },
            { "West", (-1, 0) }
        };

            foreach (var dir in directions)
            {
                int nx = x + dir.Value.dx;
                int ny = y + dir.Value.dy;

                if (grid.TryGetValue((nx, ny), out Room neighbor))
                {
                    bool alreadyLinked =
                        (room.North == neighbor && neighbor.South == room) ||
                        (room.South == neighbor && neighbor.North == room) ||
                        (room.East == neighbor && neighbor.West == room) ||
                        (room.West == neighbor && neighbor.East == room);

                    if (alreadyLinked)
                        continue;

                    string key, value;

                    if (room.Name.CompareTo(neighbor.Name) < 0)
                    {
                        key = $"{room.Name}-{neighbor.Name}";
                        value = dir.Key;
                    }
                    else
                    {
                        key = $"{neighbor.Name}-{room.Name}";
                        value = GetOppositeDirection(dir.Key);
                    }

                    if (!unlinkedNextDoorRooms.ContainsKey(key))
                        unlinkedNextDoorRooms[key] = value;
                }
            }
        }

        return unlinkedNextDoorRooms;
    }
    private string GetOppositeDirection(string dir) => dir switch
    {
        "North" => "South",
        "South" => "North",
        "East" => "West",
        "West" => "East",
        _ => throw new ArgumentException("Invalid direction", nameof(dir))
    };
    private List<Room> FindRoomsNotConnectedToEntrance()
    {
        var entrance = rooms.FirstOrDefault(r => r.Name == "Entrance");
        if (entrance == null) return rooms.ToList();

        var visited = new HashSet<Room>();
        var queue = new Queue<Room>([entrance]);
        visited.Add(entrance);

        while (queue.TryDequeue(out var current))
            foreach (var neighbor in new[] { current.North, current.South, current.East, current.West }.Where(n => n != null && visited.Add(n)))
                queue.Enqueue(neighbor);

        return rooms.Where(r => !visited.Contains(r)).ToList();
    }
    private void RemoveLinksInUnconnectedRooms()
    {
        Room entrance = rooms.First(r => r.Name == "Entrance");
        var unreachableRooms = new List<Room>();
        var visited = new HashSet<Room>();
        var queue = new Queue<Room>();

        queue.Enqueue(entrance);
        visited.Add(entrance);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            var neighbors = new List<Room> { current.North, current.South, current.East, current.West };

            foreach (var neighbor in neighbors)
            {
                if (neighbor != null && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        // After visiting, any room not in 'visited' is unreachable
        foreach (var room in rooms)
        {
            if (!visited.Contains(room))
            {
                UnlinkRoom(room); //make sure room isn't connected to any other rooms
                unreachableRooms.Add(room);
            }
        }
    }
    private void DeleteRoom()
    {
        _outputManager.Write("\nnote: Entrance cannot be deleted", ConsoleColor.Yellow);
        Room roomToDelete = SelectARoom("\tSelect A Room To Delete: ", rooms.Where(r => r.Name != "Entrance").ToList());
        string confirm = _inputManager.ReadString($"\nAre you sure you want to delete {roomToDelete.Name}? (y/n): ", new[] { "y", "n" });
        if (confirm == "n")
        {
            _outputManager.WriteLine("Room deletion cancelled.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nDeleting {roomToDelete.Name}...", ConsoleColor.Red);

        UnlinkRoom(roomToDelete);
        RemoveLinksInUnconnectedRooms();
        rooms.Remove(roomToDelete);
        _context.Rooms.Remove(roomToDelete);
        _context.SaveChanges();

        _outputManager.WriteLine($"Room deleted successfully.", ConsoleColor.Green);
    }
    */
}


