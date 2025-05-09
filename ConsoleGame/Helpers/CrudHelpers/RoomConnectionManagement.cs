using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Helpers.CrudHelpers;

public class RoomConnectionManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao, MapManager mapManager)
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
            _outputManager.WriteLine("Room Connection Management Menu", ConsoleColor.Cyan);
            _outputManager.WriteLine("1. Change Room Location"
                + "\n2. Connect Room to Entrance"
                + "\n3. Add Connection Between Unlinked Neighbors"
                + "\n4. Remove Connections"
                + "\n5. Return to Room Menu");

            var choice = _inputManager.ReadMenuKey(5);

            switch (choice)
            {
                case 1:
                    ChangeRoomPlacement();
                    break;
                case 2:
                    //ConnectRoomToEntrance();
                    ChangeRoomPlacement(true);
                    break;
                case 3:
                    LinkNeighbors();
                    break;
                case 4:
                    RemoveConnections();
                    break;
                case 5:
                    _outputManager.Clear();
                    return;
                default:
                    _outputManager.WriteLine("\nInvalid choice. Please try again.\n");
                    break;
            }
        }
    }
    private void ChangeRoomPlacement(bool disconnect = false)
    {
        string again;
        int connections = disconnect ? 0 : 4;
        do
        {
            List<Room> rooms = _roomDao.GetAllRoomsMaxConnections(connections);
            int index = SelectARoom("Select a room to move (-1 to cancel): ", rooms);

            if (index == -1)
            {
                _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.", ConsoleColor.Red);
                break;
            }

            Room roomToChange = rooms[index - 1];

            MoveRoom(roomToChange);

            again = _inputManager.ReadString("Would you like to move another? (y/n): ", new[] { "y", "n" }).ToLower();
        } while (again == "y");
        _outputManager.WriteLine();
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
    private void MoveRoom(Room roomToMove)
    {
        Dictionary<string, string[]> availableRooms = _mapManager.GetAvailableDirections(roomToMove);

        if (availableRooms.Count == 0)
        {
            _outputManager.WriteLine("\nNo valid rooms to place this room next to.\n", ConsoleColor.Red);
            return;
        }

        for (int i = 0; i < availableRooms.Count; i++)
        {
            var entry = availableRooms.ElementAt(i);
            _outputManager.WriteLine($"{i + 1}. {entry.Key} ({string.Join(", ", entry.Value)})");
        }

        int addOntoIndex = _inputManager.ReadInt($"\tEnter number of room where {roomToMove.Name} will go: ", availableRooms.Count);

        string selectedRoomName = availableRooms.ElementAt(addOntoIndex - 1).Key;

        Room roomToAddOnto = _roomDao.FindRoomByName(selectedRoomName);

        string[] directions = availableRooms[selectedRoomName];

        _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);

        string directionToAddOnto = directions.Length == 1
            ? directions[0]
            : _inputManager.ReadString($"\tEnter direction to add room ({string.Join("/", directions)}): ", directions);

        string confirm = _inputManager.ReadString($"\nAre you sure you want to move {roomToMove.Name} to the {directionToAddOnto} of {roomToAddOnto.Name}? (y/n): ", new[] { "y", "n" });

        if (confirm == "n")
        {
            _outputManager.WriteLine("\nRoom change placement cancelled.\n", ConsoleColor.Red);
            return;
        }

        UnlinkRoom(roomToMove);
        LinkRooms(roomToAddOnto, roomToMove, directionToAddOnto);
        _mapManager.DirtyGrid = true;

        _roomDao.UpdateRoom(roomToMove);
        _roomDao.UpdateRoom(roomToAddOnto);

        _mapManager.CheckForDisconnectedRooms();
        
        _outputManager.WriteLine($"\n{roomToMove.Name} successfully moved!\n", ConsoleColor.Green);
    }
    private void LinkNeighbors()
    {
        string again;
        var unlinkedNeighbors = _mapManager.FindUnlinkedNeighbors();
        do
        {             
            _outputManager.WriteLine();

            if (unlinkedNeighbors.Count == 0)
            {
                _outputManager.WriteLine("\nNo unlinked neighbors found.", ConsoleColor.Yellow);
                break;
            }

            for (int i = 0; i < unlinkedNeighbors.Count; i++)
            {
                var entry = unlinkedNeighbors.ElementAt(i);
                var keyParts = entry.Key.Split("-");
                var (name1, direction) = ParseKey(entry.Key);
                var name2 = entry.Value;

                _outputManager.WriteLine($"{i + 1}. {name2} is {direction} of {name1}");
            }

            int index = _inputManager.ReadInt("\nSelect which rooms you'd like to link (-1 to cancel): ", unlinkedNeighbors.Count, cancel: true);

            if (index == -1)
            {
                _outputManager.WriteLine("\nLinking cancelled.", ConsoleColor.Red);
                break;
            }

            var selectedEntry = unlinkedNeighbors.ElementAt(index - 1);
            var parts = selectedEntry.Key.Split("-");
            var (roomName1, directionToLink) = ParseKey(selectedEntry.Key);
            var roomName2 = selectedEntry.Value;

            var confirm = _inputManager.ReadString($"\nConfirm linking {roomName2} to the {directionToLink} of {roomName1} (y/n): ", new[] { "y", "n" }).ToLower();

            if (confirm == "n")
            {
                _outputManager.WriteLine("\nLinking cancelled.", ConsoleColor.Red);
                break;
            }

            var roomA = _roomDao.FindRoomByName(roomName1);
            var roomB = _roomDao.FindRoomByName(roomName2);

            LinkRooms(roomA, roomB, directionToLink);

            _roomDao.UpdateRoom(roomA);
            _roomDao.UpdateRoom(roomB);

            _mapManager.DirtyGrid = true;

            _outputManager.WriteLine($"\n{roomName1} and {roomName2} have been successfully linked!\n", ConsoleColor.Green);

            unlinkedNeighbors = _mapManager.FindUnlinkedNeighbors();
            if (unlinkedNeighbors.Count == 0)
            {
                _outputManager.WriteLine("\nNo more unlinked neighbors remaining!", ConsoleColor.Green);
                break;
            }

            again = _inputManager.ReadString("Would you like to link another? (y/n): ", new[] { "y", "n" }).ToLower();

        } while (again == "y");
        _outputManager.WriteLine();
    }
    private (string name, string direction) ParseKey(string key)
    {
        var parts = key.Split("-");
        return (parts[0], parts[1]);
    }
    private void RemoveConnections()
    {
        string again;
        do
        {
            var rooms = _roomDao.GetAllRoomsMaxConnections(0);
            if (!rooms.Any())
            {
                _outputManager.WriteLine("\nNo rooms available with all connections intact.", ConsoleColor.Yellow);
                break;
            }

            var index = SelectARoom("Select The Room You Wish To Remove A Connection From: ", rooms);

            if (index == -1)
            {
                _outputManager.WriteLine("\nConnection Removal Cancelled.", ConsoleColor.Red);
                break;
            }

            var roomToChange = rooms[index - 1];
            var directions = new Dictionary<string, Room>();

            if (roomToChange.North != null) directions.Add("North", roomToChange.North);
            if (roomToChange.South != null) directions.Add("South", roomToChange.South);
            if (roomToChange.East != null) directions.Add("East", roomToChange.East);
            if (roomToChange.West != null) directions.Add("West", roomToChange.West);

            _outputManager.Write($"\n{roomToChange.Name} has {directions.Count} connection(s): ");

            int loopCount = directions.Count;
            foreach (var dir in directions)
            {
                loopCount--;
                _outputManager.Write($"{dir.Key} - {dir.Value.Name}");
                if (loopCount != 0)
                    _outputManager.Write(", ");
                else
                    _outputManager.Write("\n");
            }

            var removeDirection = directions.ElementAt(0).Key;
            if (directions.Count != 1)
            {
                for (int i = 0; i < directions.Count; i++)
                {
                    _outputManager.WriteLine($"{i + 1}. {directions.ElementAt(i).Key}");
                }
                var choice = _inputManager.ReadInt("Select the direction you'd like to remove: ", directions.Count) - 1;
                removeDirection = directions.ElementAt(choice).Key;
            }

            var confirm = _inputManager.ReadString($"\nConfirm removal of link between {roomToChange.Name} and {directions[removeDirection].Name} (y/n): ", new[] { "y", "n" }).ToLower();
            if (confirm == "n")
            {
                _outputManager.WriteLine("\nConnection Removal Cancelled.", ConsoleColor.Red);
                break;
            }

            UnlinkDirection(roomToChange, directions[removeDirection], removeDirection);

            _roomDao.UpdateRoom(roomToChange);
            _roomDao.UpdateRoom(directions[removeDirection]);
            _mapManager.DirtyGrid = true;

            _mapManager.CheckForDisconnectedRooms();

            _outputManager.WriteLine($"\n{roomToChange.Name} and {directions[removeDirection].Name} have been successfully disconnected!\n", ConsoleColor.Green);

            again = _inputManager.ReadString("Would you like to disconnect another link? (y/n): ", new[] { "y", "n" }).ToLower();
        } while (again == "y");
        _outputManager.WriteLine();
    }
    private void UnlinkDirection(Room from, Room to, string direction)
    {
        switch (direction)
        {
            case "North":
                from.North = null;
                to.South = null;
                break;
            case "South":
                from.South = null;
                to.North = null;
                break;
            case "East":
                from.East = null;
                to.West = null;
                break;
            case "West":
                from.West = null;
                to.East = null;
                break;
        }
    }
}