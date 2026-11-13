using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerAttackTests
{
    private static Player CreatePlayer()
    {
        var player = new Player
        {
            CurrentHealth = 100,
            MaxHealth = 100,
            DodgeChance = 0
        };

        var archetype = new Archetype
        {
            Name = "Test",
            ArchetypeType = ArchetypeType.Martial,
            AttackMultiplier = 1.5m,
            MagicMultiplier = 1.2m,
            DefenseMultiplier = 1.0m,
            ResistanceMultiplier = 0.5m,
            SpeedMultiplier = 2.0m,
            ResourceMultiplier = 3.0m,
            ResourceName = "Energy",
            RecoveryRate = 2,
            RecoveryGrowth = 1,
            HealthBase = 10,
            MaxResource = 10,
            CurrentResource = 10
        };

        var inventory = new Inventory
        {
            Id = 1,
            Gold = 100,
            Capacity = 36.2M,
            Items = []
        };

        player.Archetype = archetype;
        player.Inventory = inventory;
        inventory.Player = player;

        return player;
    }

    private static Weapon CreateWeapon()
    {
        return new Weapon
        {
            Id = 7,
            Name = "Test Weapon",
            Value = 4.6M,
            RequiredLevel = 1,
            AttackPower = 12,
            Durability = 3
        };
    }

    private static ActiveEffect CreateCorruptedEffect(int power)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Corrupted),
            power: power,
            duration: 3);
    }

    private static Weapon CreateWeaponWithRune(ElementType element, int tier, int power)
    {
        var weaponRune = new WeaponRune
        {
            Id = 1,
            Name = $"Rune of {element}",
            Element = element,
            Power = power,
            Rarity = RarityLevel.Common
        };

        var inventoryRune = new InventoryRune
        {
            InventoryId = 1,
            Rune = weaponRune,
            Tier = tier,
            Quantity = 1
        };

        var equippedRune = new EquippedRune
        {
            Id = 1,
            Tier = tier,
            InventoryRune = inventoryRune
        };

        inventoryRune.EquippedRunes.Add(equippedRune);

        var weapon = CreateWeapon();
        weapon.EquippedRune = equippedRune;

        return weapon;
    }

    // ── Basic Attack Tests ──

    [TestMethod]
    public void Attack_WithWeapon_AttacksTarget()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        player.Inventory.AddItem(weapon);
        player.Equipment.Add(weapon);

        var oldDurability = weapon.Durability;
        var targetMock = new Mock<ITargetable>();

        player.Attack(targetMock.Object);

        targetMock.Verify(t => t.TakeDamage(It.IsAny<AttackDamage>()), Times.Once);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You attack with")));
        Assert.IsTrue(weapon.Durability < oldDurability);
    }

    [TestMethod]
    public void Attack_WithoutWeapon_ThrowsException()
    {
        var player = CreatePlayer();

        var targetMock = new Mock<ITargetable>();

        var ex = Assert.ThrowsException<EquipmentException>(() => player.Attack(targetMock.Object));

        StringAssert.Contains(ex.Message, "need to equip a weapon");
    }

    [TestMethod]
    public void Attack_WithWeapon_UsesUpDurability()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.Durability = 1;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();

        player.Attack(targetMock.Object);

        targetMock.Verify(t => t.TakeDamage(It.IsAny<AttackDamage>()), Times.Once);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("broke and can't be used")));
        Assert.IsFalse(weapon.IsEquipped());
    }

    // ── Attack: No Rune ──

    [TestMethod]
    public void Attack_WithoutRune_ElementalDamageIsZero()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(0, capturedAttack.ElementalDamage);
        Assert.IsNull(capturedAttack.Element);
    }

    [TestMethod]
    public void Attack_WithoutRune_StandardDamageDealt()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.IsTrue(capturedAttack.StandardDamage > 0);
    }

    // ── Attack: Each Elemental Rune ──

    [TestMethod]
    public void Attack_WithFireRune_DealsFireElementalDamage()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
        Assert.AreEqual(ElementType.Fire, capturedAttack.Element);
    }

    [DataTestMethod]
    [DataRow(ElementType.Fire, 5)]
    [DataRow(ElementType.Lightning, 5)]
    [DataRow(ElementType.Ice, 5)]
    [DataRow(ElementType.Nature, 5)]
    [DataRow(ElementType.Radiance, 5)]
    [DataRow(ElementType.Abyssal, 5)]
    public void Attack_WithElementalRune_AllElements(ElementType element, int expectedDamage)
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(element, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(expectedDamage, capturedAttack.ElementalDamage);
        Assert.AreEqual(element, capturedAttack.Element);
    }

    // ── Attack: Tier-Based Damage ──

    [TestMethod]
    public void Attack_Tier1Rune_DealsTierTimesPower()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
    }

    [TestMethod]
    public void Attack_Tier2Rune_DealsTierTimesPower()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 2, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(10, capturedAttack.ElementalDamage);
    }

    [TestMethod]
    public void Attack_Tier3Rune_DealsTierTimesPower()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 3, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(15, capturedAttack.ElementalDamage);
    }

    [DataTestMethod]
    [DataRow(1, 3, 3)]
    [DataRow(2, 3, 6)]
    [DataRow(3, 3, 9)]
    [DataRow(4, 3, 12)]
    [DataRow(5, 3, 15)]
    public void Attack_TierScaling_ElementalDamageScales(int tier, int power, int expectedDamage)
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: tier, power: power);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(expectedDamage, capturedAttack.ElementalDamage);
    }

    // ── Attack: Combined Standard + Elemental ──

    [TestMethod]
    public void Attack_WithRune_BothStandardAndElementalDamage()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.IsTrue(capturedAttack.StandardDamage > 0);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
    }

    // ── Attack: Dodge with Rune ──

    [TestMethod]
    public void Attack_WithRune_PassesBothDamageComponentsToTarget()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.IsTrue(capturedAttack.StandardDamage > 0);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
        Assert.AreEqual(ElementType.Fire, capturedAttack.Element);
    }

    // ── Attack: Action Items with Rune ──

    [TestMethod]
    public void Attack_WithRune_ActionItemShowsElementalDamage()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();

        player.Attack(targetMock.Object);

        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("primordial energy")));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("5 points of Fire")));
    }

    [TestMethod]
    public void Attack_WithoutRune_ActionItemDoesNotShowElementalDamage()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();

        player.Attack(targetMock.Object);

        Assert.IsFalse(player.ActionItems.Values.Any(a => a.Contains("primordial energy")));
    }

    // ── Attack: Magical Archetype with Rune ──

    [TestMethod]
    public void Attack_MagicalArchetype_WithRune_DealsElementalDamage()
    {
        var player = CreatePlayer();
        player.Archetype.ArchetypeType = ArchetypeType.Magical;

        var weapon = CreateWeaponWithRune(ElementType.Ice, tier: 2, power: 4);
        weapon.RequiredLevel = player.Level;
        weapon.DamageType = DamageType.Magical;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(8, capturedAttack.ElementalDamage);
        Assert.AreEqual(ElementType.Ice, capturedAttack.Element);
        Assert.AreEqual(DamageType.Magical, capturedAttack.DamageType);
    }
    
    [TestMethod]
    public void Attack_WithCorruptedEffect_ReducesBothDamageComponents()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        player.ActiveEffectManager.Register(
            CreateCorruptedEffect(power: 3));

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;

        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(weapon.AttackPower - 3, capturedAttack.StandardDamage);
        Assert.AreEqual(2, capturedAttack.ElementalDamage);
        Assert.AreEqual(ElementType.Fire, capturedAttack.Element);
    }
    
    [TestMethod]
    public void Attack_WithCorruptedEffect_ElementalDamageReducedToZero()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        player.ActiveEffectManager.Register(
            CreateCorruptedEffect(power: 5));

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;

        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(weapon.AttackPower - 5, capturedAttack.StandardDamage);
        Assert.AreEqual(0, capturedAttack.ElementalDamage);
        Assert.IsNull(capturedAttack.Element);
    }
    
    [TestMethod]
    public void Attack_WithCorruptedEffect_ReducesBothComponentsToZero_ThrowsAttackException()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        player.ActiveEffectManager.Register(
            CreateCorruptedEffect(power: weapon.AttackPower));

        var targetMock = new Mock<ITargetable>();

        Assert.ThrowsException<AttackException>(
            () => player.Attack(targetMock.Object));

        targetMock.Verify(
            t => t.TakeDamage(It.IsAny<AttackDamage>()),
            Times.Never);
    }
    
    [TestMethod]
    public void Attack_WithCorruptedEffect_StandardDamageReducedToZero()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 3, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        player.ActiveEffectManager.Register(
            CreateCorruptedEffect(power: weapon.AttackPower));

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;

        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(0, capturedAttack.StandardDamage);
        Assert.AreEqual(3, capturedAttack.ElementalDamage);
    }
    
    [TestMethod]
    public void Attack_AfterCorruptedRemoval_RestoresOriginalDamage()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var effect = CreateCorruptedEffect(power: 3);
        player.ActiveEffectManager.Register(effect);

        player.ActiveEffectManager.RemoveById(effect.EffectId);

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;

        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.AreEqual(weapon.AttackPower, capturedAttack.StandardDamage);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
    }
}
