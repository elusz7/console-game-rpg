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
    private Room SelectARoom(string prompt, List<Room>? roomList = null)
    {
        int index;
        Room returnRoom;

        _outputManager.WriteLine();
        if (roomList == null)
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {rooms[i].Name}");
            }
            index = _inputManager.ReadInt($"\n\t{prompt}", rooms.Count);
            returnRoom = rooms[index - 1];
        }
        else
        {
            for (int i = 0; i < roomList.Count; i++)
            {
                _outputManager.WriteLine($"{i + 1}. {roomList[i].Name}");
            }
            index = _inputManager.ReadInt(prompt, roomList.Count);
            returnRoom = roomList[index - 1];
        }

        return returnRoom;
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
    private void AddRoom()
    {
        Dictionary<string, string[]> availableRooms = GetAvailableDirections();

        int index = _inputManager.ReadInt("Select a room to add onto by number: ", availableRooms.Count);

        Room roomToAddOnto = rooms.Where(r => r.Name == availableRooms.ElementAt(index - 1).Key).First();
        string[] directions = availableRooms[roomToAddOnto.Name];
        _outputManager.WriteLine($"You selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);

        string directionToAddOnto = _inputManager.ReadString($"\nEnter direction to add room ({string.Join("/", directions)}): ", directions);

        Room newRoom = CreateRoom();

        LinkRooms(roomToAddOnto, newRoom, directionToAddOnto);

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
            _outputManager.WriteLine("\n\tnote: Room Name Cannot Be Changed After Creation", ConsoleColor.Yellow);
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
    private Dictionary<string, string[]> GetAvailableDirections(Room? editingRoom = null)
    {
        Dictionary<string, string[]> availableRooms = [];

        _outputManager.WriteLine();
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];

            if (room == editingRoom) continue;

            string availableDirections = "";
            
            if (room.North == null && !HasReverseConflict(room, "North")) availableDirections += "North ";
            if (room.South == null && !HasReverseConflict(room, "South")) availableDirections += "South ";
            if (room.East == null && !HasReverseConflict(room, "East")) availableDirections += "East ";
            if (room.West == null && !HasReverseConflict(room, "West")) availableDirections += "West ";

            if (string.IsNullOrEmpty(availableDirections)) continue;

            availableDirections = RemoveLogicalConflicts(availableDirections, room);

            if (availableDirections.Length == 0) continue;

            _outputManager.WriteLine($"{availableRooms.Count + 1}. {room.Name} - Available directions: {availableDirections}");

            availableRooms.Add(room.Name, availableDirections.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries));
        }

        return availableRooms;
    }
    private string RemoveLogicalConflicts(string availableDirections, Room room)
    {
        string[] directions = availableDirections.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (directions.Contains("North"))
        {
            if (room.East != null || room.West != null)
            {
                if (room.East?.North != null || room.West?.North != null)
                {
                    if (room.East?.North?.West != null || room.West?.North?.East != null)
                    {
                        availableDirections = availableDirections.Replace("North", "");
                    }
                }
            }
        }

        if (directions.Contains("South"))
        {
            if (room.East != null || room.West != null)
            {
                if (room.East?.South != null || room.West?.South != null)
                {
                    if (room.East?.South?.West != null || room.West?.South?.East != null)
                    {
                        availableDirections = availableDirections.Replace("South", "");
                    }
                }
            }
        }

        if (directions.Contains("East"))
        {
            if (room.North != null || room.South != null)
            {
                if (room.North?.East != null || room.South?.East != null)
                {
                    if (room.North?.East?.South != null || room.South?.East?.North != null)
                    {
                        availableDirections = availableDirections.Replace("East", "");
                    }
                }
            }
        }

        if (directions.Contains("West"))
        {
            if (room.North != null || room.South != null)
            {
                if (room.North?.West != null || room.South?.West != null)
                {
                    if (room.North?.West?.South != null || room.South?.West?.North != null)
                    {
                        availableDirections = availableDirections.Replace("West", "");
                    }
                }
            }
        }

        return availableDirections.Trim();
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
    private void LinkRooms(Room from, Room to, string direction)
    {
        switch (direction)
        {
            case "North":
                from.North = to;
                to.South = from;
                break;
            case "South":
                from.South = to;
                to.North = from;
                break;
            case "East":
                from.East = to;
                to.West = from;
                break;
            case "West":
                from.West = to;
                to.East = from;
                break;
        }
    }
    private void UpdateRoom()
    {
        while (true)
        {
            _outputManager.WriteLine("\nRoom Editing", ConsoleColor.Cyan);
            string menuPrompt = "1. Change Room Description"
                + "\n2. Change Room Placement"
                + "\n3. Add Connection Between Rooms"
                + "\n4. Remove Connection Between Rooms"
                + "\n5. Return to Room Main Menu"
                + "\n\tSelect an option: ";
            string input = _inputManager.ReadString(menuPrompt);

            switch (input)
            {
                case "1":
                    EditRoomDescription();
                    break;
                case "2":
                    ChangeRoomPlacement();
                    break;
                case "3":
                    //AddConnectionBetweenRooms();
                    break;
                case "4":
                    //RemoveConnectionBetweenRooms();
                    break;
                case "5":
                    _outputManager.WriteLine();
                    return;
                default:
                    _outputManager.WriteLine("Invalid option. Please try again.", ConsoleColor.Red);
                    break;
            }
        }
    }
    private void EditRoomDescription()
    {
        _outputManager.WriteLine("\nnote: Entrance cannot be edited", ConsoleColor.Yellow);

        Room roomToEdit = SelectARoom("Select A Room To Edit Description: ", rooms.Where(r => r.Name != "Entrance").ToList());

        string newDescription = _inputManager.ReadString($"\nEnter new description for {roomToEdit.Name}: ");
        
        roomToEdit.Description = newDescription;

        _context.SaveChanges();
    }
    private void ChangeRoomPlacement()
    {
        Room roomToChange = SelectARoom("Select a room to move: ");

        Dictionary<string, string[]> availableRooms = GetAvailableDirections(roomToChange);

        int index = _inputManager.ReadInt($"Enter number of room where {roomToChange.Name} will go: ", availableRooms.Count);

        Room roomToAddOnto = rooms.Where(r => r.Name == availableRooms.ElementAt(index - 1).Key).First();
        string[] directions = availableRooms[roomToAddOnto.Name];

        string directionToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", directions)}): ", directions);

        string confirm = _inputManager.ReadString($"\nAre you sure you want to move {roomToChange.Name} to the {directionToAddOnto} of {roomToAddOnto.Name}? (y/n): ", new[] { "y", "n" });

        if (confirm == "n")
        {
            _outputManager.WriteLine("Room change placement cancelled.", ConsoleColor.Red);
            return;
        }

        UnlinkRoom(roomToChange);

        LinkRooms(roomToAddOnto, roomToChange, directionToAddOnto);

        _context.SaveChanges();

    }
    private void UnlinkRoom(Room room)
    {
        if (room.North != null)
        {
            room.North.South = null;
            room.North = null;
        }
        if (room.South != null)
        {
            room.South.North = null;
            room.South = null;
        }
        if (room.East != null)
        { 
            room.East.West = null;
            room.East = null;
        }
        if (room.West != null)
        {
            room.West.East = null;
            room.West = null;
        }
    }

}