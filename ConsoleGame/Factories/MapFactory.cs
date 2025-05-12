using ConsoleGame.GameDao;
using ConsoleGame.Managers;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Factories;

public class MapFactory(RoomDao roomDao)
{
    private readonly RoomDao _roomDao = roomDao;  

    public List<Room> GenerateMap(int level, bool campaign)
    {
        var allRooms = _roomDao.GetAllRooms();
        var rooms = allRooms.ToList(); // Make a copy to mutate safely
        var entrance = rooms.First(r => r.Name.Equals("Entrance"));

        if (campaign)
        {
            var totalRoomsAllowed = Math.Min(level * 3, rooms.Count) + 1;
            
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
