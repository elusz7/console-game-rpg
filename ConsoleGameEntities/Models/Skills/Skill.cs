using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Abilities
{
    public class Skill : ISkill
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public int Power { get; set; }
        public int ElapsedTime { get; set; } // time since skill was last used
        public int Cooldown { get; set; }
        public TargetType TargetType { get; set; }
        public SkillType SkillType { get; set; }

        public int? ArchetypeId { get; set; }
        public virtual Archetype Archetype { get; set; }

        public void Activate()
        {
            // check if skill is on cooldown
            //check if enough resource

            // use resource
            // apply skill effect

            // set elapsed time to cooldown
        }
        public void UpdateElapsedTime()
        {
            //subtract 1 from elapsed time
        }
    }
}
