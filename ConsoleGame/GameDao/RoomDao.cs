using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.GameDao;

public class RoomDao
{
    private readonly GameContext _context;
    public RoomDao(GameContext context)
    {
        _context = context;
    }
    public void AddRoom(Room room)
    {        
        _context.Rooms.Add(room);
        _context.SaveChanges();
    }
    public void UpdateRoom(Room room)
    {        
        _context.Rooms.Update(room);
        _context.SaveChanges();
    }
    public void UpdateAllRooms(List<Room> rooms)
    {        
        _context.Rooms.UpdateRange(rooms);
        _context.SaveChanges();
    }
    public void DeleteRoom(Room room)
    {        
        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }
    public List<Room> GetAllRooms()
    {        
        return _context.Rooms.ToList();
    }
    public List<Room> GetAllEditableRooms()
    {        
        return _context.Rooms.Where(r => !r.Name.Equals("Entrance")).ToList();
    }
    public List<Room> GetAllRoomsMaxConnections(int maxConnections)
    {
        return GetAllEditableRooms()
            .Where(r =>
            {
                int connections = 0;
                if (r.North != null) connections++;
                if (r.South != null) connections++;
                if (r.East != null) connections++;
                if (r.West != null) connections++;
                return connections <= maxConnections;
            })
            .ToList();
    }
    public Room GetEntrance()
    {
        return _context.Rooms.FirstOrDefault(r => r.Name.Equals("Entrance"));
    }
    public Room FindRoomByName(string name)
    {
        return _context.Rooms.FirstOrDefault(r => r.Name.Equals(name));
    }
    public bool RoomExists(string name)
    {        
        return _context.Rooms.Any(r => r.Name.Equals(name));
    }
    public bool HasReverseConflict(Room baseRoom, string direction)
    {        
        return _context.Rooms.ToList().Any(r => direction switch
        {
            "North" => r.South == baseRoom,
            "South" => r.North == baseRoom,
            "East" => r.West == baseRoom,
            "West" => r.East == baseRoom,
            _ => false
        });
    }
}
