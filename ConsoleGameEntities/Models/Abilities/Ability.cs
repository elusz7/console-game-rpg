using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;

namespace ConsoleGameEntities.Models.Abilities
{
    public abstract class Ability : IAbility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AbilityType { get; set; }

        public abstract void Activate(IPlayer user, ITargetable target);
    }
}
