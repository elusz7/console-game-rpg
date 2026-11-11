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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

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
    public void ExectueAttack_HealthUnderThreshold_UsesHealingSkill_WhenAvailable()
    {
        var healingSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 60
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExectueAttack_HealthUnderThreshold_NoHealingSkillAvailable_UsesBuffSkill_WhenAvailable()
    {
        var buffSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 60
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExectueAttack_HealthOverThreshold_UsesBuffSkill_WhenAvailable()
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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExectueAttack_BasicAttack_WhenNoSkillsAvailable()
    {
        var healingSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new CautiousStrategy(skillSelector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

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

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 0.8);

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(6, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Lightning, player.ElementDealt);
    }
}