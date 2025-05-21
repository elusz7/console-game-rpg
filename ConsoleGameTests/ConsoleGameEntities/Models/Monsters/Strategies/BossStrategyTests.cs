using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class BossStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesBossSkill_WhenAvailable()
    {
        var bossSkill = new DummyBossSkill();
        var skillSelector = new FakeSkillSelector(bossSkill: bossSkill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(bossSkill.Activated);
        Assert.AreEqual(0, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesSupportSkill_WhenAvailable()
    {
        var supportSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(buffSkill: supportSkill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(supportSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDamageSkill_WhenAvailable()
    {
        var skill = new DummyAttackSkill();
        var skillSelector = new FakeSkillSelector(damageSkill: skill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(skill.Activated);
        Assert.AreEqual(skill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_NoSkillsAvailable()
    {
        var skillSelector = new FakeSkillSelector();
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }
}