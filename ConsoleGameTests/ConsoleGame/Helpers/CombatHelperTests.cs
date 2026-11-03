using ConsoleGame.Helpers;
using ConsoleGame.Helpers.CombatHelpers;
using ConsoleGame.Managers.Interfaces;
using ConsoleGameEntities.Interfaces;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Skills;
using Moq;

namespace ConsoleGameTests.ConsoleGame.Helpers;

[TestClass]
public class CombatHelperTests
{
    private Mock<IInputManager> _inputManagerMock = null!;
    private Mock<IOutputManager> _outputManagerMock = null!;

    private EquipmentHelper _equipmentHelper = null!;
    private CombatTurnScheduler _turnScheduler = null!;
    private CombatRoundCoordinator _roundCoordinator = null!;
    private CombatHelper _combatHelper = null!;

    [TestInitialize]
    public void Setup()
    {
        // Create mocks for console input and output.
        _inputManagerMock = new Mock<IInputManager>();
        _outputManagerMock = new Mock<IOutputManager>();

        // Create the helpers required by CombatHelper.
        _equipmentHelper = new EquipmentHelper(
            _inputManagerMock.Object,
            _outputManagerMock.Object);

        _turnScheduler = new CombatTurnScheduler();
        _roundCoordinator = new CombatRoundCoordinator(_turnScheduler);

        _combatHelper = new CombatHelper(
            _inputManagerMock.Object,
            _outputManagerMock.Object,
            _equipmentHelper,
            _roundCoordinator);
    }

    /// <summary>
    /// Creates a player with the minimum state required for combat.
    /// </summary>
    private static Player CreateTestPlayer()
    {
        var player = new Player
        {
            Name = "Test Player",
            Level = 1,
            CurrentHealth = 100,
            MaxHealth = 100,
            DodgeChance = 0,
            Archetype = new Archetype
            {
                Name = "Test Archetype",
                ArchetypeType = ArchetypeType.Martial,
                Skills = new List<Skill>(),
                CurrentResource = 100,
                MaxResource = 100,
                RecoveryRate = 0
            }
        };

        player.Inventory = new Inventory
        {
            Player = player,
            Items = new List<Item>()
        };

        return player;
    }

    /// <summary>
    /// Creates and equips a valid weapon for the test player.
    /// </summary>
    private static Weapon EquipTestWeapon(Player player)
    {
        var weapon = new Weapon
        {
            Name = "Test Sword",
            AttackPower = 20,
            DamageType = DamageType.Martial,
            Durability = 100,
            RequiredLevel = 1,
            Inventory = player.Inventory
        };

        player.Inventory.Items.Add(weapon);
        player.Equip(weapon);

        return weapon;
    }

    /// <summary>
    /// Creates a valid weapon in the player's inventory without equipping it.
    /// </summary>
    private static Weapon CreateUnequippedTestWeapon(Player player)
    {
        return new Weapon
        {
            Name = "Test Sword",
            AttackPower = 20,
            DamageType = DamageType.Martial,
            Durability = 100,
            RequiredLevel = 1,
            Inventory = player.Inventory
        };
    }

    /// <summary>
    /// Creates a health consumable for the test player.
    /// </summary>
    private static Consumable CreateTestHealthPotion(Player player)
    {
        return new Consumable
        {
            Name = "Health Potion",
            Description = "Restores health.",
            RequiredLevel = 1,
            Power = 20,
            Durability = 1,
            ConsumableType = ConsumableType.Health,
            Inventory = player.Inventory
        };
    }

    /// <summary>
    /// Creates an available offensive magic skill for the test player.
    /// </summary>
    private static MagicSkill CreateTestMagicSkill()
    {
        var skill = new MagicSkill
        {
            Name = "Test Fireball",
            Description = "A test offensive spell.",
            RequiredLevel = 1,
            Cost = 10,
            Power = 20,
            Cooldown = 2,
            TargetType = TargetType.SingleEnemy,
            SkillCategory = SkillCategory.Basic,
            DamageType = DamageType.Magical
        };

        skill.InitializeSkill(1);

        return skill;
    }

    /// <summary>
    /// Creates a monster that ends combat when its turn executes.
    /// </summary>
    private sealed class EndingCombatMonster : Monster
    {
        public override void Attack(IPlayer target)
        {
            CurrentHealth = 0;
        }
    }

    /// <summary>`
    /// Creates a living monster for combat tests.
    /// </summary>
    private static Monster CreateTestMonster()
    {
        return new EndingCombatMonster
        {
            Name = "Test Monster",
            CurrentHealth = 100,
            MaxHealth = 100,
            AggressionLevel = 1,
            AttackPower = 1,
            DodgeChance = 0,
            DamageType = DamageType.Martial
        };
    }

