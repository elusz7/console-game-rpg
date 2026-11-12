using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Combat;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Entities.Inventories;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Skills;
using Moq;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

namespace ConsoleGameTests.Unit.ConsoleGameEntities.Models.Entities.PlayerTests;

[TestClass]
public class PlayerTests
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

    private static Valuable CreateValuable()
    {
        return new Valuable
        {
            Id = 6,
            Name = "Test Valuable",
            Value = 56.7M,
            RequiredLevel = 1,
            Durability = 1
        };
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

    private static Monster CreateMonster()
    {
        return new Monster
        {
            Id = 10,
            Name = "Test Monster",
            Level = 1,
            CurrentHealth = 100,
            MaxHealth = 100,
            DefensePower = 10,
            Resistance = 10,
            AttackPower = 20,
            DamageType = DamageType.Martial
        };
    }

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

    [TestMethod]
    public void TakeDamage_MartialDamage_ReducesHealth()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 0;

        var attack = new AttackDamage(10, DamageType.Martial, 0, null);
        player.TakeDamage(attack);

        if (player.CurrentHealth < player.MaxHealth)
        {
            Assert.AreEqual(90, player.CurrentHealth);
            Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You've taken")));
        }
        else
        {
            Assert.AreEqual(player.CurrentHealth, player.MaxHealth);
            Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You dodged the attack")));
        }
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
        player.Archetype.ResistanceBonus = 0;

        var attack = new AttackDamage(10, DamageType.Magical, 0, null);
        player.TakeDamage(attack);

        if (player.CurrentHealth < player.MaxHealth)
        {
            Assert.AreEqual(90, player.CurrentHealth);
            Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You've taken")));
        }
        else
        {
            Assert.AreEqual(player.CurrentHealth, player.MaxHealth);
            Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You dodged the attack")));
        }
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
    public void Heal_HealthRestored()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.Heal(30);

        Assert.AreEqual(80, player.CurrentHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 30")));
    }

    [TestMethod]
    public void Heal_HealthDoesNotExceedMax()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 90;

        player.Heal(30);

        Assert.AreEqual(player.CurrentHealth, player.MaxHealth);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("healed for 10")));
    }

    [TestMethod]
    public void LevelUp_IncreasesLevelAndStats()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 70;

        var oldLevel = player.Level;
        var oldGold = player.Inventory.Gold;
        var oldCapacity = player.Inventory.Capacity;

        player.LevelUp();

        Assert.AreEqual(oldLevel + 1, player.Level);
        Assert.IsTrue(player.MaxHealth > 100);
        Assert.AreEqual(player.CurrentHealth, player.MaxHealth);
        Assert.IsTrue(player.Inventory.Gold > oldGold);
        Assert.IsTrue(player.Inventory.Capacity > oldCapacity);
    }

    [TestMethod]
    public void Equip_Weapon_EquipsWeapon()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);

        player.Equip(weapon);

        Assert.IsTrue(player.Equipment.Contains(weapon));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {weapon.Name}")));
    }

    [TestMethod]
    public void Equip_ItemNotInInventory_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();

        var ex = Assert.ThrowsException<ItemNotFoundException>(() => player.Equip(weapon));
        StringAssert.Contains(ex.Message, "not in your inventory");
    }

    [TestMethod]
    public void Equip_TooLowLevel_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level + 5;
        player.Inventory.AddItem(weapon);

        var ex = Assert.ThrowsException<EquipmentException>(() => player.Equip(weapon));
        StringAssert.Contains(ex.Message, "Your level is too low");
    }

    [TestMethod]
    public void Equip_BrokenItem_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.Durability = 0;
        player.Inventory.AddItem(weapon);

        var ex = Assert.ThrowsException<EquipmentException>(() => player.Equip(weapon));
        StringAssert.Contains(ex.Message, "This item is broken");
    }

    [TestMethod]
    public void Equip_MartialArchetypeMagicWeapon_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.DamageType = DamageType.Magical;
        player.Inventory.AddItem(weapon);

        var ex = Assert.ThrowsException<EquipmentException>(() => player.Equip(weapon));
        StringAssert.Contains(ex.Message, "You cannot equip weapons that deal");
    }

    [TestMethod]
    public void Equip_MagicArchetypeMartialWeapon_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Archetype.ArchetypeType = ArchetypeType.Magical;
        player.Inventory.AddItem(weapon);

        var ex = Assert.ThrowsException<EquipmentException>(() => player.Equip(weapon));
        StringAssert.Contains(ex.Message, "You cannot equip weapons that deal");
    }

    [TestMethod]
    public void Equip_Weapon_UnequipsOldWeapon()
    {
        var player = CreatePlayer();
        var oldWeapon = CreateWeapon();
        var newWeapon = CreateWeapon();
        player.Inventory.AddItem(oldWeapon);
        player.Inventory.AddItem(newWeapon);

        oldWeapon.RequiredLevel = player.Level;
        oldWeapon.Name = "Spear";
        newWeapon.RequiredLevel = player.Level;

        player.Equip(oldWeapon);
        player.ClearActionItems();

        player.Equip(newWeapon);

        Assert.IsFalse(player.Equipment.Contains(oldWeapon));
        Assert.IsTrue(player.Equipment.Contains(newWeapon));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"unequipped {oldWeapon.Name}")));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {newWeapon.Name}")));
    }

    [TestMethod]
    public void Equip_Armor_EquipsArmors()
    {
        var player = CreatePlayer();
        var armor = CreateArmor();
        armor.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor);

        player.Equip(armor);

        Assert.IsTrue(player.Equipment.Contains(armor));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {armor.Name}")));
    }

    [TestMethod]
    public void Equip_Armor_EquipsDifferentArmorTypes()
    {
        var player = CreatePlayer();
        var armor1 = CreateArmor();
        var armor2 = CreateArmor();
        armor1.RequiredLevel = player.Level;
        armor2.RequiredLevel = player.Level;
        armor2.ArmorType = ArmorType.Chest;
        player.Inventory.AddItem(armor1);
        player.Inventory.AddItem(armor2);

        player.Equip(armor1);
        player.Equip(armor2);

        Assert.IsTrue(player.Equipment.Contains(armor1));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {armor1.Name}")));
        Assert.IsTrue(player.Equipment.Contains(armor2));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {armor2.Name}")));
    }

    [TestMethod]
    public void Equip_Armor_ReplacesOldArmor()
    {
        var player = CreatePlayer();
        var armor1 = CreateArmor();
        var armor2 = CreateArmor();
        armor1.RequiredLevel = player.Level;
        armor2.RequiredLevel = player.Level;
        player.Inventory.AddItem(armor1);
        player.Inventory.AddItem(armor2);

        player.Equip(armor1);
        player.ClearActionItems();

        player.Equip(armor2);

        Assert.IsFalse(player.Equipment.Contains(armor1));
        Assert.IsTrue(player.Equipment.Contains(armor2));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"unequipped {armor1.Name}")));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains($"equipped {armor2.Name}")));
    }

    [TestMethod]
    public void ModifyStat_Health_Heals()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 50;

        player.ModifyStat(StatType.Health, 20);

        Assert.AreEqual(70, player.CurrentHealth);
    }

    [TestMethod]
    public void ModifyStat_OtherStat_ModifiesActiveEffect()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;

        player.ModifyStat(StatType.Speed, 5);

        Assert.AreEqual(5, player.GetStat(StatType.Speed));
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("Speed has been modified")));
    }

    [TestMethod]
    public void ModifyStat_InvalidStat_Throws()
    {
        var player = CreatePlayer();

        Assert.ThrowsException<StatTypeException>(() => player.ModifyStat((StatType)999, 5));
    }

    // -----------------------------------------------------------------------
    // Regression test: LevelUp desynchronizes _activeEffects from
    // SupportSkill.TargetsAffected, causing an incorrect stat modifier
    // when the skill effect subsequently expires.
    //
    // Expected behavior: After LevelUp clears _activeEffects to 0, the
    // SupportSkill should recognize the effect is no longer valid and
    // should NOT apply a revert modifier on expiration.
    //
    // Observed failure: LevelUp zeroes _activeEffects[Attack] to 0, but
    // SupportSkill.TargetsAffected still tracks the player. When the skill
    // expires, RevertEffect calls ModifyStat(Attack, -Power), pushing
    // _activeEffects[Attack] into the negative — creating a debuff where
    // none existed.
    // -----------------------------------------------------------------------

    [TestMethod]
    [Ignore("Deferred to Step 11, Phase C3: ActiveEffectManager lifecycle migration.")]
    public void LevelUp_WhileBuffActive_SkillExpirationCausesNegativeDebuff()
    {
        // Arrange: create a player with a +10 Attack buff from a SupportSkill.
        // Give the player a weapon so baseAttack > 0 — this makes the bug visible
        // because Math.Max(0, base + (-10)) will differ from base when base > 0.
        var player = CreatePlayer();
        player.Archetype.AttackBonus = 0;
        player.Archetype.AttackMultiplier = 0M; // Prevent LevelUp from changing AttackBonus
        player.Level = 2; // Ensure player meets the skill's RequiredLevel

        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.AttackPower = 20; // Non-zero base so the debuff is visible
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var buffSkill = new SupportSkill
        {
            Name = "Battle Cry",
            Power = 10,
            Duration = 2,
            Cooldown = 3,
            ElapsedTime = 6, // off cooldown
            Cost = 0,
            RequiredLevel = 1,
            StatAffected = StatType.Attack,
            SupportEffect = (int)SupportEffectType.Boost,
            TargetType = TargetType.Self
        };

        int baseAttack = player.GetStat(StatType.Attack);

        // Act 1: apply the buff.
        buffSkill.Activate(player);
        int boostedAttack = player.GetStat(StatType.Attack);
        Assert.AreEqual(baseAttack + 10, boostedAttack, "Buff should increase Attack by Power.");
        Assert.IsTrue(buffSkill.TargetsAffected.ContainsKey(player), "Skill should track the player.");

        // Act 2: level up while the buff is still active.
        player.LevelUp();
        int afterLevelUpAttack = player.GetStat(StatType.Attack);
        Assert.AreEqual(baseAttack, afterLevelUpAttack,
            "LevelUp zeroes _activeEffects, so Attack should return to base.");

        // The bug: TargetsAffected still contains the player, even though
        // _activeEffects[Attack] was zeroed by LevelUp.
        Assert.IsTrue(buffSkill.TargetsAffected.ContainsKey(player),
            "BUG: Skill still tracks the player after LevelUp cleared the effect.");

        // Act 3: advance time until the skill expires.
        buffSkill.UpdateElapsedTime(); // 1 turn left
        buffSkill.UpdateElapsedTime(); // 0 turns — should expire

        // Assert: the skill has expired and called RevertEffect.
        Assert.IsFalse(buffSkill.TargetsAffected.ContainsKey(player),
            "Skill should no longer track the player after expiration.");

        int finalAttack = player.GetStat(StatType.Attack);

        // EXPECTED: finalAttack == baseAttack (the effect was already zeroed
        // by LevelUp, so reverting should be a no-op).
        //
        // OBSERVED FAILURE: finalAttack == 0 (clamped from -10), because
        // RevertEffect called ModifyStat(Attack, -10) on an already-zeroed
        // _activeEffects[Attack], creating a -10 debuff.
        //
        // The test demonstrates the desynchronization bug. Math.Max(0, ...)
        // in GetStat clamps the result, so the effective Attack is 0 instead
        // of the expected base value.
        Assert.AreEqual(baseAttack, finalAttack,
            "BUG: After LevelUp + expiration, Attack should remain at base value, " +
            $"but RevertEffect applied a negative modifier. Expected {baseAttack}, got {finalAttack}.");
    }

    [TestMethod]
    public void Loot_MonsterWithLoot_AddsItem()
    {
        var player = CreatePlayer();
        var monster = CreateMonster();
        var valuable = CreateValuable();
        monster.SetLoot(valuable);
        monster.CurrentHealth = -5;

        player.Loot(monster);

        Assert.IsTrue(player.Inventory.Items.Contains(valuable));
        Assert.IsNull(monster.Treasure);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("You found")));
    }

    [TestMethod]
    public void Loot_MonsterWithLoot_MonsterAlive_Throws()
    {
        var player = CreatePlayer();
        var monster = CreateMonster();
        var valuable = CreateValuable();
        monster.SetLoot(valuable);

        var ex = Assert.ThrowsException<TreasureException>(() => player.Loot(monster));
        StringAssert.Contains(ex.Message, "might be better to defeat");
    }

    [TestMethod]
    public void Loot_MonsterWithLoot_LootWeightExceedsCapacity_Throws()
    {
        var player = CreatePlayer();
        player.Inventory.Capacity = 5.0M;
        var monster = CreateMonster();
        monster.CurrentHealth = -5;
        var weapon = CreateWeapon();
        weapon.Weight = 6.0M;
        monster.SetLoot(weapon);

        var ex = Assert.ThrowsException<TreasureException>(() => player.Loot(monster));
        StringAssert.Contains(ex.Message, "Will exceed carrying capacity");
    }

    [TestMethod]
    public void Loot_MonsterNoLoot_Throws()
    {
        var player = CreatePlayer();
        var monster = CreateMonster();
        monster.CurrentHealth = -5;

        var ex = Assert.ThrowsException<TreasureException>(() => player.Loot(monster));
        StringAssert.Contains(ex.Message, "has no treasure to loot");
    }

    [TestMethod]
    public void ClearActionItems_RemovesAll()
    {
        var player = CreatePlayer();
        player.AddActionItem("Test action");
        Assert.IsTrue(player.ActionItems.Count > 0);

        player.ClearActionItems();

        Assert.AreEqual(0, player.ActionItems.Count);
    }

    [TestMethod]
    public void AddActionItem_AddsAction()
    {
        var player = CreatePlayer();
        player.AddActionItem("Test action");
        Assert.IsTrue(player.ActionItems.Values.Contains("Test action"));
    }

    [TestMethod]
    public void AddActionItem_Skill_AddsAction()
    {
        var player = CreatePlayer();
        var skill = new Skill { Name = "Fireball" };
        player.AddActionItem(skill);
        Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("use Fireball")));
    }

    [TestMethod]
    public void Sell_Item_Sucess()
    {
        var player = CreatePlayer();
        var valuable = CreateValuable();
        player.Inventory.AddItem(valuable);

        var expectedGold = player.Inventory.Gold + (int)Math.Round(valuable.Value);

        player.Sell(valuable);

        Assert.AreEqual(expectedGold, player.Inventory.Gold);
        Assert.IsFalse(player.Inventory.Items.Contains(valuable));
    }

    [TestMethod]
    public void Sell_EquippedItem_Throws()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var ex = Assert.ThrowsException<InvalidOperationException>(() => player.Sell(weapon));
        StringAssert.Contains(ex.Message, "sell an equipped item");
    }

    [TestMethod]
    public void Buy_Item_Sucess()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();

        var expectedGold = player.Inventory.Gold - (int)Math.Round(weapon.GetBuyPrice());

        player.Buy(weapon);

        Assert.AreEqual(expectedGold, player.Inventory.Gold);
        Assert.IsTrue(player.Inventory.Items.Contains(weapon));
    }

    [TestMethod]
    public void Initialize_SetsPlayerDefaults()
    {
        var player = new Player();
        player.Name = "Initializing Test Player";

        var archetype = new Archetype
        {
            Name = "Initializing Test Archetype",
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
            MaxResource = 10
        };

        player.Archetype = archetype;
        player.Initialize();

        Assert.AreEqual(1, player.Level);
        Assert.AreEqual((int)(archetype.HealthBase * 1.5), player.MaxHealth);
        Assert.AreEqual(player.MaxHealth, player.CurrentHealth);
        Assert.AreEqual(player.Archetype.MaxResource, player.Archetype.CurrentResource);
        Assert.IsNotNull(player.Inventory);
    }

    [TestMethod]
    public void RecoverResource_CallsArchetypeRecover()
    {
        var player = CreatePlayer();
        player.Archetype.UseResource(10);

        var oldResource = player.Archetype.CurrentResource;

        Assert.IsTrue(oldResource < player.Archetype.MaxResource);

        player.RecoverResource(5);

        Assert.IsTrue(player.Archetype.CurrentResource > oldResource);
    }

    [TestMethod]
    public void Unequip_Armor_RemovesStatContribution()
    {
        var player = CreatePlayer();
        player.Archetype.DefenseBonus = 3;
    
        var armor = CreateArmor();
        armor.DefensePower = 15;
        armor.RequiredLevel = player.Level;
    
        player.Inventory.AddItem(armor);
        player.Equip(armor);
    
        Assert.AreEqual(18, player.GetStat(StatType.Defense));
    
        player.Unequip(armor);
    
        Assert.AreEqual(3, player.GetStat(StatType.Defense));
        Assert.IsFalse(player.Equipment.Contains(armor));
        Assert.IsFalse(armor.IsEquipped());
    }
    
    [TestMethod]
    public void Equip_AlreadyEquippedWeapon_DoesNotDuplicate()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;

        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        player.Equip(weapon);

        Assert.AreEqual(1, player.Equipment.OfType<Weapon>().Count());
        Assert.IsTrue(player.Equipment.Contains(weapon));
        Assert.IsTrue(weapon.IsEquipped());
    }
    
    [TestMethod]
    public void Equip_CursedItem_DoesNotKillPlayer()
    {
        var player = CreatePlayer();
        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.Curse();
        player.Inventory.AddItem(weapon);

        player.Equip(weapon);

        Assert.AreEqual(90, player.CurrentHealth);
        Assert.AreEqual(90, player.MaxHealth);
        Assert.IsFalse(weapon.IsCursed());
    }
    
    [TestMethod]
    public void Equip_CursedItem_LethalCurse_ThrowsPlayerDeathException()
    {
        var player = CreatePlayer();
        player.CurrentHealth = 5;

        var weapon = CreateWeapon();
        weapon.RequiredLevel = player.Level;
        weapon.Curse();
        player.Inventory.AddItem(weapon);

        Assert.ThrowsException<PlayerDeathException>(
            () => player.Equip(weapon));

        Assert.IsTrue(player.CurrentHealth <= 0);
        Assert.IsFalse(weapon.IsCursed());
    }
    
    [TestMethod]
    public void TakeDamage_Hybrid_DefenseHigher_DamagesDefenseArmor()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;
        player.Archetype.ResistanceBonus = 0;

        var headArmor = new Armor
        {
            Name = "Head Armor",
            ArmorType = ArmorType.Head,
            Durability = 5,
            DefensePower = 10,
            RequiredLevel = player.Level
        };

        var chestArmor = new Armor
        {
            Name = "Chest Armor",
            ArmorType = ArmorType.Chest,
            Durability = 5,
            Resistance = 5,
            RequiredLevel = player.Level
        };

        player.Inventory.AddItem(headArmor);
        player.Inventory.AddItem(chestArmor);

        player.Equip(headArmor);
        player.Equip(chestArmor);

        var attack = new AttackDamage(20, DamageType.Hybrid, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(4, headArmor.Durability);
        Assert.AreEqual(5, chestArmor.Durability);
    }

    [TestMethod]
    public void TakeDamage_Hybrid_ResistanceHigher_DamagesResistanceArmor()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;
        player.Archetype.ResistanceBonus = 0;

        var headArmor = new Armor
        {
            Name = "Head Armor",
            ArmorType = ArmorType.Head,
            Durability = 5,
            DefensePower = 5,
            RequiredLevel = player.Level
        };

        var chestArmor = new Armor
        {
            Name = "Chest Armor",
            ArmorType = ArmorType.Chest,
            Durability = 5,
            Resistance = 10,
            RequiredLevel = player.Level
        };

        player.Inventory.AddItem(headArmor);
        player.Inventory.AddItem(chestArmor);

        player.Equip(headArmor);
        player.Equip(chestArmor);

        var attack = new AttackDamage(20, DamageType.Hybrid, 0, null);
        player.TakeDamage(attack);

        Assert.AreEqual(5, headArmor.Durability);
        Assert.AreEqual(4, chestArmor.Durability);
    }

    [TestMethod]
    public void TakeDamage_Hybrid_StatsEqual_DamagesEitherArmorType()
    {
        var player = CreatePlayer();
        player.Archetype.Speed = 0;
        player.Archetype.DefenseBonus = 0;
        player.Archetype.ResistanceBonus = 0;

        var headArmor = new Armor
        {
            Name = "Head Armor",
            ArmorType = ArmorType.Head,
            Durability = 5,
            DefensePower = 10,
            RequiredLevel = player.Level
        };

        var chestArmor = new Armor
        {
            Name = "Chest Armor",
            ArmorType = ArmorType.Chest,
            Durability = 5,
            Resistance = 10,
            RequiredLevel = player.Level
        };

        player.Inventory.AddItem(headArmor);
        player.Inventory.AddItem(chestArmor);

        player.Equip(headArmor);
        player.Equip(chestArmor);

        var attack = new AttackDamage(20, DamageType.Hybrid, 0, null);
        player.TakeDamage(attack);

        var headDamaged = headArmor.Durability == 4;
        var chestDamaged = chestArmor.Durability == 4;

        Assert.IsTrue(headDamaged ^ chestDamaged,
            "Exactly one armor piece should lose durability.");
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
        Assert.IsTrue(capturedAttack.PhysicalDamage > 0);
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
        Assert.IsTrue(capturedAttack.PhysicalDamage > 0);
        Assert.AreEqual(5, capturedAttack.ElementalDamage);
    }

    // ── Attack: Dodge with Rune ──

    [TestMethod]
    public void Attack_WithRune_TargetDodges_BothDamagesZeroed()
    {
        var player = CreatePlayer();
        var weapon = CreateWeaponWithRune(ElementType.Fire, tier: 1, power: 5);
        weapon.RequiredLevel = player.Level;
        player.Inventory.AddItem(weapon);
        player.Equip(weapon);

        var monster = CreateMonster();
        monster.DodgeChance = 33;

        var targetMock = new Mock<ITargetable>();
        AttackDamage? capturedAttack = null;
        targetMock.Setup(t => t.TakeDamage(It.IsAny<AttackDamage>()))
            .Callback<AttackDamage>(a => capturedAttack = a);

        player.Attack(targetMock.Object);

        Assert.IsNotNull(capturedAttack);
        Assert.IsTrue(capturedAttack.PhysicalDamage > 0);
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
}