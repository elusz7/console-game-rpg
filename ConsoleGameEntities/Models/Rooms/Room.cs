using ConsoleGameEntities.Models.Characters;
using ConsoleGameEntities.Models.Characters.Monsters;

namespace ConsoleGameEntities.Models.Rooms;

public class Room
{
    public int Id { get; set; }
    public virtual ICollection<Monster> Monsters { get; set; } = new List<Monster>();
    public string Name { get; set; }
    public string Description { get; set; }

    public int? PlayerId { get; set; }
    public virtual Player? Player { get; set; }
    public int? NorthId { get; set; }
    public virtual Room? North { get; set; }
    public int? SouthId { get; set; }
    public virtual Room? South { get; set; }
    public int? WestId { get; set; }
    public virtual Room? West { get; set; }
    public int? EastId { get; set; }
    public virtual Room? East { get; set; }

    public Room() { }
    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public void Enter()
    {
        /*_outputManager.WriteLine($"You have entered {Name}. {Description}", ConsoleColor.Green);
        foreach (var character in Characters)
        {
            _outputManager.WriteLine($"{character.Name} is here.", ConsoleColor.Red);
        }*/
    }

    public void AddMonster(Monster monster)
    {
        Monsters.Add(monster);
    }

    public void RemoveMonster(Monster monster)
    {
        Monsters.Remove(monster);
    }
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();

        sb.Append($"{Name}: {Description}");

        if (Monsters.Any())
        {
            sb.Append($"\n\tMonsters: {String.Join(", ", Monsters.Select(m => m.Name))}");
        }

        if (North != null) sb.Append($"\n\tNorth: {North.Name}");
        if (South != null) sb.Append($"\n\tSouth: {South.Name}");
        if (East != null) sb.Append($"\n\tEast: {East.Name}");
        if (West != null) sb.Append($"\n\tWest: {West.Name}");

        return sb.ToString();
    }
}
