using System.Collections.Concurrent;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers;

public class MapHelper(IRandomProvider randomProvider) : IMapHelper
{
    private readonly IRandomProvider _randomProvider = randomProvider;

    sealed record RoomPosition(Room Room, int X, int Y);


    public List<Room> CreateCampaignMap(int totalRoomsAllowed, Room entrance, List<Room> availableRooms)
    {
        Shuffle(availableRooms);

        availableRooms.Remove(entrance);

        var connectedRooms = new List<Room> { entrance };
        var frontier = new List<Room> { entrance };

        var placedGrid = GetRoomGrid(entrance); // Keeps track of used coordinates

        // Track the index of the next room to pick from shuffled availableRooms
        int nextRoomIndex = 0;

        // To store coordinates of rooms for positioning
        var roomCoordinates = new Dictionary<Room, (int x, int y)>
        {
            [entrance] = (0, 0)
        };

        while (connectedRooms.Count < totalRoomsAllowed && frontier.Count > 0 && nextRoomIndex < availableRooms.Count)
        {
            int frontierIndex = _randomProvider.Next(frontier.Count);
            var current = frontier[frontierIndex];
            frontier.RemoveAt(frontierIndex);

            var availableDirections = GetAvailableDirectionsForRoom(current, connectedRooms)
                .OrderBy(_ => _randomProvider.Next())
                .ToList();

            foreach (var direction in availableDirections)
            {
                if (nextRoomIndex >= availableRooms.Count || connectedRooms.Count >= totalRoomsAllowed) break;

                var (dx, dy) = GetDirectionOffset(direction);
                var (x, y) = roomCoordinates[current];
                var newCoords = (x + dx, y + dy);

                if (placedGrid.ContainsKey(newCoords)) continue;

                var newRoom = availableRooms[nextRoomIndex];
                nextRoomIndex++;

                LinkRooms(current, newRoom, direction);

                connectedRooms.Add(newRoom);
                frontier.Add(newRoom);
                placedGrid[newCoords] = newRoom;
                roomCoordinates[newRoom] = newCoords;
            }
        }

        ConnectNeighbors(connectedRooms);

        return connectedRooms;
    }

    // Fisher-Yates shuffle helper method
    private void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _randomProvider.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    public void UnlinkRoom(Room room)
    {
        foreach (var (neighbor, reverseSetter) in new (Room? neighbor, Action<Room?> reverseSetter)[]
                 {
                     (room.North, r =>
                     {
                         if (r != null) r.South = null;
                     }),
                     (room.South, r =>
                     {
                         if (r != null) r.North = null;
                     }),
                     (room.East, r =>
                     {
                         if (r != null) r.West = null;
                     }),
                     (room.West, r =>
                     {
                         if (r != null) r.East = null;
                     })
                 })
        {
            if (neighbor != null)
            {
                reverseSetter(neighbor);
            }
        }

        room.North = room.South = room.East = room.West = null;
    }

    public void UnlinkDirection(Room from, Room to, string direction)
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

    public void LinkRooms(Room from, Room to, string direction)
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

    public List<string> GetAvailableDirectionsForRoom(Room room, List<Room> connectedRooms)
    {
        var directions = new List<string>();
        
        if (connectedRooms.Count == 0)
            return [];
        
        try
        {
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
        }
        catch (InvalidOperationException)
        {
            return [];
        }

        return directions;
    }

