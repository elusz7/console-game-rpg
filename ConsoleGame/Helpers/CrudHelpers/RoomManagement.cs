using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers.CrudHelpers;

public class RoomManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly RoomDao _roomDao = roomDao;

    public void Menu()
    {        
        while (true)
        {
            _outputManager.Clear();

            _outputManager.WriteLine("Room Management Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Add Room"
                + "\n2. Edit Room Description"
                + "\n3. Delete Room"
                + "\n4. Return to Admin Menu");

            var choice = _inputManager.ReadMenuKey(4);

            switch (choice)
            {
                case 1:
                    AddRoom();
                    break;
                case 2:
                    EditRoomDescription();
                    break;
                case 3:
                    DeleteRoom();
                    break;
                case 4:
                    _outputManager.Clear();
                    return;
            }
        }
    }
    private void AddRoom()
    {
        Dictionary<string, string[]> availableRooms = MapHelper.GetAvailableDirections(_roomDao.GetAllRooms());

        if (availableRooms.Count == 0)
        {
            _outputManager.WriteLine("No available rooms to add onto. Please create a room first.", ConsoleColor.Red);
            return;
        }

        for (int i = 0; i < availableRooms.Count; i++)
        {
            var entry = availableRooms.ElementAt(i);
            _outputManager.WriteLine($"[{i + 1}] {entry.Key} ({string.Join(", ", entry.Value)})");
        }

        int index = _inputManager.ReadInt("\tSelect a room to add onto by number: ", availableRooms.Count);

        Room? roomToAddOnto = _roomDao.FindRoomByName(availableRooms.ElementAt(index - 1).Key);

        if (roomToAddOnto == null)
        {
            _outputManager.WriteLine("Invalid room selection. Please try again.", ConsoleColor.Red);
            //if you're getting this error something went very wrong
            return;
        }

        string[] directions = availableRooms[roomToAddOnto.Name];

        string directionToAddOnto = directions[0];

        if (directions.Length > 1)
        {
            _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);
            directionToAddOnto = _inputManager.ReadString($"Enter direction to add room ({string.Join("/", directions)}): ", directions);
        }

        _outputManager.WriteLine($"\nAdding new room to the {directionToAddOnto} of {roomToAddOnto.Name}", ConsoleColor.Green);

        Room newRoom = CreateRoom();

        MapHelper.LinkRooms(roomToAddOnto, newRoom, directionToAddOnto);

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
    private void EditRoomDescription()
    {
        _outputManager.Write("\nnote: Entrance cannot be edited", ConsoleColor.Yellow);

        Room? roomToEdit = _inputManager.PaginateList(_roomDao.GetAllEditableRooms(), "room", "edit description of", true);

        if (roomToEdit == null)
        {
            _outputManager.WriteLine("Room editing cancelled.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nOld Description: {roomToEdit.Description}");
        string newDescription = _inputManager.ReadString($"Enter new description for {roomToEdit.Name}: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm new description of {roomToEdit.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"Editing cancelled. Room [{roomToEdit.Name}] has not been edited.\n", ConsoleColor.Red);
            return;
        }

        roomToEdit.Description = newDescription;
        _roomDao.UpdateRoom(roomToEdit);

        _outputManager.WriteLine($"{roomToEdit.Name}'s description updated successfully!\n", ConsoleColor.Green);
    }
    private void DeleteRoom()
    {
        _outputManager.WriteLine("\nnote: Entrance cannot be removed\n", ConsoleColor.Yellow);

        var rooms = _roomDao.GetAllDeletableRooms();

        if (rooms.Count == 0)
        {
            _outputManager.WriteLine("No rooms available for deletion.\n", ConsoleColor.Red);
            return;
        }
        
        Room? roomToRemove = _inputManager.PaginateList(rooms, "room", "remove", true);

        if (roomToRemove == null)
        {
            _outputManager.WriteLine("Room deletion cancelled.\n", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction("deletion"))
        {
            _outputManager.WriteLine($"Deletion cancelled. Room [{roomToRemove.Name}] has not been removed.\n", ConsoleColor.Red);
            return;
        }

        _roomDao.DeleteRoom(roomToRemove);

        var disconnectedRooms = MapHelper.CheckForDisconnectedRooms(_roomDao.GetAllRooms());
        if (disconnectedRooms.Count > 0)
        {
            _outputManager.WriteLine($"\nWarning: The following rooms are now disconnected: {string.Join(", ", disconnectedRooms.Select(r => r.Name))}", ConsoleColor.Yellow);
            _outputManager.WriteLine("Please check the map for any issues.", ConsoleColor.DarkYellow);
            _roomDao.UpdateAllRooms(disconnectedRooms);
        }        

        _outputManager.WriteLine("\nRoom has been successfully deleted!\n", ConsoleColor.Green);
    }
}