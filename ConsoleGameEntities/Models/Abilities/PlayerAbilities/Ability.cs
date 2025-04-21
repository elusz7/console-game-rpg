using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Characters;

namespace ConsoleGameEntities.Models.Abilities.PlayerAbilities
{
    public abstract class Ability : IAbility
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AbilityType { get; set; }

        public virtual IEnumerable<Player> Players { get; set; }

        protected Ability()
        {
            Players = new List<Player>();
        }

        public abstract void Activate(IPlayer user, ITargetable target);
    }
}
