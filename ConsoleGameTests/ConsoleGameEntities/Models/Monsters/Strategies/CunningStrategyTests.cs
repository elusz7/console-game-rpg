using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.ConsoleGameEntities.Models.Monsters.Strategies;

[TestClass]
public class CunningStrategyTests
{
    [TestMethod]
    public void ExecuteAttack_UsesEmergencyHeal_WhenHealthVeryLow()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 20,
            Strategy = new CunningStrategy(selector)
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.1);
        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesDebuff_BasedOnDamageType()
    {
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90,
            TookMoreMartial = true
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.1);
        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_UsesBuff_WhenNoDebuffAvailable()
    {
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90,
            DamageType = DamageType.Martial
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.1);
        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
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
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
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
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Ceiling(monster.AttackPower * 1.1);
        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }
}

