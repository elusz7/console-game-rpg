using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers;

public class RoomConnectionManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao, MapManager mapManager)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly RoomDao _roomDao = roomDao;
    private readonly MapManager _mapManager = mapManager;

    private Dictionary<(int x, int y), Room> _roomGridCache;
    private bool _gridDirty = true;

    public void Menu()
    {
        _outputManager.Clear();
        while (true)
        {
            _outputManager.WriteLine("\nRoom Connection Management Menu", ConsoleColor.Cyan);
            string menuPrompt = "1. Change Room Placement"
                + "\n2. Add Room Connection"
                + "\n3. Remove Room Connection"
                + "\n4. Return to Main Menu"
                + "\n\tSelect an option: ";
            string choice = _inputManager.ReadString(menuPrompt);
            switch (choice)
            {
                case "1":
                    ChangeRoomPlacement();
                    break;
                case "2":
                    //EditRoomConnection();
                    break;
                case "3":
                    //RemoveRoomConnection();
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
    private void ChangeRoomPlacement()
    {
        List<Room> editableRooms = _roomDao.GetAllEditableRooms();
        int index = SelectARoom("Select a room to move (-1 to cancel): ", editableRooms);

        if (index == -1)
        {
            _outputManager.WriteLine("Room placement cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

        Room roomToChange = editableRooms[index - 1];

        Dictionary<string, string[]> availableRooms = GetAvailableDirections(roomToChange);

        if (availableRooms.Count == 0)
        {
            _outputManager.WriteLine("No valid rooms to place this room next to.", ConsoleColor.Red);
            return;
        }

        int addOntoIndex = _inputManager.ReadInt($"\tEnter number of room where {roomToChange.Name} will go: ", availableRooms.Count);

        string selectedRoomName = availableRooms.ElementAt(addOntoIndex - 1).Key;

        Room roomToAddOnto = _roomDao.FindRoomByName(selectedRoomName);

        string[] directions = availableRooms[selectedRoomName];

        _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);

        string directionToAddOnto = directions.Length == 1
            ? directions[0]
            : _inputManager.ReadString($"\tEnter direction to add room ({string.Join("/", directions)}): ", directions);

        string confirm = _inputManager.ReadString($"\nAre you sure you want to move {roomToChange.Name} to the {directionToAddOnto} of {roomToAddOnto.Name}? (y/n): ", new[] { "y", "n" });

        if (confirm == "n")
        {
            _outputManager.WriteLine("Room change placement cancelled.", ConsoleColor.Red);
            return;
        }

        UnlinkRoom(roomToChange);
        LinkRooms(roomToAddOnto, roomToChange, directionToAddOnto);
        _gridDirty = true;

        _roomDao.UpdateRoom(roomToChange);
        _roomDao.UpdateRoom(roomToAddOnto);

        List<Room> allRooms = _roomDao.GetAllRooms();
        RemoveLinksInUnconnectedRooms(allRooms);
        _gridDirty = true;

        _roomDao.UpdateAllRooms(allRooms);
    }
    private int SelectARoom(string prompt, List<Room> roomList)
    {
        _outputManager.WriteLine();

        for (int i = 0; i < roomList.Count; i++)
        {
            _outputManager.WriteLine($"{i + 1}. {roomList[i].Name}");
        }

        return _inputManager.ReadInt($"\t{prompt}", roomList.Count, true);
    }
    private Dictionary<string, string[]> GetAvailableDirections(Room? editingRoom = null)
    {
        var availableRooms = new Dictionary<string, string[]>();
        var allRooms = _roomDao.GetAllRooms();

        _outputManager.WriteLine();
        for (int i = 0; i < allRooms.Count; i++)
        {
            var room = allRooms[i];

            if (room == editingRoom) continue;

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
        string finalDirections = null;

        var grid = GetRoomGrid();
        try
        {
            var baseCoords = _mapManager.FindRoomCoordinates(baseRoom, grid);

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
            finalDirections = null;
        }

        return finalDirections;
    }
    private (int dx, int dy) GetDirectionOffset(string direction)
    {
        return direction switch
        {
            "North" => (0, 1),
            "South" => (0, -1),
            "East" => (1, 0),
            "West" => (-1, 0),
            _ => (0, 0)  // Default case if direction is unrecognized
        };
    }
    private void UnlinkRoom(Room room)
    {
        foreach (var (neighbor, reverseSetter) in new (Room neighbor, Action<Room> reverseSetter)[]
        {
        (room.North, r => r.South = null),
        (room.South, r => r.North = null),
        (room.East,  r => r.West = null),
        (room.West,  r => r.East = null)
        })
        {
            if (neighbor != null)
            {
                reverseSetter(neighbor);
            }
        }

        room.North = room.South = room.East = room.West = null;
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
    private void RemoveLinksInUnconnectedRooms(List<Room> roomList)
    {
        var grid = GetRoomGrid();
        foreach (var room in roomList)
        {
            try
            {
                var coords = _mapManager.FindRoomCoordinates(room, grid);
            }
            catch (InvalidOperationException ex) { 
                UnlinkRoom(room);
            }
        }
    }
    private Dictionary<(int x, int y), Room> GetRoomGrid()
    {
        if (_gridDirty || _roomGridCache == null)
        {
            _roomGridCache = _mapManager.GetRoomGrid();
            _gridDirty = false;
        }
        return _roomGridCache;
    }
}