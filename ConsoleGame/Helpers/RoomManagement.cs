using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Rooms;

namespace ConsoleGame.Helpers;

public class RoomManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao, MapManager mapManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly RoomDao _roomDao = roomDao;
    private readonly MapManager _mapManager = mapManager;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("\nRoom Management Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Room"
                + "\n2. Edit Room Description"
                + "\n3. Remove Room"
                + "\n4. Return to Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);
            switch (choice)
            {
                case "1":
                    AddRoom();
                    break;
                case "2":
                    EditRoomDescription();
                    break;
                case "3":
                    RemoveRoom();
                    break;
                case "4":
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.Write("Invalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void AddRoom()
    {
        Dictionary<string, string[]> availableRooms = GetAvailableDirections();

        int index = _inputManager.ReadInt("\tSelect a room to add onto by number: ", availableRooms.Count);

        Room roomToAddOnto = _roomDao.FindRoomByName(availableRooms.ElementAt(index - 1).Key);
        
        string[] directions = availableRooms[roomToAddOnto.Name];

        string directionToAddOnto = directions[0];

        if (directions.Length > 1)
        {
            _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);
            directionToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", directions)}): ", directions);
        }

        _outputManager.WriteLine($"Adding new room to the {directionToAddOnto} of {roomToAddOnto.Name}", ConsoleColor.Green);

        Room newRoom = CreateRoom();

        LinkRooms(roomToAddOnto, newRoom, directionToAddOnto);

        _roomDao.AddRoom(newRoom);
        _roomDao.UpdateRoom(roomToAddOnto);
    }
    private Dictionary<string, string[]> GetAvailableDirections(Room? editingRoom = null)
    {
        var availableRooms = new Dictionary<string, string[]>();
        var allRooms = _roomDao.GetAllRooms();
        //var unconnectedRooms = FindRoomsNotConnectedToEntrance();

        _outputManager.WriteLine();
        for (int i = 0; i < allRooms.Count; i++)
        {
            var room = allRooms[i];

            if (room == editingRoom) continue;
            //if (unconnectedRooms.Contains(room)) continue;

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
    private Room CreateRoom()
    {
        // Create the new room
        string name;
        while (true)
        {
            _outputManager.WriteLine("\n\tnote: Room Name Cannot Be Changed After Creation", ConsoleColor.Yellow);
            name = _inputManager.ReadString("Enter name for the new room: ");
            if (_roomDao.RoomExists(name))
            {
                _outputManager.WriteLine("Room name already exists. Please choose a different name.", ConsoleColor.Red);
            }
            else break;
        }

        string description = _inputManager.ReadString("Enter description: ");
        return new Room(name, description);
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
    private string RemoveLogicalConflicts(Room baseRoom, string availableDirections)
    {
        string finalDirections = null;

        var grid = _mapManager.GetRoomGrid();
        try
        {
            var baseCoords = _mapManager.FindRoomCoordinates(baseRoom, grid);

            if (!(baseCoords == (-100, -100)))
            {
                var directions = availableDirections.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                var result = new List<string>();

                foreach (var dir in directions)
                {
                    var (dx, dy) = dir switch
                    {
                        "North" => (0, 1),
                        "South" => (0, -1),
                        "East" => (1, 0),
                        "West" => (-1, 0),
                        _ => (0, 0)
                    };

                    var targetCoords = (baseCoords.x + dx, baseCoords.y + dy);
                    if (!grid.ContainsKey(targetCoords)) // only allow direction if no room exists at target
                    {
                        result.Add(dir);
                    }
                }
                if (!(result.Count == 0))
                {
                    finalDirections = string.Join(" ", result);
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            //room not found in grid
            finalDirections = null;
        }
        return finalDirections;
    }
    private void EditRoomDescription()
    {
        _outputManager.WriteLine("\nnote: Entrance cannot be edited", ConsoleColor.Yellow);

        Room roomToEdit = SelectARoom("Select A Room To Edit Description: ");

        if (roomToEdit == null)
        {
            _outputManager.WriteLine("Room editing cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

        string newDescription = _inputManager.ReadString($"\nEnter new description for {roomToEdit.Name}: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm editing of {roomToEdit.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"Editing cancelled. Room [{roomToEdit.Name}] has not been edited.", ConsoleColor.Red);
            return;
        }

        roomToEdit.Description = newDescription;
        _roomDao.UpdateRoom(roomToEdit);
    }
    private Room SelectARoom(string prompt)
    {
        List<Room> rooms = _roomDao.GetAllEditableRooms();

        if (prompt.Contains("Remove"))
            rooms = _roomDao.GetAllDeletableRooms();

        _outputManager.WriteLine();
        
        for (int i = 0; i < rooms.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {rooms[i].Name}");
        }

        int index = _inputManager.ReadInt($"\t{prompt}", rooms.Count, true);

        Room selectedRoom = null;

        if (index != -1)
            selectedRoom = rooms[index - 1];
        
        return selectedRoom; 
    }
    private void RemoveRoom()
    {
        _outputManager.WriteLine("\nnote: Entrance cannot be removed", ConsoleColor.Yellow);
        _outputManager.WriteLine("\nnote: Only rooms with 0 or 1 connections can be removed", ConsoleColor.Yellow);
        
        Room roomToRemove = SelectARoom("Select A Room To Remove (-1 to cancel): ");
        if (roomToRemove == null)
        {
            _outputManager.WriteLine("Room deletion cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {roomToRemove.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"Deletion cancelled. Room [{roomToRemove.Name}] has not been removed.", ConsoleColor.Red);
            return;
        }

        _roomDao.DeleteRoom(roomToRemove);
    }
}
