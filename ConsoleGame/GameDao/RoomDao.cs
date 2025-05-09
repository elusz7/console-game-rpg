using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.GameDao;

public class RoomDao(GameContext context)
{
    private readonly GameContext _context = context;

    public List<Room> GetAllRooms()
    {
        return _context.Rooms?.ToList() ?? [];
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
