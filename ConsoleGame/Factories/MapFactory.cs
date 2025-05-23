using ConsoleGame.Factories.Interfaces;
using ConsoleGame.GameDao.Interfaces;
using ConsoleGame.Helpers.Interfaces;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Factories;

public class MapFactory(IRoomDao roomDao, IMapHelper mapHelper) : IMapFactory
{
    private readonly IRoomDao _roomDao = roomDao;
    private readonly IMapHelper _mapHelper = mapHelper;

    public List<Room> GenerateMap(int level, bool campaign, bool randomMap)
    {
        var rooms = (campaign || randomMap)
            ? [.. _roomDao.GetAllRooms().Select(r => r.Clone())]
            : _roomDao.GetAllRooms()/*.Select(r => r.DeepClone())*/.ToList();

        var entrance = rooms.First(r => r.Name.Equals("Entrance", StringComparison.OrdinalIgnoreCase));
        var totalRoomsAllowed = Math.Min(level * 3, rooms.Count) + 2;

        if (campaign || randomMap)
        {
            rooms = _mapHelper.CreateCampaignMap(totalRoomsAllowed, entrance, rooms);
        }
        else
        {
            var unconnectedRooms = _mapHelper.FindUnconnectedRooms(entrance, rooms);
            rooms.RemoveAll(r => unconnectedRooms.Contains(r));
        }

        return rooms;
    }
}
