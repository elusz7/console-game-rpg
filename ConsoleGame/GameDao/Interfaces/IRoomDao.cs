using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsoleGame.GameDao.Interfaces;

public interface IRoomDao
{
    public void AddRoom(Room room);
    public void UpdateRoom(Room room);
    public void UpdateAllRooms(List<Room> rooms);
    public void DeleteRoom(Room room);
    public List<Room> GetAllRooms();
    public Room? FindRoomByName(string name);
    public bool RoomExists(string name);
    public Room GetEntrance();
    public List<Room> GetAllEditableRooms();
    public List<Room> GetAllDeletableRooms();
    public List<Room> GetAllRoomsMaxConnections(int maxConnections);
}
