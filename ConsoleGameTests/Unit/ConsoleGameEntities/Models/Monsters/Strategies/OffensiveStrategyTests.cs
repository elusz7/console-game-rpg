using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class OffensiveStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_WhenAvailable()
    {
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(damageSkill.Activated);
        Assert.AreEqual(damageSkill.Power, player.DamageTaken, "Damage should be from skill, not basic attack");
    }

    [TestMethod]
    public void ExecuteAttack_UsesBuffSkillAndThenAttacks_WhenNoDamageSkill()
    {
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        // Damage is from basic attack with 1.2x multiplier
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesHealingSkill_WhenHealthBelowThreshold()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 40, // Below 67% of 100 max health (67)
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated, "Healing skill should activate when health is below threshold");
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_DoesNotUseHealingSkill_WhenHealthAtOrAboveThreshold()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 70, // Above 67 threshold
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated,
            "Healing skill should NOT activate when health is at or above threshold");
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_MakesBasicAttack_WhenNoSkillsAvailable()
    {
        var selector = new FakeSkillSelector(); // No skills
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithoutElementalProperties_IsNeutral()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
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
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: element, elementalPower: 3)
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithBuffSkill_StillIncludesElementalDamage()
    {
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Ice, elementalPower: 4)
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Ice, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_At67PercentHealth_UsesExpectedBehavior()
    {
        // At exactly 67 HP (67% of 100), healing should NOT activate because
        // the condition is CurrentHealth < healingThreshold (67 < 67 is false).
        // The strategy falls through to the basic attack path.
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 67,
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_DamageSkillAvailable_DoesNotPerformBasicAttack()
    {
        // When a damage skill is available, it should activate and return
        // without also performing a basic attack.
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(damageSkill.Activated);
        // Player damage should equal only the skill's power, not skill + basic attack
        Assert.AreEqual(damageSkill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_AppliesAfterOffensiveMultiplier()
    {
        // Corrupted should reduce the standard damage AFTER the 1.2x offensive
        // multiplier is applied, and also reduce the elemental component.
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new OffensiveStrategy(selector)
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.Attack(player);

        // Standard damage = round(50 * 1.2) - 10 = 60 - 10 = 50
        var expectedStandard = (int)Math.Round(monster.AttackPower * 1.2, MidpointRounding.AwayFromZero) - 10;
        Assert.AreEqual(expectedStandard, player.DamageTaken);
        // Elemental damage = max(0, 5 - 10) = 0
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }
}