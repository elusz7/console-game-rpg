using ConsoleGame.Factories;
using ConsoleGame.GameDao;
using ConsoleGame.Helpers;
using ConsoleGameEntities.Data;
using ConsoleGameEntities.Interfaces.Attributes;
using ConsoleGameEntities.Models.Entities;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Monsters.Strategies;
using ConsoleGameEntities.Models.Skills;
using ConsoleGameTests.TestHelpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ConsoleGameTests.Integration.ConsoleGame.Factories;

[TestClass]
public class FloorFactoryIntegrationTests
{
    private SqliteConnection _connection = null!;
    private GameContext _context = null!;
    
    private readonly Mock<IRandomProvider> _randomProviderMock = new();

    private RoomDao _roomDao = null!;
    private MonsterDao _monsterDao = null!;
    private SkillDao _skillDao = null!;
    private ItemDao _itemDao = null!;

    private FloorFactory _floorFactory = null!;

    [TestInitialize]
    public void Initialize()
    {
        //set up the randomizer 
        var random = new Random(12345);

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>()))
            .Returns((int max) => random.Next(max));

        _randomProviderMock
            .Setup(r => r.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int min, int max) => random.Next(min, max));

        _randomProviderMock
            .Setup(r => r.NextDouble())
            .Returns(random.NextDouble);
        
        // Create and open an isolated in-memory SQLite database.
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        // Configure the real GameContext to use SQLite.
        var options = new DbContextOptionsBuilder<GameContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new GameContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        // Use the real DAO
        _roomDao = new RoomDao(_context);
        _monsterDao = new MonsterDao(_context);
        _skillDao = new SkillDao(_context);
        _itemDao = new ItemDao(_context);

        var mapFactory = new MapFactory(_roomDao, new MapHelper(_randomProviderMock.Object));
        var monsterFactory = new MonsterFactory(_monsterDao, _skillDao, new MonsterSkillSelector(), _randomProviderMock.Object);
        var itemFactory = new ItemFactory(_itemDao, _randomProviderMock.Object);
        
        _floorFactory = new FloorFactory(mapFactory, monsterFactory, itemFactory, _randomProviderMock.Object);
        
        SeedSkills();
        SeedMonsters();
        SeedItems();
        SeedRooms();
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Dispose the context before closing its database connection.
        _context.Dispose();
        _connection.Dispose();
    }

    private void SeedSkills()
    {
        var skills = new List<Skill>();
        
        for (int i = 1; i < 16; i += 2)
        {
            for (int k = 1; k < 10; k += 2)
            {
                skills.Add(CreateSkill(ModelCreationHelper.TestSkillType.Martial, $"Martial Skill #{i}-{k}", k, i));
                skills.Add(CreateSkill(ModelCreationHelper.TestSkillType.Magical, $"Magical Skill #{i}-{k}", k, i));
                skills.Add(CreateSkill(ModelCreationHelper.TestSkillType.Ultimate, $"Ultimate Skill #{i}-{k}", k, i));
                skills.Add(CreateSkill(ModelCreationHelper.TestSkillType.Boss, $"Boss Skill #{i}-{k}", k, i));
                
                skills.Add(CreateSkill(ModelCreationHelper.TestSkillType.Support, $"Support Skill #{i}-{k}-Health", k, i, StatType.Health, TargetType.Self, SupportEffectType.Boost));
                foreach (var stat in new[] { StatType.Attack, StatType.Magic, StatType.Defense, StatType.Resistance, StatType.Speed })
                {
                    skills.Add(CreateSkill(
                        ModelCreationHelper.TestSkillType.Support,
                        $"Support Buff #{i}-{k}-{stat}",
                        k, i, stat, TargetType.Self));

                    skills.Add(CreateSkill(
                        ModelCreationHelper.TestSkillType.Support,
                        $"Support Debuff #{i}-{k}-{stat}",
                        k, i, stat, TargetType.SingleEnemy, SupportEffectType.Reduce));
                }
            }
        }
        
        _context.Skills.AddRange(skills);
        _context.SaveChanges();
    }

    private void SeedMonsters()
    {
        var monsters = new List<Monster>();
        for (int i = 1; i < 16; i += 2)
        {
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"martialLowMonster[{i}]", i, ThreatLevel.Low, DamageType.Martial));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"magicalLowMonster[{i}]", i, ThreatLevel.Low, DamageType.Magical));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"martialMediumMonster[{i}]", i, ThreatLevel.Medium, DamageType.Martial));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"magicalMediumMonster[{i}]", i, ThreatLevel.Medium, DamageType.Magical));
            monsters.Add( CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"martialHighMonster[{i}]", i, ThreatLevel.High, DamageType.Martial));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Common, $"magicalHighMonster[{i}]", i, ThreatLevel.High, DamageType.Magical));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Elite, $"martialEliteMonster[{i}]", i, ThreatLevel.Elite, DamageType.Martial));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Elite, $"magicalEliteMonster[{i}]", i, ThreatLevel.Elite, DamageType.Magical));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Elite, $"hybridEliteMonster[{i}]", i, ThreatLevel.Elite, DamageType.Hybrid));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Boss, $"martialBossMonster[{i}]", i, ThreatLevel.Boss, DamageType.Martial));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Boss, $"magicalBossMonster[{i}]", i, ThreatLevel.Boss, DamageType.Magical));
            monsters.Add(CreateMonster(ModelCreationHelper.TestMonsterType.Boss, $"hybridBossMonster[{i}]", i, ThreatLevel.Boss, DamageType.Hybrid));
        }
        
        _context.Monsters.AddRange(monsters);
        _context.SaveChanges();
    }

    private void SeedItems()
    {
        var items = new List<Item>();
        for (int i = 1; i < 16; i += 2)
        {
            items.Add(CreateItem(ModelCreationHelper.TestItemType.Weapon, $"MartialWeapon[{i}]", i, i + 1, i * 2.0M, damage: DamageType.Martial));
            items.Add(CreateItem(ModelCreationHelper.TestItemType.Weapon, $"MagicalWeapon[{i}]", i, i + 1, i * 2.0M, damage: DamageType.Magical));
            
            for (int j = 0; j < 4; j++)
            {
                items.Add(CreateItem(ModelCreationHelper.TestItemType.Armor, $"Armor[{i}-{j}]", i, i + 1, i * 2.0M, armor: (ArmorType)j));
            }

            for (int k = 0; k < 3; k++)
            {
                items.Add(CreateItem(ModelCreationHelper.TestItemType.Consumable, $"Consumable[{i}-{k}]", i, i + 1, i * 2.0M, consumable: (ConsumableType)k));
            }

            for (int x = 0; x < 5; x++)
            {
                items.Add(CreateItem(ModelCreationHelper.TestItemType.Valuable, $"Valuable[{i}-{x}]", i, i + 1, i * x * 1.0M));
            }
        }

        _context.Items.AddRange(items);
        _context.SaveChanges();
    }

    
    private void SeedRooms()
    {
        var rooms = new List<Room>
        {
            new Room
            {
                Name = "Entrance",
                Description = "A test entrance"
            }
        };

        for (int i = 1; i < 50; i++)
        {
            rooms.Add(new Room
            {
                Name = $"Test Room[{i}]",
                Description = $"Test description for room #{i}"
            });
        }

        // First save: insert rooms without connections.
        _context.Rooms.AddRange(rooms);
        _context.SaveChanges();

        // Second step: establish bidirectional connections.
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            rooms[i].East = rooms[i + 1];
            rooms[i + 1].West = rooms[i];
        }

        // Second save: update existing rooms with connections.
        _context.SaveChanges();
    }

    
    private static Skill CreateSkill(ModelCreationHelper.TestSkillType type, string name, int power, int level, StatType stat = StatType.Attack, TargetType target = TargetType.SingleEnemy, SupportEffectType effectType = SupportEffectType.Boost)
    {
        return type switch
        {
            ModelCreationHelper.TestSkillType.Martial =>
                new MartialSkill
                {
                    Name = name,
                    Description = $"Test description for {name}",
                    RequiredLevel = level,
                    Cooldown = 3,
                    ElapsedTime = 10,
                    Power = power,
                    Cost = 2,
                    TargetType = TargetType.SingleEnemy,
                    DamageType = DamageType.Martial,
                    SkillCategory = SkillCategory.Basic
                },
            ModelCreationHelper.TestSkillType.Magical =>
                new MagicSkill
                {
                    Name = name,
                    Description = $"Test description for {name}",
                    RequiredLevel = level,
                    Cooldown = 3,
                    ElapsedTime = 10,
                    Power = power,
                    Cost = 2,
                    TargetType = TargetType.SingleEnemy,
                    DamageType = DamageType.Magical,
                    SkillCategory = SkillCategory.Basic
                },
            ModelCreationHelper.TestSkillType.Ultimate => new UltimateSkill
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = level,
                Cooldown = 3,
                ElapsedTime = 10,
                Power = power,
                Cost = 2,
                TargetType = TargetType.SingleEnemy,
                DamageType = DamageType.Hybrid,
                SkillCategory = SkillCategory.Ultimate
            },
            ModelCreationHelper.TestSkillType.Boss => new BossSkill
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = level,
                Cost = 10,
                Power = power,
                Cooldown = 8,
                TargetType = TargetType.SingleEnemy,
                DamageType = DamageType.Hybrid
            },
            _ => new SupportSkill
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = level,
                Cooldown = 4,
                ElapsedTime = 10,
                Power = power,
                Cost = 2,
                TargetType = target,
                SkillCategory = SkillCategory.Support,
                Duration = 2,
                StatAffected = stat,
                SupportEffect = effectType
            }
        };
    }
    
    private static Monster CreateMonster(ModelCreationHelper.TestMonsterType type, string name, int level, ThreatLevel threat, DamageType damage)
    {
        return type switch
        {
            ModelCreationHelper.TestMonsterType.Elite =>
                new EliteMonster
                {
                    Name = name,
                    Description = $"A description for {name}.",
                    Level = level,
                    MaxHealth = 100,
                    CurrentHealth = 100,
                    ThreatLevel = threat,
                    AggressionLevel = 5,
                    IsCore = false,
                    DefensePower = 10,
                    AttackPower = 15,
                    DamageType = damage,
                    Resistance = 5
                },
            ModelCreationHelper.TestMonsterType.Boss => new BossMonster
            {
                Name = name,
                Description = $"A description for {name}.",
                Level = level,
                MaxHealth = 100,
                CurrentHealth = 100,
                ThreatLevel = threat,
                AggressionLevel = 5,
                IsCore = false,
                DefensePower = 10,
                AttackPower = 15,
                DamageType = damage,
                Resistance = 5
            },
            _ => new Monster
            {
                Name = name,
                Description = $"A description for {name}.",
                Level = level,
                MaxHealth = 100,
                CurrentHealth = 100,
                ThreatLevel = threat,
                AggressionLevel = 5,
                IsCore = false,
                DefensePower = 10,
                AttackPower = 15,
                DamageType = damage,
                Resistance = 5,
                MonsterType = "Monster"
            }
        };
    }
    
    private static Item CreateItem(ModelCreationHelper.TestItemType type, string name, int level, int power, decimal value, 
        DamageType damage = 0, ArmorType armor = 0, ConsumableType consumable = 0)
    {
        return type switch
        {
            ModelCreationHelper.TestItemType.Weapon => new Weapon
            {
                Name = name,
                Description = "A test weapon.",
                Value = value,
                RequiredLevel = level,
                DamageType = damage,
                AttackPower = power,
                Durability = 3
            },
            ModelCreationHelper.TestItemType.Armor => new Armor
            {
                Name = name,
                Description = "A test armor.",
                Value = value,
                RequiredLevel = level,
                DefensePower = power,
                Resistance = power / 2,
                ArmorType = armor,
                Durability = 3
            },
            ModelCreationHelper.TestItemType.Consumable => new Consumable()
            {
                Name = name,
                Description = "A test consumable.",
                Value = value,
                RequiredLevel = level,
                ConsumableType = consumable,
                Power = power,
                Durability = 3
            },
            _ => new Valuable()
            {
                Name = name,
                Description = "A test valuable.",
                Value = value,
                RequiredLevel = level,
                Durability = 3
            }
        };
    }
    
    ///
    /// FLOOR CREATION AND CONFIGURATION
    ///
    [TestMethod]
    public void CreateFloor_Adventure_GeneratesValidFloor()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, false, false);

        Assert.IsNotNull(floor);
        Assert.AreEqual(level, floor.Level);
        Assert.IsNotNull(floor.Rooms);
        Assert.IsNotNull(floor.Monsters);
        Assert.IsNotNull(floor.Loot);
        Assert.IsTrue(floor.Rooms.Count > 1);
        Assert.IsTrue(floor.Monsters.Count > 0);
    }

    [TestMethod]
    public void CreateFloor_Campaign_GeneratesValidFloor()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, true, false);

        Assert.IsNotNull(floor);
        Assert.AreEqual(level, floor.Level);
        Assert.IsNotNull(floor.Rooms);
        Assert.IsNotNull(floor.Monsters);
        Assert.IsNotNull(floor.Loot);
        Assert.IsTrue(floor.Rooms.Count > 1);
        Assert.IsTrue(floor.Monsters.Count > 0);
    }

    [TestMethod]
    public void CreateFloor_RandomMap_GeneratesValidFloor()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, false, true);

        Assert.IsNotNull(floor);
        Assert.AreEqual(level, floor.Level);
        Assert.IsNotNull(floor.Rooms);
        Assert.IsNotNull(floor.Monsters);
        Assert.IsNotNull(floor.Loot);
        Assert.IsTrue(floor.Rooms.Count > 1);
        Assert.IsTrue(floor.Monsters.Count > 0);
    }

    [TestMethod]
    public void CreateFloor_MultipleLevels_GeneratesValidFloors()
    {
        int[] levels = [1, 3, 5, 7, 9];

        foreach (var level in levels)
        {
            var floor = _floorFactory.CreateFloor(level, false, true);

            Assert.IsNotNull(floor);
            Assert.AreEqual(level, floor.Level);
            Assert.IsTrue(floor.Rooms.Count > 1,
                $"Floor {level} should contain multiple rooms.");
            Assert.IsTrue(floor.Monsters.Count > 0,
                $"Floor {level} should contain monsters.");
            Assert.IsNotNull(floor.Loot);
        }
    }

    ///
    /// MONSTER GENERATION AND ROOM ASSIGNMENT
    ///
    [TestMethod]
    public void CreateFloor_GeneratedMonsters_AreAssignedToRooms()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        Assert.IsTrue(floor.Monsters.Count > 0);

        foreach (var monster in floor.Monsters)
        {
            Assert.IsNotNull(monster.Room,
                $"Monster {monster.Name} has no assigned room.");

            Assert.IsTrue(floor.Rooms.Contains(monster.Room),
                $"Monster {monster.Name} was assigned to a room outside the floor.");
        }
    }

    [TestMethod]
    public void CreateFloor_EntranceRoom_ContainsNoMonsters()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var entrance = floor.Rooms.Single(r => r.Name == "Entrance");

        Assert.AreEqual(0, entrance.Monsters.Count);

        Assert.IsFalse(floor.Monsters.Any(m => m.Room == entrance));
    }

    [TestMethod]
    public void CreateFloor_RoomMonsterCollections_MatchMonsterAssignments()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        foreach (var monster in floor.Monsters)
        {
            Assert.IsNotNull(monster.Room);

            Assert.AreEqual(1,
                monster.Room.Monsters.Count(m => ReferenceEquals(m, monster)),
                $"Monster {monster.Name} should appear exactly once in its assigned room.");
        }

        foreach (var room in floor.Rooms)
        {
            foreach (var monster in room.Monsters)
            {
                Assert.IsTrue(floor.Monsters.Contains(monster),
                    $"Room {room.Name} contains a monster not belonging to the floor.");

                Assert.AreSame(room, monster.Room,
                    $"Monster {monster.Name} has an inconsistent room reference.");
            }
        }
    }

    [TestMethod]
    public void CreateFloor_EliteMonsters_OccupyDistinctRooms()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var elites = floor.Monsters.OfType<EliteMonster>().ToList();

        foreach (var elite in elites)
        {
            Assert.IsNotNull(elite.Room,
                $"Elite monster {elite.Name} has no assigned room.");
        }

        Assert.AreEqual(
            elites.Count,
            elites.Select(e => e.Room).Distinct().Count(),
            "Multiple elite monsters were assigned to the same room.");
    }

    [TestMethod]
    public void CreateFloor_EliteMonsters_DoNotShareRoomsWithRegularMonsters()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var elites = floor.Monsters.OfType<EliteMonster>().ToList();

        foreach (var elite in elites)
        {
            Assert.IsNotNull(elite.Room);

            Assert.IsFalse(
                elite.Room.Monsters.Any(m => m is not EliteMonster),
                $"Elite monster {elite.Name} shares a room with a regular monster.");
        }
    }

    [TestMethod]
    public void CreateFloor_EliteMonsterCount_DoesNotExceedRoomLimit()
    {
        int[] levels = [1, 3, 5, 7, 9];

        foreach (var level in levels)
        {
            var floor = _floorFactory.CreateFloor(level, false, true);

            var eliteCount = floor.Monsters.OfType<EliteMonster>().Count();
            var maximumElites = floor.Rooms.Count / 5;

            Assert.IsTrue(
                eliteCount <= maximumElites,
                $"Floor {level} generated {eliteCount} elites " +
                $"with {floor.Rooms.Count} rooms. Maximum allowed: {maximumElites}.");
        }
    }

    ///
    /// MONSTER SKILLS AND PROGRESSION
    ///
    
    [TestMethod]
    public void CreateFloor_GeneratedMonsters_HaveValidLevels()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, false, true);

        Assert.IsTrue(floor.Monsters.Count > 0);

        foreach (var monster in floor.Monsters)
        {
            Assert.IsTrue(
                monster.Level >= 1 && monster.Level <= level,
                $"Monster {monster.Name} has level {monster.Level} " +
                $"on floor {level}.");
        }
    }

    [TestMethod]
    public void CreateFloor_GeneratedMonsters_HaveValidSkills()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        Assert.IsTrue(floor.Monsters.Count > 0);

        foreach (var monster in floor.Monsters)
        {
            Assert.IsNotNull(monster.Skills);

            foreach (var skill in monster.Skills)
            {
                Assert.IsTrue(
                    skill.RequiredLevel <= monster.Level,
                    $"Monster {monster.Name} (level {monster.Level}) " +
                    $"has skill {skill.Name} requiring level {skill.RequiredLevel}.");
            }
        }
    }

    [TestMethod]
    public void CreateFloor_EliteMonsters_HaveUltimateSkills()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var elites = floor.Monsters.OfType<EliteMonster>().ToList();

        foreach (var elite in elites)
        {
            Assert.IsTrue(
                elite.Skills.OfType<UltimateSkill>().Any(),
                $"Elite monster {elite.Name} has no ultimate skill.");
        }
    }

    [TestMethod]
    public void CreateFloor_GeneratedMonsters_HaveInitializedStrategies()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        Assert.IsTrue(floor.Monsters.Count > 0);

        foreach (var monster in floor.Monsters)
        {
            Assert.IsNotNull(
                monster.Strategy,
                $"Monster {monster.Name} has no initialized strategy.");
        }
    }

    ///
    /// LOOT AND TREASURE ASSIGNMENT
    /// 
    
    [TestMethod]
    public void CreateFloor_GeneratedLoot_HasValidRequiredLevels()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, false, true);

        foreach (var item in floor.Loot)
        {
            Assert.IsTrue(
                item.RequiredLevel <= level,
                $"Item {item.Name} requires level {item.RequiredLevel} " +
                $"but was generated for floor {level}.");
        }
    }

    
    [TestMethod]
    public void CreateFloor_AssignedTreasure_BelongsToFloorMonsters()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var assignedTreasures = floor.Monsters
            .Where(m => m.Treasure != null)
            .Select(m => m.Treasure!)
            .ToList();

        foreach (var treasure in assignedTreasures)
        {
            Assert.IsTrue(
                floor.Loot.Contains(treasure),
                $"Assigned treasure {treasure.Name} is not part of floor loot.");
        }
    }


    
    [TestMethod]
    public void CreateFloor_MonsterTreasure_HasConsistentReferences()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var assignedTreasures = floor.Monsters
            .Where(m => m.Treasure != null)
            .Select(m => m.Treasure!)
            .ToList();

        Assert.AreEqual(
            floor.Loot.Count,
            assignedTreasures.Count,
            "Every generated loot item should be assigned to a monster.");

        foreach (var item in floor.Loot)
        {
            Assert.AreEqual(
                1,
                assignedTreasures.Count(t => ReferenceEquals(t, item)),
                $"Loot item {item.Name} should be assigned to exactly one monster.");
        }
    }

    [TestMethod]
    public void CreateFloor_TreasureItems_AreNotAssignedToMultipleMonsters()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var treasures = floor.Monsters
            .Where(m => m.Treasure != null)
            .Select(m => m.Treasure!)
            .ToList();

        Assert.AreEqual(
            treasures.Count,
            treasures.Distinct(ReferenceEqualityComparer.Instance).Count(),
            "The same treasure instance was assigned to multiple monsters.");
    }
    
    ///
    /// MERCHANT AND MAP INTEGRATION
    ///
    
    [TestMethod]
    public void CreateFloor_MerchantItems_AreUpdated()
    {
        const int level = 5;

        var floor = _floorFactory.CreateFloor(level, false, true);

        var merchantItems = floor.GetMerchantItems();

        Assert.IsNotNull(merchantItems);
        Assert.IsTrue(merchantItems.Count > 0);

        Assert.IsTrue(merchantItems.OfType<Consumable>().Any(),
            "Merchant should have at least one consumable.");

        Assert.IsTrue(merchantItems.OfType<Weapon>()
                .Any(w => w.DamageType == DamageType.Martial),
            "Merchant should have at least one martial weapon.");

        Assert.IsTrue(merchantItems.OfType<Weapon>()
                .Any(w => w.DamageType == DamageType.Magical),
            "Merchant should have at least one magical weapon.");

        Assert.IsTrue(merchantItems.OfType<Armor>().Any(),
            "Merchant should have at least one armor item.");

        foreach (var item in merchantItems)
        {
            Assert.IsTrue(
                item.RequiredLevel <= level,
                $"Merchant item {item.Name} requires level {item.RequiredLevel}, " +
                $"but the floor is level {level}.");
        }
    }
    
    [TestMethod]
    public void CreateFloor_GeneratedRooms_HaveValidConnections()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        Assert.IsTrue(floor.Rooms.Count > 1);

        foreach (var room in floor.Rooms)
        {
            foreach (var connection in room.GetConnections())
            {
                Assert.IsTrue(
                    floor.Rooms.Contains(connection.Value),
                    $"Room {room.Name} has a {connection.Key} connection " +
                    $"to {connection.Value.Name}, which is not part of the floor.");
            }
        }
    }

    [TestMethod]
    public void CreateFloor_GeneratedRooms_AreReachableFromEntrance()
    {
        var floor = _floorFactory.CreateFloor(5, false, true);

        var entrance = floor.Rooms.Single(r => r.Name == "Entrance");

        var visited = new HashSet<Room> { entrance };
        var queue = new Queue<Room>();
        queue.Enqueue(entrance);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var connectedRoom in current.GetConnections().Values)
            {
                if (visited.Add(connectedRoom))
                    queue.Enqueue(connectedRoom);
            }
        }

        Assert.AreEqual(
            floor.Rooms.Count,
            visited.Count,
            "Not all rooms are reachable from the entrance.");
    }


}