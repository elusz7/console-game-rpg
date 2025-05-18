using ConsoleGame.GameDao;
using ConsoleGame.Managers;
using ConsoleGameEntities.Main.Models.Entities;

namespace ConsoleGame.Factories;

public class MapFactory(RoomDao roomDao)
{
    private readonly RoomDao _roomDao = roomDao;

    public List<Room> GenerateMap(int level, bool campaign, bool randomMap)
    {
        var rooms = (campaign || randomMap)
            ? [.. _roomDao.GetAllRooms().Select(r => r.Clone())]
            : _roomDao.GetAllRoomsNoTracking().ToList();

        var entrance = rooms.First(r => r.Name.Equals("Entrance", StringComparison.OrdinalIgnoreCase));
        var totalRoomsAllowed = Math.Min(level * 3, rooms.Count) + 1;

        if (campaign || randomMap)
        {
            rooms = MapHelper.CreateCampaignMap(totalRoomsAllowed, entrance, rooms);
        }
        else
        {
            var unconnectedRooms = MapHelper.FindUnconnectedRooms(entrance, rooms);
            rooms.RemoveAll(r => unconnectedRooms.Contains(r));
        }

        return rooms;
    }
}
