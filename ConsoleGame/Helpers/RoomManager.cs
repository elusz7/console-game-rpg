using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Rooms;

namespace ConsoleGame.Helpers;

public class RoomManager(GameContext context, InputManager inputManager, OutputManager outputManager, MapManager mapManager, MonsterManager monsterManager)
{
    private readonly GameContext _context = context;
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly MapManager _mapManager = mapManager;
    private readonly MonsterManager _monsterManager = monsterManager;
    //set default current room to entrance
    private Room currentRoom;
    private List<Room> rooms;

    public void RoomMainMenu()
    {
        rooms = _context.Rooms.ToList();
        currentRoom = rooms.FirstOrDefault(r => r.Name == "Entrance");

        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Room Main Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. View Rooms"
                + "\n2. View Map"
                + "\n3. Manage Rooms"
                + "\n4. Return to Main Menu"
                + "\n\tSelect an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    ViewRooms();
                    break;
                case "2":
                    ViewMap();
                    break;
                case "3":
                    ManageRooms();
                    break;
                case "4":
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    RoomMainMenu();
                    break;
            }
        }
    }
    private void ViewRooms()
    {
        while (true)
        {
            _outputManager.WriteLine("\nRoom Display", ConsoleColor.Cyan);
            string menuPrompt = "1. View All Rooms"
                + "\n2. Return to Room Main Menu"
                + "\n\tSelect an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    ListRooms(rooms);
                    break;
                case "2":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
    private void ListRooms(List<Room> roomList)
    {
        _outputManager.WriteLine();
        foreach (var room in roomList)
        {
            _outputManager.WriteLine(room.ToString());
        }
    }
    private Room SelectARoom(List<Room> roomList)
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {roomList[i].Name}");
        }
        int index = _inputManager.ReadInt("Select a room by number: ", roomList.Count);
        return roomList[index - 1];
    }
    private void ViewMap()
    {
        _outputManager.Clear();
        _mapManager.UpdateCurrentRoom(currentRoom);
        _mapManager.DisplayMap();

        _outputManager.WriteLine("\nPress direction keys to change room or any other key to return to the menu.");
        _outputManager.Display();

        while (true)
        {
            
            ConsoleKeyInfo key = _inputManager.ReadKey();

            if (key.Key == ConsoleKey.UpArrow)
            {
                if (currentRoom.North != null)
                {
                    currentRoom = currentRoom.North;
                    _outputManager.Clear();
                    _mapManager.UpdateCurrentRoom(currentRoom);
                    _mapManager.DisplayMap();
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
                if (currentRoom.South != null)
                {
                    currentRoom = currentRoom.South;
                    _outputManager.Clear();
                    _mapManager.UpdateCurrentRoom(currentRoom);
                    _mapManager.DisplayMap();
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
                if (currentRoom.West != null)
                {
                    currentRoom = currentRoom.West;
                    _outputManager.Clear();
                    _mapManager.UpdateCurrentRoom(currentRoom);
                    _mapManager.DisplayMap();
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
                if (currentRoom.East != null)
                {
                    currentRoom = currentRoom.East;
                    _outputManager.Clear();
                    _mapManager.UpdateCurrentRoom(currentRoom);
                    _mapManager.DisplayMap();
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
    private void ManageRooms()
    {
        while (true)
        {
            _outputManager.WriteLine("\nRoom Management", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Room"
                + "\n2. Update Room"
                + "\n3. Delete Room"
                + "\n4. Return to Room Main Menu"
                + "\n\tSelect an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    AddRoom();
                    break;
                case "2":
                    UpdateRoom();
                    break;
                case "3":
                    //DeleteRoom();
                    break;
                case "4":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
    public void AddRoom()
    {
        Dictionary<string, string[]> availableRooms = [];

        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            string availableDirections = "";
            if (room.North == null && !HasReverseConflict(room, "North")) availableDirections += "North ";
            if (room.South == null && !HasReverseConflict(room, "South")) availableDirections += "South ";
            if (room.East == null && !HasReverseConflict(room, "East")) availableDirections += "East ";
            if (room.West == null && !HasReverseConflict(room, "West")) availableDirections += "West ";

            if (string.IsNullOrEmpty(availableDirections)) continue;

            _outputManager.WriteLine($"{availableRooms.Count + 1}. {room.Name} - Available directions: {availableDirections}");

            availableRooms.Add(room.Name, availableDirections.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries));
        }

        int index = _inputManager.ReadInt("Select a room to add onto by number: ", availableRooms.Count);

        Room roomToAddOnto = rooms.Where(r => r.Name == availableRooms.ElementAt(index - 1).Key).First();
        string[] directions = availableRooms[roomToAddOnto.Name];
        _outputManager.WriteLine($"You selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);

        string directToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", directions)}): ", directions);

        Room newRoom = CreateRoom();

        switch (directToAddOnto)
        {
            case "North":
                roomToAddOnto.North = newRoom;
                newRoom.South = roomToAddOnto;
                break;
            case "South":
                roomToAddOnto.South = newRoom;
                newRoom.North = roomToAddOnto;
                break;
            case "East":
                roomToAddOnto.East = newRoom;
                newRoom.West = roomToAddOnto;
                break;
            case "West":
                roomToAddOnto.West = newRoom;
                newRoom.East = roomToAddOnto;
                break;
        }

        rooms.Add(newRoom);
        // Save the new room to the database
        _context.Rooms.Add(newRoom);
        _context.SaveChanges();
    }
    private Room CreateRoom()
    {
        // Create the new room
        string name;
        while (true)
        {
            name = _inputManager.ReadString("Enter name for the new room: ");
            if (rooms.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _outputManager.WriteLine("Room name already exists. Please choose a different name.", ConsoleColor.Red);
            }
            else break;
        }

        string description = _inputManager.ReadString("Enter description: ");
        return new Room(name, description);
    }
    private bool HasReverseConflict(Room baseRoom, string direction)
    {
        return rooms.Any(r => direction switch
        {
            "North" => r.South == baseRoom,
            "South" => r.North == baseRoom,
            "East" => r.West == baseRoom,
            "West" => r.East == baseRoom,
            _ => false
        });
    }

    private void UpdateRoom()
    {
        while (true)
        {
            
        }
    }
}