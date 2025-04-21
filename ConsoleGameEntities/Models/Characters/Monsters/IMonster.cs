using ConsoleGameEntities.Models.Attributes;

namespace ConsoleGameEntities.Models.Characters.Monsters;

public interface IMonster
{
    int Id { get; set; }
    string Name { get; set; }

    void Attack(ITargetable target);
}