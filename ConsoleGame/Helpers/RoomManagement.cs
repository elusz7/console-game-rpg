using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Rooms;

namespace ConsoleGame.Helpers;

/// <summary>
/// Handles creation, editing, and deletion of rooms.
/// </summary>
public class RoomManagement
{
    private readonly InputManager _inputManager;
    private readonly OutputManager _outputManager;
    private readonly RoomDao _roomDao;
    private readonly MapManager _mapManager;

    public RoomManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao, MapManager mapManager)
    {
        _inputManager = inputManager;
        _outputManager = outputManager;
        _roomDao = roomDao;
        _mapManager = mapManager;
    }
    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("Room Management Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Add Room"
                + "\n2. Edit Room Description"
                + "\n3. Delete Room"
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
                    DeleteRoom();
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
        Dictionary<string, string[]> availableRooms = _mapManager.GetAvailableDirections();

        int index = _inputManager.ReadInt("\tSelect a room to add onto by number: ", availableRooms.Count);

        Room roomToAddOnto = _roomDao.FindRoomByName(availableRooms.ElementAt(index - 1).Key);
        
        string[] directions = availableRooms[roomToAddOnto.Name];

        string directionToAddOnto = directions[0];

        if (directions.Length > 1)
        {
            _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);
            directionToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", directions)}): ", directions);
        }

        _outputManager.WriteLine($"\nAdding new room to the {directionToAddOnto} of {roomToAddOnto.Name}", ConsoleColor.Green);

        Room newRoom = CreateRoom();

        LinkRooms(roomToAddOnto, newRoom, directionToAddOnto);

        _roomDao.AddRoom(newRoom);
        _roomDao.UpdateRoom(roomToAddOnto);

        _outputManager.WriteLine($"\nRoom [{newRoom.Name}] succesfully created!\n", ConsoleColor.Green);
    }
    private Room CreateRoom()
    {
        // Create the new room
        string name;
        while (true)
        {
            _outputManager.WriteLine("\nnote: Room Name Cannot Be Changed After Creation", ConsoleColor.Yellow);
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
    private void EditRoomDescription()
    {
        _outputManager.Write("\nnote: Entrance cannot be edited", ConsoleColor.Yellow);

        Room roomToEdit = SelectARoom("Select A Room To Edit Description: ");

        if (roomToEdit == null)
        {
            _outputManager.WriteLine("Room editing cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nOld Description: {roomToEdit.Description}");
        string newDescription = _inputManager.ReadString($"Enter new description for {roomToEdit.Name}: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm new description of {roomToEdit.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"Editing cancelled. Room [{roomToEdit.Name}] has not been edited.", ConsoleColor.Red);
            return;
        }

        roomToEdit.Description = newDescription;
        _roomDao.UpdateRoom(roomToEdit);

        _outputManager.WriteLine($"{roomToEdit.Name}'s description updated successfully!\n", ConsoleColor.Green);
    }
    private Room SelectARoom(string prompt)
    {
        List<Room> rooms = _roomDao.GetAllEditableRooms();

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
    private void DeleteRoom()
    {
        _outputManager.WriteLine("\nnote: Entrance cannot be removed\n", ConsoleColor.Yellow);
        
        Room roomToRemove = SelectARoom("Select A Room To Remove (-1 to cancel): ");
        if (roomToRemove == null)
        {
            _outputManager.WriteLine("\nRoom deletion cancelled.\n", ConsoleColor.Red);
            return;
        }

        string confirm = _inputManager.ReadString($"\nPlease confirm deletion of {roomToRemove.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"\nDeletion cancelled. Room [{roomToRemove.Name}] has not been removed.\n", ConsoleColor.Red);
            return;
        }

        _roomDao.DeleteRoom(roomToRemove);

        _mapManager.DirtyGrid = true;
        _mapManager.CheckForDisconnectedRooms();

        _outputManager.WriteLine("\nRoom has been successfully deleted!\n", ConsoleColor.Green);
    }
}
