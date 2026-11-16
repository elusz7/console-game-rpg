using ConsoleGameEntities.Models.Entities;

namespace ConsoleGame.Helpers.Interfaces;

public interface IMapHelper
{
    List<Room> CreateCampaignMap(int totalRoomsAllowed, Room entrance, List<Room> availableRooms);
    void UnlinkRoom(Room room);
    void UnlinkDirection(Room from, Room to, string direction);
    void LinkRooms(Room from, Room to, string direction);
    List<string> GetAvailableDirectionsForRoom(Room room, List<Room> connectedRooms);
    Dictionary<(int x, int y), Room> GetRoomGrid(Room rootRoom);
    (int x, int y) FindRoomCoordinates(Room room, Dictionary<(int x, int y), Room> grid);
    (int dx, int dy) GetDirectionOffset(string direction);
    bool HasReverseConflict(Room baseRoom, string direction, List<Room> rooms);
    List<Room> FindUnconnectedRooms(Room rootRoom, List<Room> rooms);
    Room? GetNeighbor(Room room, string direction);
    string GetReverseDirection(string direction);
    Dictionary<Room, List<string>> GetAvailableDirections2(List<Room> rooms, Room? editingRoom = null);
    Dictionary<string, string> FindUnlinkedNeighbors(List<Room> rooms);
    (string name, string direction) ParseKey(string key);
    List<Room> CheckForDisconnectedRooms(List<Room> rooms);
    List<Room> FindRoomsDisconnectedByMove(Room entrance, Room roomToMove, Room newParent, string direction,
        List<Room> rooms);
}