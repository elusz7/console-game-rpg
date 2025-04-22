using ConsoleGameEntities.Models.Attributes;
using ConsoleGameEntities.Models.Characters;

namespace ConsoleGameEntities.Models.Abilities
{
    public class ShoveAbility : Ability
    {
        public int Damage { get; set; }
        public int Distance { get; set; }

        public override void Activate(IPlayer user, ITargetable target)
        {
            // Fireball ability logic
            Console.WriteLine($"{user.Name} shoves {target.Name} back {Distance} feet, dealing {Damage} damage!");
            target.TakeDamage(Damage);
        }
    }
}
