using System.Collections.Concurrent;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Managers;

public class MapHelper : IMapHelper
{
    private static readonly Random _rng = Random.Shared;
    private record RoomPosition(Room Room, int X, int Y);


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
            int frontierIndex = _rng.Next(frontier.Count);
            var current = frontier[frontierIndex];
            frontier.RemoveAt(frontierIndex);

            var availableDirections = GetAvailableDirectionsForRoom(current, connectedRooms)
                .OrderBy(_ => _rng.Next())
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
    private static void Shuffle<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public void UnlinkRoom(Room room)
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
        var grid = GetRoomGrid(connectedRooms.First()); // assumes root is the entrance
        try
        {
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
        catch (InvalidOperationException) { return []; }

        return directions;
    }
    public Dictionary<(int x, int y), Room> GetRoomGrid(Room rootRoom)
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
    public (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid)
    {
        foreach (var kvp in grid)
        {
            if (kvp.Value.Id == room.Id)
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
    public List<Room> FindUnconnectedRooms(Room rootRoom, List<Room> rooms)
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
                Console.WriteLine($"Unconnected: {room.Name}");
                list.Add(room);
            }
        }


        return list;
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
    public int CountPathsToEntranceThroughRoom(Room startRoom, Room requiredRoom, Room entrance)
    {
        int pathCount = 0;

        var queue = new Queue<Room>();
        var visitedRooms = new HashSet<Room>();

        // Start BFS from the startRoom
        queue.Enqueue(startRoom);

        // Keep track of the rooms that are visited as part of the current path
        var roomParents = new Dictionary<Room, Room>();

        while (queue.Count > 0)
        {
            var currentRoom = queue.Dequeue();

            if (currentRoom == entrance)
            {
                // Check if the requiredRoom is part of the path by walking back from entrance
                var pathContainsRequired = false;
                var parent = currentRoom;

                // Walk backwards through parent map to see if requiredRoom is part of the path
                while (parent != null)
                {
                    if (parent == requiredRoom)
                    {
                        pathContainsRequired = true;
                        break;
                    }

                    parent = roomParents.GetValueOrDefault(parent);
                }

                if (pathContainsRequired)
                {
                    pathCount++;
                }
                continue; // If reached entrance, no need to explore further
            }

            if (visitedRooms.Contains(currentRoom)) continue;

            visitedRooms.Add(currentRoom);

            // Enqueue neighbors (only unvisited rooms)
            if (currentRoom.North != null && !visitedRooms.Contains(currentRoom.North))
            {
                roomParents[currentRoom.North] = currentRoom;
                queue.Enqueue(currentRoom.North);
            }
            if (currentRoom.South != null && !visitedRooms.Contains(currentRoom.South))
            {
                roomParents[currentRoom.South] = currentRoom;
                queue.Enqueue(currentRoom.South);
            }
            if (currentRoom.East != null && !visitedRooms.Contains(currentRoom.East))
            {
                roomParents[currentRoom.East] = currentRoom;
                queue.Enqueue(currentRoom.East);
            }
            if (currentRoom.West != null && !visitedRooms.Contains(currentRoom.West))
            {
                roomParents[currentRoom.West] = currentRoom;
                queue.Enqueue(currentRoom.West);
            }
        }

        return pathCount;
    }
    public List<Room>? SinglePathThrough(Room requiredRoom, List<Room> rooms)
    {
        var entrance = rooms.Where(r => r.Id == 1).First();

        var singlePathRooms = new ConcurrentBag<Room>();

        Parallel.ForEach(rooms, room =>
        {
            int pathCount = CountPathsToEntranceThroughRoom(room, requiredRoom, entrance);
            if (pathCount == 1)
            {
                singlePathRooms.Add(room);
            }
        });

        return [.. singlePathRooms];
    }    
    public Dictionary<Room, List<string>> GetAvailableDirections2(List<Room> rooms, Room? editingRoom = null)
    {
        var availableRooms = new Dictionary<Room, List<string>>();
        var singlePathRooms = editingRoom == null ? [] : SinglePathThrough(editingRoom, rooms);

        foreach (Room room in rooms)
        {
            if (room == editingRoom) continue;
            if (editingRoom != null && singlePathRooms != null && singlePathRooms.Contains(room)) continue;

            var availableDirections = GetAvailableDirectionsForRoom(room, rooms);

            if (availableDirections.Count == 0) continue;

            availableRooms.Add(room, availableDirections);
        }

        return availableRooms;
    }
    public Dictionary<string, string> FindUnlinkedNeighbors(List<Room> rooms)
    {
        Dictionary<string, string> unlinkedNeighbors = [];

        var grid = GetRoomGrid(rooms.Where(r => r.Name.Equals("Entrance")).First());

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
                    if (!grid.ContainsKey(targetCoords)) continue; // no room there

                    var neighborRoom = grid[targetCoords];

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
    public List<Room> CheckForDisconnectedRooms(List<Room> rooms)
    {
        var disconnectedRooms = new List<Room>();

        var grid = GetRoomGrid(rooms.Where(r => r.Id == 1).First());

        foreach (var room in rooms)
        {
            try
            {
                FindRoomCoordinates(room, grid);
            }
            catch (InvalidOperationException)
            {
                //set disconnected room connects to null
                room.North = room.South = room.East = room.West = null;

                disconnectedRooms.Add(room);
            }
        }

        return disconnectedRooms;
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
        int neighborCount = unlinkedNeighbors.Values
            .Select(v => v.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
            .Sum();

        var ratio = 3.0;
        var rawCount = neighborCount / ratio;
        var connectionCount = Math.Max(1, (int)Math.Round(rawCount + Random.Shared.NextDouble() - 0.5));

        for (int i = 0; i < connectionCount && unlinkedNeighbors.Count > 0; i++)
        {
            var randomElement = unlinkedNeighbors.ElementAt(_rng.Next(unlinkedNeighbors.Count));
            var (roomName1, direction) = ParseKey(randomElement.Key);
            var roomName2 = randomElement.Value;

            var room1 = rooms.Where(r => r.Name.Equals(roomName1)).First();
            var room2 = rooms.Where(r => r.Name.Equals(roomName2)).First();

            LinkRooms(room1, room2, direction);
            unlinkedNeighbors.Remove(randomElement.Key);
        }
    }

}
