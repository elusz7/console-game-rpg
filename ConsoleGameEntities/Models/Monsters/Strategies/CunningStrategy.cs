using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CunningStrategy : DefaultStrategy
{
    private readonly IMonsterSkillSelector _skillSelector;

    public CunningStrategy(IMonsterSkillSelector skillSelector)
    {
        _skillSelector = skillSelector;
    }

    /*
     * CunningStrategy behavior:
     * - heal first if reallllly low
     * - Choose debuff based on recorded damage type taken (Martial or Magical).
     * - If no debuff used, buff self based on own damage type.
     * - If no buff, use damage skill.
     * - If no damage, heal if low
     * - Else, fallback to basic attack.
    */
    public override void ExecuteAttack(IMonster monster, IPlayer target)
    {        
        var healthLost = monster.MaxHealth - monster.CurrentHealth;
        var emergencyHealThreshold = monster.MaxHealth * 0.25;

        if (monster.CurrentHealth <= emergencyHealThreshold)
        {
            var emergencyHeal = _skillSelector.GetHealingSkill(monster, healthLost);
            if (emergencyHeal != null)
            {
                emergencyHeal.Activate(monster);
                MakeAttack(monster, target);
                return;
            }
        }

        // Step 1: Try a debuff based on the type of damage received more
        StatType? targetStatToDebuff = monster.MoreMartialDamageTaken()
                ? StatType.Attack
                : StatType.Magic;

        var debuffSkill = _skillSelector.GetDebuffSkill(monster, targetStatToDebuff);
        if (debuffSkill != null)
        {
            debuffSkill.Activate(monster, target);
            MakeAttack(monster, target);
            return;
        }

        // Step 2: If no debuff used, try buffing own attack stat
        StatType selfBuffStat = monster.DamageType == DamageType.Magical
                ? StatType.Magic
                : StatType.Attack;

        var buffSkill = _skillSelector.GetBuffSkill(monster, selfBuffStat);
        if (buffSkill != null)
        {
            buffSkill.Activate(monster);
            MakeAttack(monster, target);
            return;
        }

        // Step 3: If still no support skill used, try a damage skill
        var damageSkill = _skillSelector.GetHighestDamageSkill(monster);
        if (damageSkill != null)
        {
            damageSkill.Activate(monster, target);
            return;
        }

        // Step 3: Heal
        _skillSelector.GetHealingSkill(monster, healthLost);
        MakeAttack(monster, target);
    }

    private static void MakeAttack(IMonster monster, IPlayer target)
    {
        var adjustedDamage = (int)Math.Ceiling(monster.GetStat(StatType.Attack) * 1.1); // 10% stronger basic hit
        monster.AddActionItem($"{monster.Name} attacks for {adjustedDamage} damage!");
        target.TakeDamage(adjustedDamage, monster.DamageType);
    }
}