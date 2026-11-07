using ConsoleGame.GameDao.Interfaces;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class RoomDao(GameContext context) : IRoomDao
{
    private readonly GameContext _context = context;

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
        foreach (var room in rooms)
        {
            _context.Rooms.Update(room);
        }

        _context.SaveChanges();
    }

    public void DeleteRoom(Room room)
    {
        _context.Rooms.Remove(room);
        _context.SaveChanges();
    }

    public List<Room> GetAllRooms()
    {
        return [.._context.Rooms
            .Include(r => r.Monsters)
            ];
    }

    public Room? FindRoomByName(string name)
    {
        return _context.Rooms.FirstOrDefault(r => EF.Functions.Like(r.Name, $"%{name}%"));
    }

    public bool RoomExists(string name)
    {
        return _context.Rooms.Any(r =>
            EF.Functions.Collate(r.Name, "NOCASE") == name);
    }

    public Room GetEntrance()
    {
        return _context.Rooms.First(r => r.Id == 1) ?? throw new InvalidOperationException("Entrance room not found.");
    }

    public List<Room> GetAllEditableRooms()
    {
        return [.._context.Rooms.Where(r => r.Id != 1)];
    }

    public List<Room> GetAllDeletableRooms()
    {
        return [.._context.Rooms.Where(r => !r.IsCore)];
    }
    
    public List<Room> GetAllRoomsMaxConnections(int maxConnections)
    {
        var editableRooms = GetAllEditableRooms();

        return
        [
            .. editableRooms.Where(r =>
            {
                var connections = 0;

                if (r.NorthId != null) connections++;
                if (r.SouthId != null) connections++;
                if (r.EastId != null) connections++;
                if (r.WestId != null) connections++;

                return connections <= maxConnections;
            })
        ];
    }
}