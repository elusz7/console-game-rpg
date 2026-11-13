using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using static ConsoleGameTests.TestHelpers.StrategyHelper;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Monsters.Strategies;

public class MonsterBasicAttackTests
{
    // ── Standard damage only ──

    [TestMethod]
    public void MakeBasicAttack_StandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Elemental damage only ──

    [TestMethod]
    public void MakeBasicAttack_ElementalDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5)
        {
            AttackPower = 0
        };

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Combined damage ──

    [TestMethod]
    public void MakeBasicAttack_CombinedDamage()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Multiplier affects standard damage only ──

    [TestMethod]
    public void MakeBasicAttack_MultiplierAffectsStandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player, multiplier: 1.5);

        var expectedStandard = (int)Math.Round(monster.AttackPower * 1.5, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedStandard, player.DamageTaken);
        Assert.AreEqual(5, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    // ── Corrupted reduces both components ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesBothComponents()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 10,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50 - 10, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Corrupted reduces either component to zero ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesStandardToZero_ElementalSurvives()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 60)
        {
            AttackPower = 50
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 50,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(10, player.ElementalDamageTaken);
        Assert.AreEqual(ElementType.Fire, player.ElementDealt);
    }

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesElementalToZero_StandardSurvives()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(50 - 5, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Corrupted reduces both to zero and throws ──

    [TestMethod]
    public void MakeBasicAttack_CorruptedReducesBothToZero_ThrowsAttackException()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 50,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        Assert.ThrowsException<AttackException>(() => monster.MakeBasicAttack(player));
    }

    // ── Damage components never become negative ──

    [TestMethod]
    public void MakeBasicAttack_DamageComponentsNeverBecomeNegative()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 3)
        {
            AttackPower = 10
        };

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 100,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        Assert.ThrowsException<AttackException>(() => monster.MakeBasicAttack(player));

        Assert.AreEqual(0, player.DamageTaken);
        Assert.AreEqual(0, player.ElementalDamageTaken);
    }

    // ── Element is null when elemental damage is zero ──

    [TestMethod]
    public void MakeBasicAttack_ElementIsNullWhenElementalDamageIsZero()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    [TestMethod]
    public void MakeBasicAttack_ElementIsNullWithoutElementalProperties()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        Assert.AreEqual(0, player.ElementalDamageTaken);
        Assert.IsNull(player.ElementDealt);
    }

    // ── Correct action messages for each component ──

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_ForStandardDamageOnly()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster();

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("50") && a.Contains(monster.DamageType.ToString())));
        Assert.IsFalse(actionValues.Any(a => a.Contains("Fire")));
    }

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_ForBothComponents()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("50") && a.Contains(monster.DamageType.ToString())));
        Assert.IsTrue(actionValues.Any(a => a.Contains("5") && a.Contains("Fire")));
    }

    [TestMethod]
    public void MakeBasicAttack_ActionMessages_NoElementalMessage_WhenElementalDamageIsZero()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster(attackElement: ElementType.Fire, elementalPower: 5);

        var corruptedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: 5,
            duration: 3);

        monster.ActiveEffectManager.Register(corruptedEffect);

        monster.MakeBasicAttack(player);

        var actionValues = monster.ActionItems.Values.ToList();
        Assert.IsTrue(actionValues.Any(a => a.Contains("45") && a.Contains(monster.DamageType.ToString())));
        Assert.IsFalse(actionValues.Any(a => a.Contains("Fire")));
    }

    // ── Midpoint rounding behavior ──

    [TestMethod]
    public void MakeBasicAttack_MidpointRoundingBehavior()
    {
        var player = new DummyPlayer();
        var monster = new DummyMonster
        {
            AttackPower = 33
        };

        monster.MakeBasicAttack(player, multiplier: 1.5);

        var expected = (int)Math.Round(33 * 1.5, MidpointRounding.AwayFromZero);
        Assert.AreEqual(50, expected);
        Assert.AreEqual(expected, player.DamageTaken);
    }
}
