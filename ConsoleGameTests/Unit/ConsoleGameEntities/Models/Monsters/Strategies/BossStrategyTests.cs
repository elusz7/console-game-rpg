using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

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

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var skillSelector = new FakeSkillSelector();
        var monster = new DummyBossMonster(skillSelector)
        {
            AttackElement = ElementType.Fire,
            ElementalPower = 5
        };
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithoutElementalProperties_IsNeutral()
    {
        var skillSelector = new FakeSkillSelector();
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
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
        var monster = new DummyBossMonster(skillSelector)
        {
            AttackElement = element,
            ElementalPower = 3
        };
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithSupportSkill_StillIncludesElementalDamage()
    {
        var supportSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(buffSkill: supportSkill);
        var monster = new DummyBossMonster(skillSelector)
        {
            AttackElement = ElementType.Ice,
            ElementalPower = 4
        };
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(supportSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Ice, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_BossSkillAvailable_DoesNotPerformBasicAttack()
    {
        // When a boss skill is available, it should activate and return
        // without also performing a basic attack.
        var bossSkill = new DummyBossSkill();
        var skillSelector = new FakeSkillSelector(bossSkill: bossSkill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(bossSkill.Activated);
        // No basic attack after boss skill — player takes no damage from basic attack
        Assert.AreEqual(0, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_DamageSkillAvailable_DoesNotPerformBasicAttack()
    {
        // When a damage skill is available (no boss or support skills),
        // it should activate and return without also performing a basic attack.
        var damageSkill = new DummyAttackSkill();
        var skillSelector = new FakeSkillSelector(damageSkill: damageSkill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(damageSkill.Activated);
        // Player damage should equal only the skill's power, not skill + basic attack
        Assert.AreEqual(damageSkill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_SupportSkillAvailable_UsesSupportSkillAndBasicAttack()
    {
        // When a support skill is available (no boss skill), the support skill
        // should activate and then a basic attack should follow.
        var supportSkill = new DummySupportSkill();
        var skillSelector = new FakeSkillSelector(buffSkill: supportSkill);
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.IsTrue(supportSkill.Activated);
        // Basic attack follows support skill
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_NoSkillsAvailable_PerformsBasicAttack()
    {
        // When no skills are available, the boss should fall back to a basic attack.
        var skillSelector = new FakeSkillSelector();
        var monster = new DummyBossMonster(skillSelector);
        var player = new DummyPlayer();

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }
}