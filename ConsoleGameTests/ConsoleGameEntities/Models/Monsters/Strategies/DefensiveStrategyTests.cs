using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

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
}
