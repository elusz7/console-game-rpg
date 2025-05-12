using ConsoleGame.GameDao;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.Managers.CrudHelpers;

public class RoomConnectionManagement(InputManager inputManager, OutputManager outputManager, RoomDao roomDao)
{
    private readonly InputManager _inputManager = inputManager;
    private readonly OutputManager _outputManager = outputManager;
    private readonly RoomDao _roomDao = roomDao;

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
        } while (_inputManager.LoopAgain("move"));
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
    private void MoveRoom(Room roomToMove)
    {
        Dictionary<string, string[]> availableRooms = MapHelper.GetAvailableDirections(_roomDao.GetAllRooms(), roomToMove);

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

        Room? roomToAddOnto = _roomDao.FindRoomByName(selectedRoomName);

        if (roomToAddOnto == null)
        {
            _outputManager.WriteLine("\nInvalid room selection. Please try again.\n", ConsoleColor.Red);
            //if this happens, something went very wrong
            return;
        }

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

        MapHelper.UnlinkRoom(roomToMove);
        MapHelper.LinkRooms(roomToAddOnto, roomToMove, directionToAddOnto);

        _roomDao.UpdateRoom(roomToMove);
        _roomDao.UpdateRoom(roomToAddOnto);

        var disconnectedRooms = MapHelper.CheckForDisconnectedRooms(_roomDao.GetAllRooms());
        if (disconnectedRooms.Count > 0)
        {
            _outputManager.WriteLine($"\nThe following rooms are now disconnected from the map: {string.Join(", ", disconnectedRooms.Select(r => r.Name))}", ConsoleColor.Yellow);
            _roomDao.UpdateAllRooms(disconnectedRooms);
        }

        _outputManager.WriteLine($"\n{roomToMove.Name} successfully moved!\n", ConsoleColor.Green);
    }
    private void LinkNeighbors()
    {
        var unlinkedNeighbors = MapHelper.FindUnlinkedNeighbors(_roomDao.GetAllRooms());
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

            if (roomA == null || roomB == null)
            {
                _outputManager.WriteLine("\nInvalid room selection. Please try again.\n", ConsoleColor.Red);
                //this really shouldn't happen, but just in case
                break;
            }

            MapHelper.LinkRooms(roomA, roomB, directionToLink);

            _roomDao.UpdateRoom(roomA);
            _roomDao.UpdateRoom(roomB);

            _outputManager.WriteLine($"\n{roomName1} and {roomName2} have been successfully linked!\n", ConsoleColor.Green);

            unlinkedNeighbors = MapHelper.FindUnlinkedNeighbors(_roomDao.GetAllRooms());
            if (unlinkedNeighbors.Count == 0)
            {
                _outputManager.WriteLine("\nNo more unlinked neighbors remaining!", ConsoleColor.Green);
                break;
            }
        } while (_inputManager.LoopAgain("link"));
        _outputManager.WriteLine();
    }
    private static (string name, string direction) ParseKey(string key)
    {
        var parts = key.Split("-");
        return (parts[0], parts[1]);
    }
    private void RemoveConnections()
    {
        do
        {
            var rooms = _roomDao.GetAllRoomsMaxConnections(0);
            if (rooms.Count == 0)
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

            MapHelper.UnlinkDirection(roomToChange, directions[removeDirection], removeDirection);

            _roomDao.UpdateRoom(roomToChange);
            _roomDao.UpdateRoom(directions[removeDirection]);

            var disconnectedRooms = MapHelper.CheckForDisconnectedRooms(_roomDao.GetAllRooms());
            if (disconnectedRooms.Count > 0)
            {
                _outputManager.WriteLine($"\nThe following rooms are now disconnected from the map: {string.Join(", ", disconnectedRooms.Select(r => r.Name))}", ConsoleColor.Yellow);
                _roomDao.UpdateAllRooms(disconnectedRooms);
            }

            _outputManager.WriteLine($"\n{roomToChange.Name} and {directions[removeDirection].Name} have been successfully disconnected!\n", ConsoleColor.Green);
        } while (_inputManager.LoopAgain("disconnect"));
        _outputManager.WriteLine();
    }
}