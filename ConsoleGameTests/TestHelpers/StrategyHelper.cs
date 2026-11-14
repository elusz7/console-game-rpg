using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.TestHelpers;

public static class StrategyHelper
{
    public class DummyMonster : Monster
    {
        public DummyMonster(ElementType? attackElement = null, int elementalPower = 0)
        {
            AttackPower = 50;
            CurrentHealth = 100;
            MaxHealth = 100;
            AttackElement = attackElement;
            ElementalPower = elementalPower;
        }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            return Strategy.ExecuteAttack(this, target);
        }

        public override bool MoreMartialDamageTaken() => TookMoreMartial;
        public bool TookMoreMartial { get; set; } = true;
    }

    public class DummyBossMonster : BossMonster
    {
        public DummyBossMonster(IMonsterSkillSelector skillSelector) : base(skillSelector)
        {
            AttackPower = 50;
        }

        public override AttackResolutionResult Attack(IPlayer target)
        {
            return Strategy.ExecuteAttack(this, target);
        }
    }

    public class DummyPlayer : Player
    {
        public int DamageTaken { get; private set; }
        public int ElementalDamageTaken { get; private set; }
        public ElementType? ElementDealt { get; private set; }

        public override AttackResolutionResult TakeDamage(AttackDamage attack)
        {
            DamageTaken += attack.StandardDamage;
            ElementalDamageTaken += attack.ElementalDamage;
            ElementDealt = attack.Element;
            CurrentHealth -= attack.StandardDamage + attack.ElementalDamage;
            var result = new AttackResolutionResult(
                attack.StandardDamage,
                attack.StandardDamage,
                attack.DamageType,
                attack.ElementalDamage,
                attack.ElementalDamage,
                attack.Element,
                false,
                false);
            if (CurrentHealth <= 0)
                result = result with { TargetDied = true };
            return result;
        }
    }

    public class DummyAttackSkill : MartialSkill
    {
        public DummyAttackSkill() => Power = 20;
        public bool Activated { get; private set; } = false;

        public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null,
            List<ITargetable>? multipleEnemies = null)
        {
            Activated = true;
            var attack = new AttackDamage(Power, ModelEnums.DamageType.Martial, 0, null);
            var result = singleEnemy?.TakeDamage(attack);
            return new SkillResolutionResult(this, result != null ? [result] : []);
        }
    }

    public class DummySupportSkill : SupportSkill
    {
        public bool Activated { get; private set; } = false;

        public override SkillResolutionResult Activate(ITargetable caster, ITargetable? singleEnemy = null,
            List<ITargetable>? multipleEnemies = null)
        {
            Activated = true;
            return new SkillResolutionResult(this, []);
        }
    }

    public class DummyUltimateSkill : UltimateSkill
    {
        public bool Activated { get; private set; } = false;

        public override SkillResolutionResult Activate(
            ITargetable caster,
            ITargetable? singleEnemy = null,
            List<ITargetable>? multipleEnemies = null)
        {
            Activated = true;

            var attack = new AttackDamage(
                Power,
                ModelEnums.DamageType.Martial,
                0,
                null);

            var result = singleEnemy?.TakeDamage(attack);

            return new SkillResolutionResult(
                this,
                result != null ? [result] : []);
        }
    }

    public class DummyBossSkill : BossSkill
    {
        public bool Activated { get; private set; } = false;

        public override SkillResolutionResult Activate(
            ITargetable caster,
            ITargetable? singleEnemy = null,
            List<ITargetable>? multipleEnemies = null)
        {
            Activated = true;

            var attack = new AttackDamage(
                Power,
                ModelEnums.DamageType.Martial,
                0,
                null);

            var result = singleEnemy?.TakeDamage(attack);

            return new SkillResolutionResult(
                this,
                result != null ? [result] : []);
        }
    }

    public class DummyFallbackStrategy : IMonsterStrategy
    {
        public bool AttackExecuted { get; private set; } = false;

        public AttackResolutionResult ExecuteAttack(IMonster monster, IPlayer target)
        {
            AttackExecuted = true;
            // Optionally, you can simulate some effect on target
            return new AttackResolutionResult(0, 0, DamageType.Martial, null, 0, null, false, false);
        }
    }

    public class FakeSkillSelector : MonsterSkillSelector
    {
        private readonly Skill? _damageSkill;
        private readonly SupportSkill? _debuffSkill;
        private readonly SupportSkill? _healingSkill;
        private readonly SupportSkill? _buffSkill;
        private readonly BossSkill? _bossSkill;
        private readonly UltimateSkill? _ultimateSkill;

        public FakeSkillSelector(Skill? damageSkill = null, SupportSkill? debuffSkill = null,
            SupportSkill? healingSkill = null, SupportSkill? buffSkill = null,
            BossSkill? bossSkill = null, UltimateSkill? ultimateSkill = null)
        {
            _damageSkill = damageSkill;
            _debuffSkill = debuffSkill;
            _healingSkill = healingSkill;
            _buffSkill = buffSkill;
            _bossSkill = bossSkill;
            _ultimateSkill = ultimateSkill;
        }

        public override Skill? GetHighestDamageSkill(IMonster monster) => _damageSkill;
        public override BossSkill? GetStrongestBossSkill(IMonster monster) => _bossSkill;
        public override SupportSkill? GetDebuffSkill(IMonster monster, StatType? affectedStat = null) => _debuffSkill;
        public override SupportSkill? GetHealingSkill(IMonster monster, int desiredHealingPower) => _healingSkill;
        public override SupportSkill? GetBuffSkill(IMonster monster, StatType? affectedStat = null) => _buffSkill;

        public override List<SupportSkill> GetSupportSkills(IMonster monster)
        {
            if (_buffSkill == null) return [];

            return [_buffSkill];
        }

        public override UltimateSkill? GetUltimateSkill(IMonster monster) => _ultimateSkill;
    }
}