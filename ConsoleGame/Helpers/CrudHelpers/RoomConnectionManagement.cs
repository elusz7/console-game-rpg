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

            if (rooms.Count == 0)
            {
                _outputManager.WriteLine("\nNo rooms available!\n", ConsoleColor.Red);
                return;
            }
            
            var room = _inputManager.Selector(
                rooms,
                r => r.Name,
                "Select the room you wish to change the placement of: ",
                colorSelector: r => r.Id == 1 ? ConsoleColor.Red : ConsoleColor.White);

            if (room == null)
            {
                _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.", ConsoleColor.Red);
                break;
            }

            MoveRoom(room);
        } while (_inputManager.LoopAgain("move"));
        _outputManager.WriteLine();
    }
    private void MoveRoom(Room roomToMove)
    {
        //Dictionary<string, string[]> availableRooms = MapHelper.GetAvailableDirections(_roomDao.GetAllRooms(), roomToMove);

        var availableRooms = MapHelper.GetAvailableDirections2(_roomDao.GetAllRooms(), roomToMove);

        if (availableRooms.Count == 0)
        {
            _outputManager.WriteLine("\nNo valid rooms to place this room next to.\n", ConsoleColor.Red);
            return;
        }

        //for (int i = 0; i < availableRooms.Count; i++)
        //{
        //    var entry = availableRooms.ElementAt(i);
        //    _outputManager.WriteLine($"{i + 1}. {entry.Key} ({string.Join(", ", entry.Value)})");
        //}

        var roomToAddOnto = _inputManager.Selector(
            availableRooms.Keys.ToList(),
            r => $"{r.Name} [{string.Join(", ", availableRooms[r])}]",
            "Select the room you wish to add this room to: ",
            colorSelector: r => ConsoleColor.White);

        if (roomToAddOnto == null)
        {
            _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

        var directions = availableRooms[roomToAddOnto];

        _outputManager.WriteLine($"\nYou selected {roomToAddOnto.Name}. Available directions: {string.Join(", ", directions)}", ConsoleColor.Green);

        var directionToAddOnto = directions.Count == 1
            ? directions[0]
            : _inputManager.Selector(directions, d => d, "Select the direction to add onto");

        if (directionToAddOnto == null)
        {
            _outputManager.WriteLine("\nRoom placement cancelled. Returning to menu.", ConsoleColor.Red);
            return;
        }

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
        do
        {
            var unlinkedNeighbors = MapHelper.FindUnlinkedNeighbors(_roomDao.GetAllRooms());
            _outputManager.WriteLine();

            if (unlinkedNeighbors.Count == 0)
            {
                _outputManager.WriteLine("\nNo unlinked neighbors found.", ConsoleColor.Yellow);
                break;
            }

            var entry = _inputManager.Selector(
                unlinkedNeighbors.Keys.ToList(),
                r => {
                    var parts = r.Split("-");
                    var (name1, direction) = ParseKey(r);
                    return $"{name1} is {direction} of {unlinkedNeighbors[r]}";
                },
                "Select the room you wish to link");

            if (entry == null)
            {
                _outputManager.WriteLine("\nLinking cancelled.", ConsoleColor.Red);
                break;
            }

            var (roomName1, directionToLink) = ParseKey(entry);
            var roomName2 = unlinkedNeighbors[entry];

            var confirm = _inputManager.ReadString($"\nConfirm linking {roomName2} to the {directionToLink} of {roomName1} (y/n): ", ["y", "n"]).ToLower();

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

            var baseRoom = _inputManager.Selector(
                rooms,
                r => r.Name,
                "Select the room you wish to remove a connection from: ",
                colorSelector: r => ConsoleColor.White);

            if (baseRoom == null)
            {
                _outputManager.WriteLine("\nConnection Removal Cancelled.", ConsoleColor.Red);
                continue;
            }

            var directions = new Dictionary<string, Room>();
            if (baseRoom.North != null) directions.Add("North", baseRoom.North);
            if (baseRoom.South != null) directions.Add("South", baseRoom.South);
            if (baseRoom.East != null) directions.Add("East", baseRoom.East);
            if (baseRoom.West != null) directions.Add("West", baseRoom.West);

            if (directions.Count == 0)
            {
                _outputManager.WriteLine($"\n{baseRoom.Name} has no connections to remove.", ConsoleColor.Yellow);
                continue;
            }

            var direction = _inputManager.Selector(
                directions.Keys.ToList(),
                r => $"{r} - {directions[r].Name}",
                "Select the room you wish to remove a connection to: ",
                colorSelector: r => ConsoleColor.White);

            if (direction == null)
            {
                _outputManager.WriteLine("\nConnection Removal Cancelled.", ConsoleColor.Red);
                continue;
            }

            var connectionToRemove = directions[direction];

            var confirm = _inputManager.ReadString(
                $"\nConfirm removal of link between {baseRoom.Name} and {connectionToRemove.Name} (y/n): ", ["y", "n"]).ToLower();

            if (confirm == "n")
            {
                _outputManager.WriteLine("\nConnection Removal Cancelled.", ConsoleColor.Red);
                continue;
            }

            MapHelper.UnlinkDirection(baseRoom, connectionToRemove, direction);
            _roomDao.UpdateRoom(baseRoom);
            _roomDao.UpdateRoom(connectionToRemove);

            _outputManager.Clear();

            var disconnectedRooms = MapHelper.CheckForDisconnectedRooms(_roomDao.GetAllRooms());
            if (disconnectedRooms.Count > 0)
            {
                _outputManager.WriteLine($"{disconnectedRooms.Count} do not have a path to the entrance.", ConsoleColor.DarkYellow);
                _roomDao.UpdateAllRooms(disconnectedRooms);
            }

            _outputManager.WriteLine($"\n{baseRoom.Name} and {connectionToRemove.Name} have been successfully disconnected!\n", ConsoleColor.Green);

        } while (_inputManager.LoopAgain("disconnect"));

        _outputManager.WriteLine();
    }
}