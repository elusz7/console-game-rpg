using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

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

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);
        var player = new DummyPlayer();
        var skillSelector = new FakeSkillSelector();

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.5);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithoutElementalProperties_IsNeutral()
    {
        var monster = new DummyMonster();
        var player = new DummyPlayer();
        var skillSelector = new FakeSkillSelector();

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.5);
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
        var monster = new DummyMonster(attackElement: element, elementalPower: 4);
        var player = new DummyPlayer();
        var skillSelector = new FakeSkillSelector();

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }
}