using ConsoleGame.Managers.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.DisplayHelpers;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers.CrudHelpers;

public class RoomManagement(IInputManager inputManager, IOutputManager outputManager, 
            IRoomDao roomDao, IMapHelper mapHelper)
{
    private readonly IInputManager _inputManager = inputManager;
    private readonly IOutputManager _outputManager = outputManager;
    private readonly IRoomDao _roomDao = roomDao;
    private readonly IMapHelper _mapHelper = mapHelper;

    public void Menu()
    {
        _outputManager.Clear();

        while (true)
        {
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
        var availableRooms = _mapHelper.GetAvailableDirections2(_roomDao.GetAllRooms());

        if (availableRooms.Count == 0)
        {
            _outputManager.WriteLine("No available rooms to add onto. Please create a room first.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine("\n=== Adding New Room ===\n", ConsoleColor.Green);

        var roomToAddOnto = _inputManager.Selector(
            availableRooms.Keys.ToList(),
            r => $"{r.Name} [{string.Join(", ", availableRooms[r])}]",
            "Select the room you wish to add this room to",
            colorSelector: r => ConsoleColor.White);

        if (roomToAddOnto == null)
        {
            _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.\n", ConsoleColor.Red);
            return;
        }

        var directions = availableRooms[roomToAddOnto];

        _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}\n", ConsoleColor.Green);

        var directionToAddOnto = directions.Count == 1
            ? directions[0]
            : _inputManager.Selector(directions, d => d, "Select the direction to add onto");

        if (directionToAddOnto == null)
        {
            _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nAdding new room to the {directionToAddOnto} of {roomToAddOnto.Name}", ConsoleColor.Green);

        Room newRoom = CreateRoom();

        _mapHelper.LinkRooms(roomToAddOnto, newRoom, directionToAddOnto);

        _roomDao.AddRoom(newRoom);
        _roomDao.UpdateRoom(roomToAddOnto);

        _outputManager.Clear();
        _outputManager.WriteLine($"Room [{newRoom.Name}] succesfully created!\n", ConsoleColor.Green);
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
                _outputManager.WriteLine("\nRoom name already exists. Please choose a different name.\n", ConsoleColor.Red);
            }
            else break;
        }

        string description = _inputManager.ReadString("Enter description: ");
        return new Room(name, description);
    }
    private void EditRoomDescription()
    {
        var rooms = _roomDao.GetAllEditableRooms();

        if (rooms.Count == 0)
        {
            _outputManager.WriteLine("\nNo rooms available for editing.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.Write("\nnote: Entrance cannot be edited", ConsoleColor.Yellow);

        var roomToEdit = 
            _inputManager.Selector(
                rooms,
                r => ColorfulToStringHelper.RoomToString(r),
                "Select the room you wish to edit");

        if (roomToEdit == null)
        {
            _outputManager.WriteLine("\nRoom editing cancelled.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine($"\nOld Description: {roomToEdit.Description}");
        string newDescription = _inputManager.ReadString($"Enter new description for {roomToEdit.Name}: ");

        string confirm = _inputManager.ReadString($"\nPlease confirm new description of {roomToEdit.Name} (y/n): ", ["y", "n"]);
        if (confirm == "n")
        {
            _outputManager.WriteLine($"\nEditing cancelled. Room [{roomToEdit.Name}] has not been edited.\n", ConsoleColor.Red);
            return;
        }

        roomToEdit.Description = newDescription;
        _roomDao.UpdateRoom(roomToEdit);

        _outputManager.Clear();
        _outputManager.WriteLine($"{roomToEdit.Name}'s description updated successfully!\n", ConsoleColor.Green);
    }
    private void DeleteRoom()
    {
        var rooms = _roomDao.GetAllDeletableRooms();

        if (rooms.Count == 0)
        {
            _outputManager.WriteLine("\nNo rooms available for deletion.\n", ConsoleColor.Red);
            return;
        }

        _outputManager.WriteLine("\nnote: Entrance cannot be removed\n", ConsoleColor.Yellow);

        var roomToRemove = _inputManager.Selector(rooms, r => r.Name, "Select the room you wish to delete");

        if (roomToRemove == null)
        {
            _outputManager.WriteLine("Room deletion cancelled.\n", ConsoleColor.Red);
            return;
        }

        if (!_inputManager.ConfirmAction("deletion"))
        {
            _outputManager.WriteLine($"\nDeletion cancelled. Room [{roomToRemove.Name}] has not been removed.\n", ConsoleColor.Red);
            return;
        }

        _roomDao.DeleteRoom(roomToRemove);

        _outputManager.Clear();
        _outputManager.WriteLine("\nRoom has been successfully deleted!\n", ConsoleColor.Green);

        var disconnectedRooms = _mapHelper.CheckForDisconnectedRooms(_roomDao.GetAllRooms());
        if (disconnectedRooms.Count > 0)
        {
            _outputManager.WriteLine($"Warning: The following rooms are now disconnected: {string.Join(", ", disconnectedRooms.Select(r => r.Name))}", ConsoleColor.Yellow);
            _outputManager.WriteLine("Please check the map for any issues.\n", ConsoleColor.DarkYellow);
            _roomDao.UpdateAllRooms(disconnectedRooms);
        }
    }
}