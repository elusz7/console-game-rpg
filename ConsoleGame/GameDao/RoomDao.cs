using ConsoleGameEntities.Data;
using ConsoleGameEntities.Migrations;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.GameDao;

public class RoomDao(GameContext context)
{
    private readonly GameContext _context = context;

    public void AddRoom(Room room)
    {
        _context.Rooms?.Add(room);
        _context.SaveChanges();
    }

    public void UpdateRoom(Room room)
    {
        _context.Rooms?.Update(room);
        _context.SaveChanges();
    }

    public void UpdateAllRooms(List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            _context.Rooms?.Update(room);
        }
        _context.SaveChanges();
    }

    public void DeleteRoom(Room room)
    {
        _context.Rooms?.Remove(room);
        _context.SaveChanges();
    }

    public List<Room> GetAllRooms()
    {
        return _context.Rooms?.ToList() ?? [];
    }

    public Room? FindRoomByName(string name)
    {
        return _context.Rooms?.ToList().FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public bool RoomExists(string name)
    {
        return _context.Rooms?.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public Room GetEntrance()
    {
        return _context.Rooms?.First(r => r.Id == 1) ?? throw new InvalidOperationException("Entrance room not found.");
    }

    public List<Room> GetAllEditableRooms()
    {
        return _context.Rooms?.Where(r => !r.Name.Equals("Entrance")).ToList() ?? [];
    }

    public List<Room> GetAllDeletableRooms()
    {
        return _context.Rooms?.Where(r => r.Id > 64).ToList() ?? [];
    }

    public List<Room> GetAllRoomsMaxConnections(int maxConnections)
    {
        var editableRooms = GetAllEditableRooms();
        return [.. editableRooms.Where(r =>
        {
            int connections = 0;
            if (r.North != null) connections++;
            if (r.South != null) connections++;
            if (r.East != null) connections++;
            if (r.West != null) connections++;
            return connections <= maxConnections;
        })];
    }
}