    /// <summary>
    /// Verifies that skipping a turn executes the selected action.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerSkipsTurn_ExecutesSkipAction()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(3);

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        _outputManagerMock.Verify(
            x => x.WriteLine(
                It.Is<string>(message => message.Contains("Skipping Turn")),
                It.IsAny<ConsoleColor>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that a successful basic attack consumes the player's turn.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerAttacks_ExecutesSuccessfulAttack()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;

        EquipTestWeapon(player);

        // Select Attack (1), then Skip Turn (3) on the next round.
        var choices = new Queue<int>([1, 3]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0,
            "The monster should be defeated by the player's attack.");

        _outputManagerMock.Verify(
            x => x.WriteLine(
                It.Is<string>(message => message.Contains("You attack with Test Sword")),
                It.IsAny<ConsoleColor>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that using an offensive skill successfully defeats its target.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerUsesMagicSkill_DefeatsMonster()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;

        var skill = CreateTestMagicSkill();
        player.Archetype.Skills.Add(skill);

        // Select Use Skill (2), then select the first available skill (1).
        var choices = new Queue<int>([2, 1]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(monster.CurrentHealth <= 0,
            "The magic skill should defeat the monster.");

        Assert.AreEqual(90, player.Archetype.CurrentResource);
    }

    /// <summary>
    /// Verifies that a skill failure does not consume the player's turn.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerLacksSkillResources_CanChooseAnotherAction()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();

        var skill = CreateTestMagicSkill();
        player.Archetype.Skills.Add(skill);

        // Not enough resources to use the skill.
        player.Archetype.CurrentResource = 5;

        // Select Use Skill (2), select the skill (1), then Skip Turn (3).
        var choices = new Queue<int>([2, 1, 3]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(5, player.Archetype.CurrentResource,
            "The failed skill should not consume player resources.");

        _outputManagerMock.Verify(
            x => x.WriteLine(
                It.Is<string>(message => message.Contains("Skipping Turn")),
                It.IsAny<ConsoleColor>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that using a health consumable heals the player and consumes their turn.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerUsesHealthConsumable_HealsPlayer()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();

        player.CurrentHealth = 50;

        var potion = CreateTestHealthPotion(player);
        player.Inventory.Items.Add(potion);

        // Select Use Consumable (3), then Skip Turn (4).
        var choices = new Queue<int>([3, 4]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        _inputManagerMock
            .Setup(x => x.SelectItem(
                It.IsAny<string>(),
                It.IsAny<List<Item>>(),
                It.IsAny<string?>()))
            .Returns(potion);

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(70, player.CurrentHealth,
            "The health potion should restore 20 health.");

        Assert.AreEqual(0, potion.Durability,
            "The potion should lose one durability when used.");

        Assert.IsFalse(player.Inventory.Items.Contains(potion),
            "The depleted potion should be removed from the inventory.");
    }

    /// <summary>
    /// Verifies that cancelling consumable selection does not consume the player's turn.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerCancelsConsumableSelection_CanChooseAnotherAction()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();

        var potion = CreateTestHealthPotion(player);
        player.Inventory.Items.Add(potion);

        // Select Use Consumable (3), cancel selection, then Skip Turn (4).
        var choices = new Queue<int>([3, 4]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        _inputManagerMock
            .Setup(x => x.SelectItem(
                It.IsAny<string>(),
                It.IsAny<List<Item>>(),
                It.IsAny<string?>()))
            .Returns((Item?)null);

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.AreEqual(1, potion.Durability,
            "Cancelling consumable selection should not use the potion.");

        Assert.IsTrue(player.Inventory.Items.Contains(potion),
            "The potion should remain in the inventory.");

        _outputManagerMock.Verify(
            x => x.WriteLine(
                It.Is<string>(message => message.Contains("Skipping Turn")),
                It.IsAny<ConsoleColor>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that an unsuccessful attack without a weapon does not consume the player's turn.
    /// </summary>
    [TestMethod]
    public void CombatRunner_PlayerAttacksWithoutWeapon_EquipsWeaponAndRetries()
    {
        // Arrange
        var player = CreateTestPlayer();
        var monster = CreateTestMonster();
        monster.CurrentHealth = 1;

        var weapon = CreateUnequippedTestWeapon(player);
        player.Inventory.Items.Add(weapon);

        // Select Attack, equip the weapon, then select Attack again.
        var choices = new Queue<int>([1, 1]);

        _inputManagerMock
            .Setup(x => x.ReadInt(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .Returns(() => choices.Dequeue());

        _inputManagerMock
            .Setup(x => x.SelectItem(
                It.IsAny<string>(),
                It.IsAny<List<Item>>(),
                It.IsAny<string?>()))
            .Returns(weapon);

        // Act
        _combatHelper.CombatRunner(player, [monster]);

        // Assert
        Assert.IsTrue(weapon.IsEquipped(),
            "The player should equip the selected weapon.");

        Assert.IsTrue(monster.CurrentHealth <= 0,
            "The player should successfully attack after equipping the weapon.");

        _outputManagerMock.Verify(
            x => x.WriteLine(
                It.Is<string>(message => message.Contains("cannot attack with your bare hands")),
                ConsoleColor.Red),
            Times.Once);
    }
}