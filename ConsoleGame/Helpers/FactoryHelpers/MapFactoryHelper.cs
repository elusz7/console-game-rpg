using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGame.GameDao;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers.FactoryHelpers;

public static class MapFactoryHelper
{
    private static readonly Random _rng = new(Guid.NewGuid().GetHashCode());
    private record RoomPosition(Room Room, int X, int Y);
    

    public static List<Room> CreateCampaignMap(int totalRoomsAllowed, Room entrance, List<Room> availableRooms)
    {
        foreach (var room in availableRooms)
        {
            UnlinkRoom(room);
        }
        availableRooms.Remove(entrance); // Remove entrance from available rooms

        var connectedRooms = new List<Room> { entrance };
        var frontier = new Queue<Room>();
        frontier.Enqueue(entrance);

        var placedGrid = GetRoomGrid(entrance); // Keeps track of used coordinates

        while (connectedRooms.Count < totalRoomsAllowed && frontier.Count > 0 && availableRooms.Count > 0)
        {
            var current = frontier.Dequeue();
            var availableDirections = GetAvailableDirectionsForRoom(current, connectedRooms);

            foreach (var direction in availableDirections.OrderBy(_ => _rng.Next()))
            {
                if (availableRooms.Count == 0 || connectedRooms.Count >= totalRoomsAllowed) break;

                var (dx, dy) = GetDirectionOffset(direction);
                var (x, y) = FindRoomCoordinates(current, placedGrid);
                var newCoords = (x + dx, y + dy);

                if (placedGrid.ContainsKey(newCoords)) continue;

                var newRoom = availableRooms[_rng.Next(availableRooms.Count)];
                availableRooms.Remove(newRoom);

                LinkRooms(current, newRoom, direction);
                connectedRooms.Add(newRoom);
                frontier.Enqueue(newRoom);
                placedGrid[newCoords] = newRoom;
            }
        }

        return connectedRooms;
    }

    private static void UnlinkRoom(Room room)
    {
        foreach (var (neighbor, reverseSetter) in new (Room? neighbor, Action<Room?> reverseSetter)[]
        {
            (room.North, r => { if (r != null) r.South = null; }),
            (room.South, r => { if (r != null) r.North = null; }),
            (room.East,  r => { if (r != null) r.West = null; }),
            (room.West,  r => { if (r != null) r.East = null; })
        })
        {
            if (neighbor != null)
            {
                reverseSetter(neighbor);
            }
        }

        room.North = room.South = room.East = room.West = null;
    }
    private static void LinkRooms(Room from, Room to, string direction)
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
    private static List<string> GetAvailableDirectionsForRoom(Room room, List<Room> connectedRooms)
    {
        var directions = new List<string>();
        var grid = GetRoomGrid(connectedRooms.First()); // assumes root is the entrance
        var (x, y) = FindRoomCoordinates(room, grid);

        foreach (var dir in new[] { "North", "South", "East", "West" })
        {
            if (HasReverseConflict(room, dir, connectedRooms)) continue;

            var (dx, dy) = GetDirectionOffset(dir);
            var targetCoords = (x + dx, y + dy);

            if (!grid.ContainsKey(targetCoords)) // no logical conflict
            {
                directions.Add(dir);
            }
        }

        return directions;
    }
    private static Dictionary<(int x, int y), Room> GetRoomGrid(Room rootRoom)
    {
        Dictionary<(int x, int y), Room> _gridCache = [];
        var root = rootRoom;
        var visited = new HashSet<Room>();

        var queue = new Queue<RoomPosition>();

        queue.Enqueue(new RoomPosition(root, 0, 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current.Room)) continue;

            _gridCache[(current.X, current.Y)] = current.Room;

            if (current.Room.North != null)
                queue.Enqueue(new RoomPosition(current.Room.North, current.X, current.Y - 1)); // Y-1 for north
            if (current.Room.South != null)
                queue.Enqueue(new RoomPosition(current.Room.South, current.X, current.Y + 1)); // Y+1 for south
            if (current.Room.East != null)
                queue.Enqueue(new RoomPosition(current.Room.East, current.X + 1, current.Y));
            if (current.Room.West != null)
                queue.Enqueue(new RoomPosition(current.Room.West, current.X - 1, current.Y));
        }

        return _gridCache;
    }
    private static (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid)
    {
        foreach (var kvp in grid)
        {
            if (kvp.Value == room)
                return kvp.Key;
        }
        throw new InvalidOperationException("Room not found in grid.");
    }
    private static (int dx, int dy) GetDirectionOffset(string direction)
    {
        return direction switch
        {
            "North" => (0, -1),
            "South" => (0, 1),
            "East" => (1, 0),
            "West" => (-1, 0),
            _ => (0, 0)
        };
    }
    public static bool HasReverseConflict(Room baseRoom, string direction, List<Room> rooms)
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

    public static List<Room> FindUnconnectedRooms(Room rootRoom, List<Room> rooms)
    {
        var list = new List<Room>();

        var grid = GetRoomGrid(rootRoom);

        foreach (var room in rooms)
        {
            try
            {
                FindRoomCoordinates(room, grid);
            }
            catch (InvalidOperationException)
            {
                list.Add(room);
            }
        }

        return list;
    }
}
