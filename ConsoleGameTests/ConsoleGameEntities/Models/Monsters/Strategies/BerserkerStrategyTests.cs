using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class BerserkerStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_WhenAvailable()
    {
        var monster = new DummyMonster();
        var player = new DummyPlayer();
        var skill = new DummyAttackSkill();
        var skillSelector = new FakeSkillSelector(damageSkill: skill);

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        Assert.IsTrue(skill.Activated);
        Assert.AreEqual(skill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_FallsBackToAttack_WhenNoSkill()
    {
        var monster = new DummyMonster();
        var player = new DummyPlayer();
        var skillSelector = new FakeSkillSelector();

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.5);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.IsTrue(monster.ActionItems.Values.Any(a => a.Contains($"attacks for {expectedDamage} damage")));
    }
}
