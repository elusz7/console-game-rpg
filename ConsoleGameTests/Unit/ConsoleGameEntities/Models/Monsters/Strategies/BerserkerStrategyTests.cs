using ConsoleGameEntities.Models.Combat.ActiveEffects;
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

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.5, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.IsTrue(monster.ActionItems.Values.Any(
            a => a.Contains($"attacks for {expectedDamage} {monster.DamageType} damage!")));
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

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_DamageSkillAvailable_DoesNotPerformBasicAttack()
    {
        // When a damage skill is available, it should activate and return
        // without also performing a basic attack.
        var monster = new DummyMonster();
        var player = new DummyPlayer();
        var skill = new DummyAttackSkill();
        var skillSelector = new FakeSkillSelector(damageSkill: skill);

        monster.Strategy = new BerserkerStrategy(skillSelector);
        monster.Attack(player);

        Assert.IsTrue(skill.Activated);
        // Player damage should equal only the skill's power, not skill + basic attack
        Assert.AreEqual(skill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_AppliesAfterBerserkerMultiplier()
    {
        // Corrupted should reduce the standard damage AFTER the 1.5x berserker
        // multiplier is applied, and also reduce the elemental component.
        // This is the most useful Corrupted integration test because Berserker
        // has the largest basic-attack multiplier.
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);
        var player = new DummyPlayer();
        var skillSelector = new FakeSkillSelector();

        monster.Strategy = new BerserkerStrategy(skillSelector);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.Attack(player);

        // Standard damage = round(50 * 1.5) - 10 = 75 - 10 = 65
        var expectedStandard = (int)Math.Round(monster.AttackPower * 1.5, MidpointRounding.AwayFromZero) - 10;
        Assert.AreEqual(expectedStandard, player.DamageTaken);
        // Elemental damage = max(0, 5 - 10) = 0
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }
}