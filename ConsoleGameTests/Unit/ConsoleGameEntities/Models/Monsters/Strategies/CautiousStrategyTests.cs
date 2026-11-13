using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class CautiousStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesDebuffSkill_WhenAvailable()
    {
        var debuffSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_WhenAvailable()
    {
        var skill = new DummyAttackSkill();
        var skillSelector = new FakeSkillSelector(damageSkill: skill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        monster.Attack(player);

        Assert.IsTrue(skill.Activated);
        Assert.AreEqual(skill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_HealthUnderThreshold_UsesHealingSkill_WhenAvailable()
    {
        var healingSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 60
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_HealthUnderThreshold_NoHealingSkillAvailable_UsesBuffSkill_WhenAvailable()
    {
        var buffSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 60
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_HealthOverThreshold_UsesBuffSkill_WhenAvailable()
    {
        var buffSkill = new DummySupportSkill();
        var healingSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(healingSkill: healingSkill, buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WhenNoSkillsAvailable()
    {
        var healingSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var skillSelector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithoutElementalProperties_IsNeutral()
    {
        var skillSelector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire)]
    [DataRow(ElementType.Lightning)]
    [DataRow(ElementType.Ice)]
    [DataRow(ElementType.Nature)]
    [DataRow(ElementType.Radiance)]
    [DataRow(ElementType.Abyssal)]
    public void ExecuteAttack_BasicAttack_AllElements(ElementType element)
    {
        var skillSelector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: element, elementalPower: 3)
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithDebuffSkill_StillIncludesElementalDamage()
    {
        var debuffSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Lightning, elementalPower: 6)
        {
            Strategy = new CautiousStrategy(skillSelector)
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(6, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Lightning, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_At75PercentHealth_UsesExpectedBehavior()
    {
        // At exactly 75 HP (75% of 100), healing SHOULD activate because
        // the condition is CurrentHealth <= healingThreshold (75 <= 75 is true).
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 75,
            Strategy = new CautiousStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_DebuffSkillAvailable_DamageSkillAvailable_PrioritizesDebuff()
    {
        // When both a debuff skill and a damage skill are available,
        // the debuff skill should be prioritized over the damage skill.
        var debuffSkill = new DummySupportSkill();
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill, damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CautiousStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.IsFalse(damageSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_AppliesAfterCautiousMultiplier()
    {
        // Corrupted should reduce the standard damage AFTER the 0.8x cautious
        // multiplier is applied, and also reduce the elemental component.
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new CautiousStrategy(selector)
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.Attack(player);

        // Standard damage = round(50 * 0.8) - 10 = 40 - 10 = 30
        var expectedStandard = (int)Math.Round(monster.AttackPower * 0.8, MidpointRounding.AwayFromZero) - 10;
        Assert.AreEqual(expectedStandard, player.DamageTaken);
        // Elemental damage = max(0, 5 - 10) = 0
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }
}