using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Combat.ActiveEffects;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerTakeDamageTests
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
    
    private static ActiveEffect CreateBlindedEffect(int power)
    {
        return new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Blinded),
            power: power,
            duration: 3);
    }

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You've taken")));
    }

    [TestMethod]
    public void TakeDamage_Martial_WithArmor_UsesUpDurability()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var armor = CreateArmor();
        armor.Durability = 1;
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("broke and can't be used")));
        Assert.IsFalse(armor.IsEquipped());
    }

    [TestMethod]
    public void TakeDamage_Magical_WithArmor_UsesUpDurability()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.ResistanceBonus = 0;

        var armor = CreateArmor();
        armor.Durability = 1;
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        player.TakeDamage(attack);

        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("broke and can't be used")));
        Assert.IsFalse(armor.IsEquipped());
    }

    [TestMethod]
    public void TakeDamage_Martial_ArmorHasNoDefense_ArmorNotUsed()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var armor = CreateArmor();
        armor.DefensePower = 0;
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var oldDurability = armor.Durability;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(armor.Durability, oldDurability);
    }

    [TestMethod]
    public void TakeDamage_Magical_ArmorHasNoResistance_ArmorNotUsed()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.ResistanceBonus = 0;

        var armor = CreateArmor();
        armor.Resistance = 0;
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var oldDurability = armor.Durability;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(armor.Durability, oldDurability);
    }

    [TestMethod]
    public void TakeDamage_MagicalDamage_ReducesHealth()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.ResistanceBonus = 0;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You've taken")));
    }

    [TestMethod]
    public void TakeDamage_HealthBelowZero_ThrowsDeath()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 0;
        
        var attack = new AttackDamage(250, DamageType.Magical, 0, null);

        Assert.ThrowsException<PlayerDeathException>(() => player.TakeDamage(attack));
    }

    [TestMethod]
    public void TakeDamage_Blinded_StandardDamageOnly_AppliesEntireBonusToStandard()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        // Blinded bonus 5 added to standard 10 = 15, defense 0, so 15 damage taken
        Assert.AreEqual(85, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_ElementalDamageOnly_AppliesEntireBonusToElemental()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);

        var attack = new AttackDamage(0, DamageType.Martial, 8, ElementType.Fire);
        player.TakeDamage(attack);

        // Blinded bonus 5 added entirely to elemental damage: 8 + 5 = 13.
        // No matching armor resilience, so 13 damage taken.
        Assert.AreEqual(87, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_CombinedDamage_EvenBonus_SplitsEvenly()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(6);
        player.ActiveEffectManager.Register(blindedEffect);

        var attack = new AttackDamage(10, DamageType.Martial, 8, ElementType.Fire);
        player.TakeDamage(attack);

        // Blinded bonus 6 split: standard = (6+1)/2 = 3, elemental = 6/2 = 3
        // Standard: 10 + 3 = 13, defense 0 = 13; Elemental: 8 + 3 = 11, no armor resilience = 11
        // Total: 13 + 11 = 24 damage taken
        Assert.AreEqual(76, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_CombinedDamage_OddBonus_GivesExtraPointToStandard()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);

        var attack = new AttackDamage(10, DamageType.Martial, 8, ElementType.Fire);
        player.TakeDamage(attack);

        // Blinded bonus 5 split: standard = (5+1)/2 = 3, elemental = 5/2 = 2
        // Standard: 10 + 3 = 13, defense 0 = 13; Elemental: 8 + 2 = 10, no armor resilience = 10
        // Total: 13 + 10 = 23 damage taken
        Assert.AreEqual(77, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_StandardDamage_AppliesDefenseAfterBonus()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 8;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        // Blinded bonus 5 added to standard 10 = 15, then defense 8 applied: max(1, 15-8) = 7
        Assert.AreEqual(93, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_ElementalDamage_AppliesResilienceAfterBonus()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);

        var armor = CreateArmor();
        armor.Resistance = 10;
        armor.Durability = 1;
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);
        player.Equip(armor);

        var attack = new AttackDamage(10, DamageType.Magical, 8, ElementType.Fire);
        player.TakeDamage(attack);

        // Standard: 10 - 0 resistance = 10; Elemental: 8 + 5 blinded = 13, armor resilience 10: max(0, 13-10) = 3
        // Total: 10 + 3 = 13 damage taken
        Assert.AreEqual(87, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_OtherElementalStatuses_DoNotIncreaseIncomingDamage()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var charredEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Charred),
            power: 5,
            duration: 3);
        var shockedEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Shocked),
            power: 5,
            duration: 3);
        var frozenEffect = new ActiveEffect(
            ActiveEffectType.ElementalStatus,
            new ElementalStatusEffectKey(ElementalStatusEffectType.Frozen),
            power: 5,
            duration: 3);
        player.ActiveEffectManager.Register(charredEffect);
        player.ActiveEffectManager.Register(shockedEffect);
        player.ActiveEffectManager.Register(frozenEffect);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        // No Blinded effect, so no bonus — same as normal: 10 damage
        Assert.AreEqual(90, player.CurrentHealth);
    }

    [TestMethod]
    public void TakeDamage_Blinded_RemovedBeforeAttack_AppliesNoBonus()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;

        var blindedEffect = CreateBlindedEffect(5);
        player.ActiveEffectManager.Register(blindedEffect);
        player.ActiveEffectManager.RemoveById(blindedEffect.EffectId);

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        // Blinded removed before attack, so no bonus — 10 damage
        Assert.AreEqual(90, player.CurrentHealth);
    }
    
    [TestMethod]
    public void TakeDamage_NoDamageComponents_ThrowsArgumentOutOfRangeException()
    {
        var player = CreatePlayer();
        player.ActiveEffectManager.Register(CreateBlindedEffect(5));

        var attack = new AttackDamage(0, DamageType.Martial, 0, null);

        Assert.ThrowsException<ArgumentOutOfRangeException>(
            () => player.TakeDamage(attack));
    }
}
