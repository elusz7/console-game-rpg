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

        _outputManager.WriteLine("\nPress enter to return to the menu.");
        _outputManager.Display();
        _inputManager.ReadKey();
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
                    //UpdateRoom();
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

    private void AddRoom()
    {
        _outputManager.WriteLine("\nCreate New Room", ConsoleColor.Cyan);

        string name = "";
        while (true)
        {
            name = _inputManager.ReadString("Enter room name: ");
            if (rooms.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _outputManager.WriteLine("Room name already exists. Please choose a different name.", ConsoleColor.Red);
            }
            else
            {
                break;
            }
        }
        string description = _inputManager.ReadString("Enter room description: ");

        Room newRoom = new Room(name, description);

        // Add directional links
        AddDirectionalLink(newRoom, "North", r => r.North, (r, linkedRoom) => r.North = linkedRoom, (linkedRoom, r) => linkedRoom.South = r);
        AddDirectionalLink(newRoom, "South", r => r.South, (r, linkedRoom) => r.South = linkedRoom, (linkedRoom, r) => linkedRoom.North = r);
        AddDirectionalLink(newRoom, "East", r => r.East, (r, linkedRoom) => r.East = linkedRoom, (linkedRoom, r) => linkedRoom.West = r);
        AddDirectionalLink(newRoom, "West", r => r.West, (r, linkedRoom) => r.West = linkedRoom, (linkedRoom, r) => linkedRoom.East = r);

        // Add to memory and database
        rooms.Add(newRoom);
        try
        {
            _context.Rooms.Add(newRoom);
            _context.SaveChanges();
            _outputManager.WriteLine($"Room '{newRoom.Name}' created successfully!", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            _outputManager.WriteLine($"Failed to save the room to the database: {ex.Message}", ConsoleColor.Red);
        }
    }
    private void AddDirectionalLink(Room newRoom, string direction,
        Func<Room, Room?> getLink, Action<Room, Room> setLink, Action<Room, Room> setReverseLink)
    {
        string addDirection = _inputManager.ReadString($"Add {direction} Room? (y/n): ", new[] { "y", "n" });
        if (addDirection == "y")
        {
            Room linkedRoom = FindAvailableRoom(direction);
            if (linkedRoom != null)
            {
                setLink(newRoom, linkedRoom);
                setReverseLink(linkedRoom, newRoom);
            }
            else
            {
                _outputManager.WriteLine($"No available room to the {direction}.", ConsoleColor.Red);
            }
        }
    }

    private Room FindAvailableRoom(string direction)
    {
        List<Room> availableRooms = direction switch
        {
            "North" => rooms.Where(r => r.North == null && r.South == null).ToList(),
            "South" => rooms.Where(r => r.South == null && r.North == null).ToList(),
            "East" => rooms.Where(r => r.East == null && r.West == null).ToList(),
            "West" => rooms.Where(r => r.West == null && r.East == null).ToList(),
            _ => new List<Room>()
        };

        return availableRooms.Count > 0 ? SelectARoom(availableRooms) : null;
    }
}