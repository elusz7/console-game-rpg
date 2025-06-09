using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

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
        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.2);
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
            CurrentHealth = 40,  // Below 67% of 100 max health (67)
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated, "Healing skill should activate when health is below threshold");
        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.2);
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
            CurrentHealth = 70,  // Above 67 threshold
            Strategy = new OffensiveStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated, "Healing skill should NOT activate when health is at or above threshold");
        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.2);
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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.2);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }
}
