using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers.Interfaces;

public interface IMapHelper
{
    public List<Room> CreateCampaignMap(int totalRoomsAllowed, Room entrance, List<Room> availableRooms);
    public void UnlinkRoom(Room room);
    public void UnlinkDirection(Room from, Room to, string direction);
    public void LinkRooms(Room from, Room to, string direction);
    public List<string> GetAvailableDirectionsForRoom(Room room, List<Room> connectedRooms);
    public Dictionary<(int x, int y), Room> GetRoomGrid(Room rootRoom);
    public (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid);
    public (int dx, int dy) GetDirectionOffset(string direction);
    public bool HasReverseConflict(Room baseRoom, string direction, List<Room> rooms);
    public List<Room> FindUnconnectedRooms(Room rootRoom, List<Room> rooms);
    public Room? GetNeighbor(Room room, string direction);
    public string GetReverseDirection(string direction);
    public int CountPathsToEntranceThroughRoom(Room startRoom, Room requiredRoom, Room entrance);
    public List<Room>? SinglePathThrough(Room requiredRoom, List<Room> rooms);
    public Dictionary<Room, List<string>> GetAvailableDirections2(List<Room> rooms, Room? editingRoom = null);
    public Dictionary<string, string> FindUnlinkedNeighbors(List<Room> rooms);
    (string name, string direction) ParseKey(string key);
    public List<Room> CheckForDisconnectedRooms(List<Room> rooms);
}
