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
    public virtual List<Monster>? Monsters { get; set; } = new List<Monster>();

    public Room() { }
    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        sb.Append($"{Name}: {Description}");

        if (North != null) sb.Append($"\n\tNorth: {North.Name}");
        if (South != null) sb.Append($"\n\tSouth: {South.Name}");
        if (East != null) sb.Append($"\n\tEast: {East.Name}");
        if (West != null) sb.Append($"\n\tWest: {West.Name}");

        return sb.ToString();
    }
}
