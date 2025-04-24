using ConsoleGameEntities.Models.Rooms;

namespace ConsoleGame.Helpers;

public class MapManager
{
    private const int RoomNameLength = 5;
    private const int gridRows = 5;
    private const int gridCols = 5;
    private readonly OutputManager _outputManager;
    private readonly string[,] mapGrid;
    private Room _currentRoom;

    public MapManager(OutputManager outputManager)
    {
        _currentRoom = null;
        _outputManager = outputManager;
        mapGrid = new string[gridRows, gridCols];
    }

    public void DisplayMap()
    {
        _outputManager.WriteLine("Map:");

        for (var i = 0; i < gridRows; i++)
        {
            for (var j = 0; j < gridCols; j++)
            {
                mapGrid[i, j] = "       ";
            }
        }

        if (_currentRoom != null)
        {
            var startRow = gridRows / 2;
            var startCol = gridCols / 2;
            PlaceRoom(_currentRoom, startRow, startCol);
        }

        for (var i = 0; i < gridRows; i++)
        {
            for (var j = 0; j < gridCols; j++)
            {
                if (mapGrid[i, j].Contains("[") && mapGrid[i, j].Contains("]"))
                {
                    if (mapGrid[i, j] == $"[{_currentRoom.Name.Substring(0, RoomNameLength)}]")
                    {
                        _outputManager.Write($"{mapGrid[i, j],-7}", ConsoleColor.Green);
                    }
                    else
                    {
                        _outputManager.Write($"{mapGrid[i, j],-7}");
                    }
                }
                else
                {
                    _outputManager.Write($"{mapGrid[i, j],-7}");
                }
            }

            _outputManager.WriteLine("");
        }
        _outputManager.Display();
    }

    public void UpdateCurrentRoom(Room currentRoom)
    {
        _currentRoom = currentRoom;
    }

    private void PlaceRoom(Room room, int row, int col)
    {
        if (mapGrid[row, col] != "       ")
        {
            return;
        }

        var roomName = room.Name.Length > RoomNameLength
            ? room.Name.Substring(0, RoomNameLength)
            : room.Name.PadRight(RoomNameLength);

        if (room == _currentRoom)
        {
            mapGrid[row, col] = $"[{roomName}]";
        }
        else
        {
            mapGrid[row, col] = $"[{roomName}]";
        }

        if (room.North != null && row > 1)
        {
            mapGrid[row - 1, col] = "   |   ";
            PlaceRoom(room.North, row - 2, col);
        }

        if (room.South != null && row < gridRows - 2)
        {
            mapGrid[row + 1, col] = "   |   ";
            PlaceRoom(room.South, row + 2, col);
        }

        if (room.East != null && col < gridCols - 2)
        {
            mapGrid[row, col + 1] = "  ---  ";
            PlaceRoom(room.East, row, col + 2);
        }

        if (room.West != null && col > 1)
        {
            mapGrid[row, col - 1] = "  ---  ";
            PlaceRoom(room.West, row, col - 2);
        }
    }
}