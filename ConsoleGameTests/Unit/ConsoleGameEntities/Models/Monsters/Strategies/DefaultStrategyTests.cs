using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

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

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithoutElementalProperties_IsNeutral()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster()
        {
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_BasicAttack_ElementalPowerScalesWithTier()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 10)
        {
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(10, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
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
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: element, elementalPower: 3)
        {
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithBuffSkill_StillIncludesElementalDamage()
    {
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Ice, elementalPower: 4)
        {
            CurrentHealth = 75,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Ice, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithDebuffSkill_StillIncludesElementalDamage()
    {
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Lightning, elementalPower: 6)
        {
            CurrentHealth = 45,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(6, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Lightning, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithHealingSkill_StillIncludesElementalDamage()
    {
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Nature, elementalPower: 7)
        {
            CurrentHealth = 30,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
        Assert.AreEqual(7, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Nature, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_At33PercentHealth_UsesExpectedBehavior()
    {
        // At exactly 33 HP (33% of 100), healing should NOT activate because
        // the condition is CurrentHealth < healingThreshold (33 < 33 is false).
        // The strategy falls through to the debuff path.
        var debuffSkill = new DummySupportSkill();
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill, healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 33,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_At50PercentHealth_UsesExpectedBehavior()
    {
        // At exactly 50 HP (50% of 100), buff should NOT activate because
        // the condition is CurrentHealth > buffThreshold (50 > 50 is false).
        // The strategy falls through to the debuff path.
        var debuffSkill = new DummySupportSkill();
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill, buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 50,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(buffSkill.Activated);
        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_HealingSkillAvailable_AboveThreshold_DoesNotActivateHealing()
    {
        // When healing skill is available but health is above the healing threshold,
        // the healing skill should NOT activate.
        var healingSkill = new DummySupportSkill();
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill, healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 50,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsFalse(healingSkill.Activated);
        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_SupportSkillAvailable_BeforeDamageSkill_PrioritizesSupport()
    {
        // When both a support skill and a damage skill are available at high health,
        // the support skill (buff) should be prioritized over the damage skill.
        var buffSkill = new DummySupportSkill();
        var damageSkill = new DummyAttackSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill, damageSkill: damageSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 75,
            Strategy = new DefaultStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        Assert.IsFalse(damageSkill.Activated);
        Assert.AreEqual(monster.AttackPower, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_ReducesBasicAttackDamage()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new DefaultStrategy(selector)
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.Attack(player);

        Assert.AreEqual(monster.AttackPower - 10, player.DamageTaken);
    }
}