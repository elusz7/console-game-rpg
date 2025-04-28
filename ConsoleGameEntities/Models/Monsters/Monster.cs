using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Models.Monsters;
public abstract class Monster : IMonster, ITargetable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Health { get; set; }
    public int AggressionLevel { get; set; }
    public string MonsterType { get; set; }

    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public abstract void Attack(ITargetable target);

    public void TakeDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Console.WriteLine($"{Name} has been defeated!");
        }
    }
}
