namespace ConsoleGameEntities.Models.Attributes;

public interface ITargetable
{
    string Name { get; set; }

    void TakeDamage(int damage);
}
