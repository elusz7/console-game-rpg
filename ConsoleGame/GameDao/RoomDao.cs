using ConsoleGameEntities.Data;
using ConsoleGameEntities.Models.Rooms;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao;

public class RoomDao
{
    private readonly IDbContextFactory<GameContext> _contextFactory;
    public RoomDao(IDbContextFactory<GameContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    public void AddRoom(Room room)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Rooms.Add(room);
        context.SaveChanges();
    }
    public void UpdateRoom(Room room)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Rooms.Update(room);
        context.SaveChanges();
    }
    public void UpdateAllRooms(List<Room> rooms)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Rooms.UpdateRange(rooms);
        context.SaveChanges();
    }
    public void DeleteRoom(Room room)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Rooms.Remove(room);
        context.SaveChanges();
    }
    public List<Room> GetAllRooms()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.ToList();
    }
    public List<Room> GetAllEditableRooms()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.Where(r => !r.Name.Equals("Entrance")).ToList();
    }
    public List<Room> GetAllDeletableRooms()
    {
        List<Room> allRooms = GetAllEditableRooms();

        List<Room> deletableRooms = allRooms
            .Where(r =>
            {
                int connections = 0;
                if (r.North != null) connections++;
                if (r.South != null) connections++;
                if (r.East != null) connections++;
                if (r.West != null) connections++;
                return connections <= 1; // 0 or 1 connections
            })
            .ToList();

        return deletableRooms;
    }
    public Room GetEntrance()
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.FirstOrDefault(r => r.Name.Equals("Entrance"));
    }
    public Room FindRoomByName(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    public bool RoomExists(string name)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    public bool HasReverseConflict(Room baseRoom, string direction)
    {
        using var context = _contextFactory.CreateDbContext();
        return context.Rooms.ToList().Any(r => direction switch
        {
            "North" => r.South == baseRoom,
            "South" => r.North == baseRoom,
            "East" => r.West == baseRoom,
            "West" => r.East == baseRoom,
            _ => false
        });
    }
}
