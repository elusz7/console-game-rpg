using ConsoleGameEntities.Interfaces.Attributes;

namespace ConsoleGameEntities.Interfaces;

public interface IMonster
{
    int Id { get; set; }
    string Name { get; set; }

    void Attack(ITargetable target);
}