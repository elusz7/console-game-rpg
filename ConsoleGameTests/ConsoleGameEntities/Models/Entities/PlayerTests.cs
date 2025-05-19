using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Exceptions;
using ConsoleGameEntities.Interfaces;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Interfaces.ItemAttributes;
using System.Collections.Generic;

namespace ConsoleGameEntities.Tests.Models.Entities
{
    [TestClass]
    public class PlayerTests
    {
        private static Player CreatePlayer()
        {
            var player = new Player
            {
                CurrentHealth = 100,
                MaxHealth = 100
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

        private static Consumable CreateConsumable()
        {
            return new Consumable
            {
                Name = "Test Consumable",
                Value = 10,
                RequiredLevel = 1,
                Durability = 1
            };
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

            targetMock.Verify(t => t.TakeDamage(It.IsAny<int>(), DamageType.Martial), Times.Once);
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
            
            targetMock.Verify(t => t.TakeDamage(It.IsAny<int>(), DamageType.Martial), Times.Once);
            Assert.IsTrue(player.ActionItems.Values.Any(a => a.Contains("broke and can't be used")));
            Assert.IsFalse(weapon.IsEquipped());
        }

        [TestMethod]
        public void TakeDamage_MartialDamage_ReducesHealth()
        {
            var player = CreatePlayer();
            player.Archetype.DefenseBonus = 0;
            
            player.TakeDamage(10, DamageType.Martial);

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

            player.TakeDamage(10, DamageType.Martial);

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

            player.TakeDamage(10, DamageType.Magical);

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

            player.TakeDamage(10, DamageType.Martial);

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

            player.TakeDamage(10, DamageType.Magical);

            Assert.AreEqual(armor.Durability, oldDurability);
        }

        [TestMethod]
        public void TakeDamage_MagicalDamage_ReducesHealth()
        {
            var player = CreatePlayer();
            player.Archetype.ResistanceBonus = 0;

            player.TakeDamage(10, DamageType.Magical);

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

            Assert.ThrowsException<PlayerDeathException>(() => player.TakeDamage(101, DamageType.Martial));
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
        public void Equip_CursedItem()
        {
            var player = CreatePlayer();
            var weapon = CreateWeapon();
            weapon.RequiredLevel = player.Level;
            weapon.Curse();
            player.Inventory.AddItem(weapon);

            var oldCurrentHealth = player.CurrentHealth;
            var oldMaxHealth = player.MaxHealth;

            player.Equip(weapon);

            Assert.IsTrue(player.CurrentHealth < oldCurrentHealth);
            Assert.IsTrue(player.MaxHealth < oldMaxHealth);
            Assert.IsFalse(weapon.IsCursed());
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
    }
}
