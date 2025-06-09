using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class DefaultStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesHealingSkill_WhenHealthIsLow()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 30,
            Strategy = new DefaultStrategy(selector)
        };

        var expectedDamage = monster.AttackPower;
        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesBuffSkill_WhenHealthAboveThreshold()
    {
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 75,
            Strategy = new DefaultStrategy(selector)
        };

        var expectedDamage = monster.AttackPower;
        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDebuffSkill_WhenHealthInMiddle()
    {
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 45,
            Strategy = new DefaultStrategy(selector)
        };

        var expectedDamage = monster.AttackPower;
        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_WhenNoSupportSkillsAvailable()
    {
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(damageSkill.Activated);
        Assert.AreEqual(damageSkill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_FallsBackToBasicAttack_WhenNoSkillsAvailable()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefaultStrategy(selector)
        };

        var expectedDamage = monster.AttackPower;
        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_ThrowsException_IfSkillSelectorIsNull()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefaultStrategy() // no selector passed
        };

        var ex = Assert.ThrowsException<InvalidOperationException>(() => monster.Attack(player));
        StringAssert.Contains(ex.Message, "Skill selector is null");
    }
}
