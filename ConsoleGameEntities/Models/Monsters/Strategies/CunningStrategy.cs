using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleGameEntities.Helpers;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Monsters.Strategies;

public class CunningStrategy : DefaultStrategy
{
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
        bool supportUsed = false;
        int healthLost = monster.MaxHealth - monster.CurrentHealth;

        if (healthLost < monster.MaxHealth / 4)
        {
            var emergencyHeal = MonsterSkillHelper.GetClosestHealingSkill(monster, healthLost);
            if (emergencyHeal != null)
            {
                emergencyHeal.Activate(monster);
                supportUsed = true;
            }
        }

        // Step 1: Try a debuff based on the type of damage received more
        if (!supportUsed)
        {
            StatType? targetStatToDebuff = ((Monster)monster).MoreMartialDamageTaken
                ? StatType.Attack
                : StatType.Magic;

            var debuffSkill = MonsterSkillHelper.GetDebuffSkill(monster, targetStatToDebuff);
            if (debuffSkill != null)
            {
                debuffSkill.Activate(monster, target);
                supportUsed = true;
            }
        }

        // Step 2: If no debuff used, try buffing own attack stat
        if (!supportUsed)
        {
            StatType selfBuffStat = monster.DamageType == DamageType.Magical
                ? StatType.Magic
                : StatType.Attack;

            var buffSkill = MonsterSkillHelper.GetBuffSkill(monster, selfBuffStat);
            if (buffSkill != null)
            {
                buffSkill.Activate(monster);
                supportUsed = true;
            }
        }

        // Step 3: If still no support skill used, try a damage skill
        if (!supportUsed)
        {
            var damageSkill = MonsterSkillHelper.GetHighestDamageSkill(monster);
            if (damageSkill != null)
            {
                damageSkill.Activate(monster, target);
                return;
            }
        }

        // Step 3: Heal
        if (!supportUsed && healthLost > 0)
        {
            MonsterSkillHelper.GetClosestHealingSkill(monster, healthLost);
        }

        // Step 4: Fallback - basic attack (slightly stronger?)
        var adjustedDamage = (int)Math.Ceiling(monster.AttackPower * 1.1); // 10% stronger basic hit
        monster.AddActionItem($"{monster.Name} attacks for {adjustedDamage} damage!");
        target.TakeDamage(adjustedDamage, monster.DamageType);
    }
}