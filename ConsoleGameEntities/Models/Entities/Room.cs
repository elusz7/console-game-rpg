using System.Text;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Monsters;

namespace ConsoleGameEntities.Models.Entities;

public class Room : IRoom
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int? NorthId { get; set; }
    public virtual Room? North { get; set; }
    public int? SouthId { get; set; }
    public virtual Room? South { get; set; }
    public int? WestId { get; set; }
    public virtual Room? West { get; set; }
    public int? EastId { get; set; }
    public virtual Room? East { get; set; }
    public virtual List<Monster> Monsters { get; set; } = new List<Monster>();

    public Room() { }
    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public Room Clone()
    {
        return new Room
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description
        };
    }

    public Room DeepClone()
    {
        return new Room
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            NorthId = this.NorthId,
            North = this.North,
            SouthId = this.SouthId,
            South = this.South,
            WestId = this.WestId,
            West = this.West,
            EastId = this.EastId,
            East = this.East
        };
    }

    public Dictionary<string, Room> GetConnections()
    {
        var connections = new Dictionary<string, Room>();
        if (North != null) connections.Add("North", North);
        if (South != null) connections.Add("South", South);
        if (East != null) connections.Add("East", East);
        if (West != null) connections.Add("West", West);
        return connections;
    }
}
