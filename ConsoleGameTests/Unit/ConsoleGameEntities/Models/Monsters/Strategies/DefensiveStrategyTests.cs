using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class DefensiveStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesHealingSkill_WhenNotAtFullHealth()
    {
        var healingSkill = new DummySupportSkill();
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(damageSkill: damageSkill, healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 80,
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated, "Healing skill should be activated");
        Assert.IsFalse(damageSkill.Activated, "Damage skill should NOT be activated when healing");
        Assert.AreEqual((int)(monster.AttackPower * 0.8), player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_IfNoHealingNeeded()
    {
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(damageSkill.Activated);
        Assert.AreEqual(damageSkill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDebuffSkill_IfNoHealingOrDamageSkill()
    {
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual((int)(monster.AttackPower * 0.8), player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesFallbackAttack_IfNoSkillsAvailable()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual((int)(monster.AttackPower * 0.8), player.DamageTaken);
    }

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8);
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
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8);
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
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithHealingSkill_StillIncludesElementalDamage()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Ice, elementalPower: 4)
        {
            CurrentHealth = 80,
            Strategy = new DefensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 0.8);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Ice, player.ElementDealt);
    }
}