using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CunningStrategy(IMonsterSkillSelector skillSelector) : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector = skillSelector;

    /*
     * CunningStrategy behavior:
     * - heal first if reallllly low
     * - Choose debuff based on recorded damage type taken (Martial or Magical).
     * - If no debuff used, buff self based on own damage type.
     * - If no buff, use damage skill.
     * - If no damage, heal if low
     * - Else, fallback to basic attack.
     */
    public override AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
    {
        var healthLost = monster.MaxHealth - monster.CurrentHealth;
        var emergencyHealThreshold = monster.MaxHealth * 0.25;

        if (monster.CurrentHealth <= emergencyHealThreshold)
        {
            var emergencyHeal = _skillSelector.GetHealingSkill(monster, healthLost);
            if (emergencyHeal != null)
            {
                var skillResult = emergencyHeal.Activate(monster);
                return monster.MakeBasicAttack(target, 1.1) with {SkillResult = skillResult};
            }
        }

        // Step 1: Try a debuff based on the type of damage received more
        StatType? targetStatToDebuff = monster.MoreMartialDamageTaken()
            ? StatType.Attack
            : StatType.Magic;

        var debuffSkill = _skillSelector.GetDebuffSkill(monster, targetStatToDebuff);
        if (debuffSkill != null)
        {
            var skillResult = debuffSkill.Activate(monster, target);
            return monster.MakeBasicAttack(target, 1.1) with {SkillResult = skillResult};
        }

        // Step 2: If no debuff used, try buffing own attack stat
        var selfBuffStat = monster.DamageType == DamageType.Magical
            ? StatType.Magic
            : StatType.Attack;

        var buffSkill = _skillSelector.GetBuffSkill(monster, selfBuffStat);
        if (buffSkill != null)
        {
            var skillResult = buffSkill.Activate(monster);
            return monster.MakeBasicAttack(target, 1.1) with {SkillResult = skillResult};
        }

        // Step 3: If still no support skill used, try a damage skill
        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            var skillResult = damageSkill.Activate(monster, target);
            
            if (skillResult.AttackResults.Count != 1)
                throw new InvalidOperationException(
                    "Monster damage skills must produce exactly one attack result."); //this shouldn't ever throw because of all the target validation
            
            var attackAction = skillResult.AttackResults[0];

            return attackAction with { SkillResult = skillResult };
        }

        // Step 4: Heal if possible, then perform a basic attack
        var healResult = _skillSelector.GetHealingSkill(monster, healthLost)?.Activate(monster);

        if (healResult != null)
        {
            return monster.MakeBasicAttack(target, 1.1) with {SkillResult = healResult};
        }
        
        return monster.MakeBasicAttack(target, 1.1);
    }
}