using System.ComponentModel.DataAnnotations.Schema;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameEntities.Models.Skills;

public class SupportSkill : Skill
{

    public int Duration { get; set; }
    public StatType StatAffected { get; set; }
    public SupportEffectType Effect { get; set; }
    [NotMapped]
    public Dictionary<ITargetable, int> TargetsAffected { get; set; } = new Dictionary<ITargetable, int>();

    public override bool Activate(ITargetable? caster, List<ITargetable>? targets)
    {
        if (IsOnCooldown)
            return false;

        if (caster is Player self)
        {
            try
            {
                self.Archetype.UseResource(Cost);
            }
            catch (InvalidOperationException) { return false; }

            if (TargetType == TargetType.Self)
                PlayerBoost(self);
            else if (TargetType == TargetType.SingleEnemy || TargetType == TargetType.RandomEnemy)
            {
                if (targets == null || targets.Count == 0) return false;
                
                MonsterReduction((Monster)targets[0]);
            }
            else if (TargetType == TargetType.AllEnemies)
            {
                if (targets == null || targets.Count == 0) return false;
                foreach (var target in targets)
                    MonsterReduction((Monster)target);
            }
            else { return false; }

            ElapsedTime = 0;
            return true;
        }
        else if (caster is Monster monster)
        {
            if (TargetType == TargetType.Self)
                MonsterBoost(monster);
            else if (TargetType == TargetType.SingleEnemy)
            {
                if (targets == null || targets.Count == 0) return false;
                PlayerReduction((Player)targets[0]);
            }
            else { return false; }

            ElapsedTime = 0;
            return true;
        }

        return false;
    }
    public override void UpdateElapsedTime()
    {
        ElapsedTime++;
        if (ElapsedTime >= Duration)
            RevertEffect();
    }

    public override void Reset()
    {
        RevertEffect();
        ElapsedTime = 0;
    }

    private void RevertEffect()
    {
        if (TargetsAffected.Count == 0) return;

        foreach (var kvp in TargetsAffected)
        {
            var target = kvp.Key;
            var originalStat = kvp.Value;

            if (Effect == SupportEffectType.Boost)
                target.ReduceStat(StatAffected, Power, originalStat);
            else
                target.BoostStat(StatAffected, Power, originalStat);
        }

        TargetsAffected.Clear();
    }

    private void PlayerBoost(Player p)
    {
        TargetsAffected[p] = p.GetStat(StatAffected); // Store the original stat
        p.BoostStat(StatAffected, Power);
        Effect = SupportEffectType.Boost;
        ElapsedTime = 0;
    }
    private void PlayerReduction(Player p)
    {
        TargetsAffected[p] = p.GetStat(StatAffected); // Store the original stat
        p.ReduceStat(StatAffected, Power);
        Effect = SupportEffectType.Reduce;
        ElapsedTime = 0;
    }
    private void MonsterBoost(Monster m)
    {
        TargetsAffected[m] = m.GetStat(StatAffected); // Store the original stat
        m.BoostStat(StatAffected, Power);
        Effect = SupportEffectType.Boost;
        ElapsedTime = 0;
    }
    private void MonsterReduction(Monster m)
    {
        TargetsAffected[m] = m.GetStat(StatAffected); // Store the original stat
        m.ReduceStat(StatAffected, Power);
        Effect = SupportEffectType.Reduce;
        ElapsedTime = 0;
    }
}
