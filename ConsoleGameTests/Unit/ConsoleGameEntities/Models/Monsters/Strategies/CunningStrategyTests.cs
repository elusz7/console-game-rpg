using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Monsters.Strategies;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

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

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
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

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
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

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
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

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    // ── Elemental Damage ──

    [TestMethod]
    public void ExecuteAttack_BasicAttack_WithElementalDamage()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        monster.Attack(player);

        Assert.AreEqual(expectedDamage, player.DamageTaken);
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
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
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
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: element, elementalPower: 3)
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        monster.Attack(player);

        Assert.AreEqual(3, player.ElementalDamageTaken);
        Assert.AreEqual(element, player.ElementDealt);
    }

    [TestMethod]
    public void ExecuteAttack_WithDebuffSkill_StillIncludesElementalDamage()
    {
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Ice, elementalPower: 4)
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90,
            TookMoreMartial = true
        };

        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
        Assert.AreEqual(4, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Ice, player.ElementDealt);
    }
    
    //verify corrupted reduces damage
    [TestMethod]
    public void ExecuteAttack_WithCorruptedEffect_ReducesBothDamageComponents()
    {
        var selector = new FakeSkillSelector();
        var player = new DummyPlayer();

        var monster = new DummyMonster(
            attackElement: ElementType.Fire,
            elementalPower: 5)
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 3,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        var expectedStandardDamage = (int)Math.Round(
            monster.AttackPower * 1.1,
            MidpointRounding.AwayFromZero) - 3;

        var expectedElementalDamage = 5 - 3;

        monster.Attack(player);

        Assert.AreEqual(expectedStandardDamage, player.DamageTaken);
        Assert.AreEqual(expectedElementalDamage, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Boundary and Decision Coverage ──

    [TestMethod]
    public void ExecuteAttack_At25PercentHealth_ActivatesEmergencyHealing()
    {
        // At exactly 25 HP (25% of 100), emergency healing SHOULD activate because
        // the condition is CurrentHealth <= emergencyHealThreshold (25 <= 25 is true).
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            CurrentHealth = 25,
            Strategy = new CunningStrategy(selector)
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_MoreMagicalDamageTaken_SelectsMagicDebuff()
    {
        // When MoreMartialDamageTaken returns false, the debuff should target Magic stat.
        // The FakeSkillSelector returns the debuff skill regardless of StatType,
        // so we verify the debuff branch is taken when TookMoreMartial is false.
        var debuffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(debuffSkill: debuffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90,
            TookMoreMartial = false
        };

        monster.Attack(player);

        Assert.IsTrue(debuffSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_MagicalDamageType_SelectsMagicBuff()
    {
        // When DamageType is Magical, the buff should target Magic stat.
        // The FakeSkillSelector returns the buff skill regardless of StatType,
        // so we verify the buff branch is taken when DamageType is Magical.
        var buffSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(buffSkill: buffSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90,
            DamageType = DamageType.Magical
        };

        monster.Attack(player);

        Assert.IsTrue(buffSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_DamageSkillAvailable_DoesNotPerformBasicAttack()
    {
        // When a damage skill is available and no support skills are available,
        // the damage skill should activate and return without also performing a basic attack.
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
        // Player damage should equal only the skill's power, not skill + basic attack
        Assert.AreEqual(damageSkill.Power, player.DamageTaken);
    }

    [TestMethod]
    public void ExecuteAttack_FallbackHealingSkillAvailable_ActivatesHealing()
    {
        // When no debuff, buff, or damage skill is available but a healing skill is,
        // the fallback branch should activate the healing skill and then perform a basic attack.
        var healingSkill = new DummySupportSkill();
        var selector = new FakeSkillSelector(healingSkill: healingSkill);
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            Strategy = new CunningStrategy(selector),
            CurrentHealth = 90
        };

        monster.Attack(player);

        Assert.IsTrue(healingSkill.Activated);
        var expectedDamage = (int)Math.Round(monster.AttackPower * 1.1, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedDamage, player.DamageTaken);
    }
}