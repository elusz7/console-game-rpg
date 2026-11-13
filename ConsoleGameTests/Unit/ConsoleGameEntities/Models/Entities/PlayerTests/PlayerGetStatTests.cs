using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Skills;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerGetStatTests
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

    private static Armor CreateArmor()
    {
        return new Armor
        {
            Name = "Test Armor",
            Value = 10.5M,
            RequiredLevel = 1,
            Durability = 3,
            DefensePower = 10,
            Resistance = 10,
            ArmorType = ArmorType.Head
        };
    }
    
    private static SupportSkill CreateSupportSkill(StatType stat, SupportEffectType effect)
    {
        return new SupportSkill
        {
            Name = "Support Boost",
            Duration = 3,
            Cooldown = 5,
            Power = 4,
            StatAffected = stat,
            SupportEffect = effect,
            TargetType = TargetType.Self,
            ElapsedTime = 5
        };
    }

    [TestMethod]
    public void GetStat_ReturnsCorrectStat()
    {
        var player = CreatePlayer();
        player.Archetype.AttackBonus = 10;

        int attack = player.GetStat(StatType.Attack);

        Assert.AreEqual(10, attack);
    }

    [TestMethod]
    public void GetStat_InvalidStat_Throws()
    {
        var player = CreatePlayer();

        Assert.ThrowsException<StatTypeException>(() => player.GetStat((StatType)999));
    }

    [TestMethod]
    public void GetStat_Attack_IncludesWeaponAndArchetypeBonus()
    {
        var player = CreatePlayer();
        player.Archetype.AttackBonus = 5;

        var weapon = CreateWeapon();
        weapon.AttackPower = 12;
        weapon.RequiredLevel = player.Level;

        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        Assert.AreEqual(17, player.GetStat(StatType.Attack));
    }

    [TestMethod]
    public void GetStat_Magic_IncludesWeaponAndArchetypeBonus()
    {
        var player = CreatePlayer();
        player.Archetype.MagicBonus = 4;

        var weapon = CreateWeapon();
        weapon.AttackPower = 12;
        weapon.RequiredLevel = player.Level;

        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        Assert.AreEqual(16, player.GetStat(StatType.Magic));
    }

    [TestMethod]
    public void GetStat_Defense_AveragesEquippedArmor()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 3;

        var headArmor = CreateArmor();
        headArmor.DefensePower = 10;
        headArmor.RequiredLevel = player.Level;

        var chestArmor = CreateArmor();
        chestArmor.DefensePower = 20;
        chestArmor.ArmorType = ArmorType.Chest;
        chestArmor.RequiredLevel = player.Level;

        player.Inventory.AddItem(headArmor);
        player.Inventory.AddItem(chestArmor);

        player.Equip(headArmor);
        player.Equip(chestArmor);

        Assert.AreEqual(18, player.GetStat(StatType.Defense));
    }

    [TestMethod]
    public void GetStat_Resistance_AveragesEquippedArmor()
    {
        var player = CreatePlayer();
        player.Archetype.ResistanceBonus = 2;

        var headArmor = CreateArmor();
        headArmor.Resistance = 8;
        headArmor.RequiredLevel = player.Level;

        var chestArmor = CreateArmor();
        chestArmor.Resistance = 18;
        chestArmor.ArmorType = ArmorType.Chest;
        chestArmor.RequiredLevel = player.Level;

        player.Inventory.AddItem(headArmor);
        player.Inventory.AddItem(chestArmor);

        player.Equip(headArmor);
        player.Equip(chestArmor);

        Assert.AreEqual(15, player.GetStat(StatType.Resistance));
    }

    [TestMethod]
    public void GetStat_WithoutEquipment_ReturnsArchetypeBonus()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 5;
        player.Archetype.ResistanceBonus = 3;
        player.Archetype.AttackBonus = 7;
        player.Archetype.MagicBonus = 4;

        Assert.AreEqual(5, player.GetStat(StatType.Defense));
        Assert.AreEqual(3, player.GetStat(StatType.Resistance));
        Assert.AreEqual(7, player.GetStat(StatType.Attack));
        Assert.AreEqual(4, player.GetStat(StatType.Magic));
    }
    
    [TestMethod]
    public void GetStat_Charred_ReducesDefense()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 10;

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 4,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(6, player.GetStat(StatType.Defense));
    }
    
    [TestMethod]
    public void GetStat_Shocked_ReducesResistance()
    {
        var player = CreatePlayer();
        player.Archetype.ResistanceBonus = 10;

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            power: 4,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(6, player.GetStat(StatType.Resistance));
    }
    
    [TestMethod]
    public void GetStat_Snared_ReducesSpeed()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 10;

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Snared),
            power: 4,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(6, player.GetStat(StatType.Speed));
    }
    
    [TestMethod]
    public void GetStat_ActiveEffectCannotReduceStatBelowZero()
    {
        var player = CreatePlayer();
        player.Archetype.ResistanceBonus = 2;

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            power: 5,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(0, player.GetStat(StatType.Resistance));
    }
    
    [TestMethod]
    public void GetStat_Charred_CombinesWithSupportStatModifier()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 10;

        var skill = CreateSupportSkill(StatType.Defense, SupportEffectType.Reduce);

        var supportEffect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill), // Unique identity key
            power: 2,
            duration: 3,
            sourceSkill:skill);

        player.ActiveEffectManager.Register(supportEffect);

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 3,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(5, player.GetStat(StatType.Defense));
    }
    
    [TestMethod]
    public void GetStat_AfterRemovingCharred_UsesUpdatedDefense()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 10;

        var effect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 4,
            duration: 3);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(6, player.GetStat(StatType.Defense));

        player.ActiveEffectManager.RemoveById(effect.EffectId);

        Assert.AreEqual(10, player.GetStat(StatType.Defense));
    }
    
    [TestMethod]
    public void GetStat_NoActiveEffects_PreservesBaseStatCalculations()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 5;
        player.Archetype.ResistanceBonus = 3;
        player.Archetype.Speed = 8;

        Assert.AreEqual(5, player.GetStat(StatType.Defense));
        Assert.AreEqual(3, player.GetStat(StatType.Resistance));
        Assert.AreEqual(8, player.GetStat(StatType.Speed));
    }
    
    [TestMethod]
    public void GetStat_Attack_CombinesWeaponArchetypeAndSupportModifier()
    {
        var player = CreatePlayer();
        player.Archetype.AttackBonus = 5;

        var weapon = CreateWeapon();
        weapon.AttackPower = 12;
        weapon.RequiredLevel = player.Level;

        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var skill = CreateSupportSkill(StatType.Attack, SupportEffectType.Boost);

        var effect = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(skill),
            power: 4,
            duration: 3,
            sourceSkill: skill);

        player.ActiveEffectManager.Register(effect);

        Assert.AreEqual(21, player.GetStat(StatType.Attack));
    }
    
    [TestMethod]
    public void GetStat_SupportBoostAndReduction_Combine()
    {
        // Arrange
        var player = CreatePlayer();
        player.Archetype.Speed = 10;

        var boostSkill = CreateSupportSkill(StatType.Speed, SupportEffectType.Boost);
        var boost = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(boostSkill),
            power: 5,
            duration: 3,
            sourceSkill: boostSkill);

        var reductionSkill = CreateSupportSkill(StatType.Speed, SupportEffectType.Reduce);
        var reduction = new ActiveEffect(
            ActiveEffectType.SupportStatModifier,
            new SupportSkillEffectKey(reductionSkill),
            power: 3,
            duration: 3,
            sourceSkill: reductionSkill);

        player.ActiveEffectManager.Register(boost);
        player.ActiveEffectManager.Register(reduction);

        // Act
        var result = player.GetStat(StatType.Speed);

        // Assert
        Assert.AreEqual(12, result); // 10 + 5 - 3
    }
    
}