    public Dictionary<(int x, int y), Room> GetRoomGrid(Room rootRoom)
    {
        Dictionary<(int x, int y), Room> gridCache = [];
        var root = rootRoom;
        var visited = new HashSet<Room>();

        var queue = new Queue<RoomPosition>();

        queue.Enqueue(new RoomPosition(root, 0, 0));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current.Room)) continue;

            gridCache[(current.X, current.Y)] = current.Room;

            if (current.Room.North != null)
                queue.Enqueue(new RoomPosition(current.Room.North, current.X, current.Y - 1)); // Y-1 for north
            if (current.Room.South != null)
                queue.Enqueue(new RoomPosition(current.Room.South, current.X, current.Y + 1)); // Y+1 for south
            if (current.Room.East != null)
                queue.Enqueue(new RoomPosition(current.Room.East, current.X + 1, current.Y));
            if (current.Room.West != null)
                queue.Enqueue(new RoomPosition(current.Room.West, current.X - 1, current.Y));
        }

        return gridCache;
    }

    public (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid)
    {
        foreach (var kvp in grid)
        {
            if (ReferenceEquals(kvp.Value, room))
                return kvp.Key;
        }

        throw new InvalidOperationException("Room not found in grid.");
    }

    public (int dx, int dy) GetDirectionOffset(string direction)
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

    public bool HasReverseConflict(Room baseRoom, string direction, List<Room> rooms)
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

    public Room? GetNeighbor(Room room, string direction)
    {
        return direction switch
        {
            "North" => room.North,
            "South" => room.South,
            "East" => room.East,
            "West" => room.West,
            _ => null
        };
    }

    public string GetReverseDirection(string direction)
    {
        return direction switch
        {
            "North" => "South",
            "South" => "North",
            "East" => "West",
            "West" => "East",
            _ => ""
        };
    }

    public Dictionary<Room, List<string>> GetAvailableDirections2(List<Room> rooms, Room? editingRoom = null)
    {
        var availableRooms = new Dictionary<Room, List<string>>();

        foreach (var room in rooms)
        {
            if (ReferenceEquals(room, editingRoom))
                continue;

            var availableDirections = GetAvailableDirectionsForRoom(room, rooms);

            if (availableDirections.Count > 0)
                availableRooms.Add(room, availableDirections);
        }

        return availableRooms;
    }
    
    public List<Room> FindRoomsDisconnectedByMove(Room entrance, Room roomToMove, Room newParent, string direction, List<Room> rooms)
    {
        string[] validDirections = ["North", "South", "East", "West"];

        if (!validDirections.Contains(direction))
            throw new ArgumentException("Invalid direction.", nameof(direction));

        if (ReferenceEquals(roomToMove, entrance))
            throw new ArgumentException("The entrance cannot be moved.", nameof(roomToMove));

        if (!rooms.Any(r => ReferenceEquals(r, roomToMove)) ||
            !rooms.Any(r => ReferenceEquals(r, newParent)) ||
            !rooms.Any(r => ReferenceEquals(r, entrance)))
        {
            throw new ArgumentException("All rooms must belong to the supplied room collection.");
        }

        if (GetNeighbor(newParent, direction) != null &&
            !ReferenceEquals(GetNeighbor(newParent, direction), roomToMove))
        {
            throw new InvalidOperationException("The selected direction is already occupied.");
        }

        // Rooms reachable before the proposed move.
        var reachableBefore = GetReachableRooms(entrance);

        // Simulate the move without changing any actual room connections.
        var reachableAfter = new HashSet<Room>();
        var queue = new Queue<Room>();

        queue.Enqueue(entrance);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (!reachableAfter.Add(current))
                continue;

            if (ReferenceEquals(current, roomToMove))
            {
                // The moved room will only retain its new connection.
                queue.Enqueue(newParent);
                continue;
            }

            // Existing connections remain, except connections to the moved room.
            foreach (var neighbor in new[]
                     {
                         current.North,
                         current.South,
                         current.East,
                         current.West
                     })
            {
                if (neighbor != null && !ReferenceEquals(neighbor, roomToMove))
                    queue.Enqueue(neighbor);
            }

            // The new parent gains a connection to the moved room.
            if (ReferenceEquals(current, newParent))
                queue.Enqueue(roomToMove);
        }

        // Only report rooms that were reachable before but won't be afterward.
        return rooms
            .Where(room =>
                reachableBefore.Contains(room) &&
                !reachableAfter.Contains(room))
            .ToList();
    }

    public Dictionary<string, string> FindUnlinkedNeighbors(List<Room> rooms)
    {
        Dictionary<string, string> unlinkedNeighbors = [];

        var grid = GetRoomGrid(rooms.First(r => r.Name.Equals("Entrance")));

        foreach (var room in rooms)
        {
            try
            {
                var (x, y) = FindRoomCoordinates(room, grid);

                string[] directions = ["North", "South", "East", "West"];

                foreach (var direction in directions)
                {
                    var (dx, dy) = GetDirectionOffset(direction);

                    // First, check if THIS room has a connection in that direction already
                    var neighbor = GetNeighbor(room, direction);
                    if (neighbor != null) continue; // already linked

                    // Next, check if there is a room at the target location
                    var targetCoords = (x + dx, y + dy);
                    if (!grid.TryGetValue(targetCoords, out var neighborRoom)) continue; // no room there

                    // Check if neighborRoom already links back toward this room
                    var reverseDirection = GetReverseDirection(direction);
                    var reverseNeighbor = GetNeighbor(neighborRoom, reverseDirection);
                    if (reverseNeighbor != null) continue; // already linked

                    // Configure key
                    string key = $"{room.Name}-{direction}";

                    // Check if the reverse is already in dictionary
                    string reverseKey = $"{neighborRoom.Name}-{reverseDirection}";
                    if (unlinkedNeighbors.ContainsKey(reverseKey)) continue;

                    //Add unlinked neighbor to dictionary
                    unlinkedNeighbors[key] = neighborRoom.Name;
                }
            }
            catch (InvalidOperationException)
            {
                //room not on grid
                continue;
            }
        }

        return unlinkedNeighbors;
    }
    
    public (string name, string direction) ParseKey(string key)
    {
        var parts = key.Split("-");
        return (parts[0], parts[1]);
    }

    private void ConnectNeighbors(List<Room> rooms)
    {
        var unlinkedNeighbors = FindUnlinkedNeighbors(rooms);

        if (unlinkedNeighbors.Count == 0) return;

        //determine how many to connect out of how many available
        //connect 1 in every 3?
        int neighborCount = unlinkedNeighbors.Count;

        var ratio = 3.0;
        var rawCount = neighborCount / ratio;
        var connectionCount = Math.Max(1, (int)Math.Round(rawCount + Random.Shared.NextDouble() - 0.5));

        for (int i = 0; i < connectionCount && unlinkedNeighbors.Count > 0; i++)
        {
            var randomElement = unlinkedNeighbors.ElementAt(_randomProvider.Next(unlinkedNeighbors.Count));
            var (roomName1, direction) = ParseKey(randomElement.Key);
            var roomName2 = randomElement.Value;

            var room1 = rooms.First(r => r.Name.Equals(roomName1));
            var room2 = rooms.First(r => r.Name.Equals(roomName2));

            LinkRooms(room1, room2, direction);
            unlinkedNeighbors.Remove(randomElement.Key);
        }
    }
    
    
    
    //Find All Rooms That Are Connected to the Entrance
    private static HashSet<Room> GetReachableRooms(Room entrance)
    {
        var visited = new HashSet<Room>();
        var queue = new Queue<Room>();

        queue.Enqueue(entrance);

        while (queue.Count > 0)
        {
            var room = queue.Dequeue();

            if (!visited.Add(room))
                continue;

            if (room.North != null) queue.Enqueue(room.North);
            if (room.South != null) queue.Enqueue(room.South);
            if (room.East != null) queue.Enqueue(room.East);
            if (room.West != null) queue.Enqueue(room.West);
        }

        return visited;
    }
    
    //Find All Rooms That Aren't Connected to the root room
    public List<Room> FindUnconnectedRooms(Room rootRoom, List<Room> rooms)
    {
        var reachableRooms = GetReachableRooms(rootRoom);

        return rooms
            .Where(room => !reachableRooms.Contains(room))
            .ToList();
    }
    
    //Find all rooms that aren't connected to the entrance and remove any connections they might still have
    public List<Room> CheckForDisconnectedRooms(List<Room> rooms)
    {
        var entrance = rooms.First(r => r.Id == 1);

        var disconnectedRooms = FindUnconnectedRooms(entrance, rooms);

        foreach (var room in disconnectedRooms)
        {
            UnlinkRoom(room);
        }

        return disconnectedRooms;
    }
}