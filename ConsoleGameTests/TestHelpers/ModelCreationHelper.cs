using ConsoleGameEntities.Models.Entities;
using static ConsoleGameEntities.Models.Entities.ModelEnums;
using ConsoleGameEntities.Models.Items;
using ConsoleGameEntities.Models.Monsters;
using ConsoleGameEntities.Models.Runes;
using ConsoleGameEntities.Models.Runes.Recipes;
using ConsoleGameEntities.Models.Skills;

namespace ConsoleGameTests.TestHelpers;

public static class ModelCreationHelper
{
    public enum TestItemType
    {
        Valuable,
        Weapon,
        Armor,
        Consumable
    }

    public enum TestRuneType
    {
        Weapon,
        Armor
    }

    public enum TestSkillType
    {
        Martial,
        Magical,
        Ultimate,
        Support,
        Boss
    }

    public enum TestMonsterType
    {
        Common,
        Elite,
        Boss
    }

    public static Inventory CreateInventory(int gold)
    {
        return new Inventory()
        {
            Gold = gold,
            Capacity = 10M,
            Items = [],
            Runes = [],
            Ingredients = []
        };
    }

    public static Ingredient CreateIngredient(
        string name,
        ComponentType component,
        RarityLevel? essenceRarity = null)
    {
        return new Ingredient
        {
            Name = name,
            ComponentType = component,
            EssenceRarity = essenceRarity
        };
    }

    public static MonsterDrop CreateMonsterDrop(ElementType element, ThreatLevel level,
        Ingredient ingredient, decimal rate)
    {
        return new MonsterDrop()
        {
            Element = element,
            ThreatLevel = level,
            Ingredient = ingredient,
            DropRate = rate
        };
    }

    public static Rune CreateRune(string name, int power, ElementType element, RarityLevel rarity, TestRuneType type)
    {
        return type switch
        {
            TestRuneType.Weapon => new WeaponRune()
            {
                Name = name,
                Power = power,
                Element = element,
                Rarity = rarity,
                Duration = 3
            },
            _ => new ArmorRune()
            {
                Name = name,
                Power = power,
                Element = element,
                Rarity = rarity
            }
        };
    }

    public static Archetype CreateArchetype(string name, ArchetypeType type)
    {
        return new Archetype
        {
            Name = name,
            Description = "A strong fighter.",
            Skills = [],
            HealthBase = 10,
            ArchetypeType = type,
            ResourceName = "Gemstones",
            MaxResource = 20,
            RecoveryRate = 5,
            RecoveryGrowth = 1,
            AttackMultiplier = 1.0M,
            MagicMultiplier = 0.5M,
            DefenseMultiplier = 1.0M,
            ResistanceMultiplier = 0.5M,
            SpeedMultiplier = 0.5M,
            ResourceMultiplier = 1.0M
        };
    }

    public static Player CreatePlayer(string name, Archetype archetype, Inventory inventory)
    {
        return new Player
        {
            Name = name,
            Level = 1,
            Archetype = archetype,
            Inventory = inventory
        };
    }

    public static Skill CreateSkill(string name, TestSkillType type)
    {
        return type switch
        {
            TestSkillType.Martial =>
                new MartialSkill
                {
                    Name = name,
                    Description = $"Test description for {name}",
                    RequiredLevel = 1,
                    Cooldown = 3,
                    ElapsedTime = 10,
                    Power = 10,
                    Cost = 2,
                    TargetType = TargetType.SingleEnemy,
                    DamageType = DamageType.Martial,
                    SkillCategory = SkillCategory.Basic
                },
            TestSkillType.Magical =>
                new MagicSkill
                {
                    Name = name,
                    Description = $"Test description for {name}",
                    RequiredLevel = 1,
                    Cooldown = 3,
                    ElapsedTime = 10,
                    Power = 10,
                    Cost = 2,
                    TargetType = TargetType.SingleEnemy,
                    DamageType = DamageType.Magical,
                    SkillCategory = SkillCategory.Basic
                },
            TestSkillType.Ultimate => new UltimateSkill()
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = 1,
                Cooldown = 3,
                ElapsedTime = 10,
                Power = 10,
                Cost = 2,
                TargetType = TargetType.SingleEnemy,
                DamageType = DamageType.Hybrid,
                SkillCategory = SkillCategory.Ultimate
            },
            TestSkillType.Boss => new BossSkill
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = 10,
                Cost = 10,
                Power = 50,
                Cooldown = 8,
                TargetType = TargetType.SingleEnemy,
                DamageType = DamageType.Hybrid
            },
            _ => new SupportSkill()
            {
                Name = name,
                Description = $"Test description for {name}",
                RequiredLevel = 1,
                Cooldown = 3,
                ElapsedTime = 10,
                Power = 10,
                Cost = 2,
                TargetType = TargetType.Self,
                SkillCategory = SkillCategory.Support,
                Duration = 4,
                StatAffected = StatType.Resistance,
                SupportEffect = SupportEffectType.Boost
            }
        };
    }

    public static Item CreateItem(string name, int level, int power, TestItemType type, ArmorType armor = 0,
        ConsumableType consumable = 0)
    {
        return type switch
        {
            TestItemType.Weapon => new Weapon
            {
                Name = name,
                Description = "A test weapon.",
                Value = 4.6M,
                RequiredLevel = level,
                DamageType = DamageType.Martial,
                AttackPower = power,
                Durability = 3
            },
            TestItemType.Armor => new Armor
            {
                Name = name,
                Description = "A test armor.",
                Value = 4.6M,
                RequiredLevel = level,
                DefensePower = power,
                ArmorType = armor,
                Durability = 3
            },
            TestItemType.Consumable => new Consumable()
            {
                Name = name,
                Description = "A test consumable.",
                Value = 4.6M,
                RequiredLevel = level,
                ConsumableType = consumable,
                Power = power,
                Durability = 3
            },
            _ => new Valuable()
            {
                Name = name,
                Description = "A test valuable.",
                Value = 4.6M,
                RequiredLevel = level,
                Durability = 3
            }
        };
    }

    public static Monster CreateMonster(string name, ThreatLevel level, DamageType damage, TestMonsterType type)
    {
        return type switch
        {
            TestMonsterType.Elite =>
                new EliteMonster
                {
                    Name = name,
                    Description = $"A description for {name}.",
                    Level = 1,
                    MaxHealth = 100,
                    CurrentHealth = 100,
                    ThreatLevel = level,
                    AggressionLevel = 5,
                    IsCore = false,
                    DefensePower = 10,
                    AttackPower = 15,
                    DamageType = damage,
                    Resistance = 5
                },
            TestMonsterType.Boss => new BossMonster
            {
                Name = name,
                Description = $"A description for {name}.",
                Level = 1,
                MaxHealth = 100,
                CurrentHealth = 100,
                ThreatLevel = level,
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
                Level = 1,
                MaxHealth = 100,
                CurrentHealth = 100,
                ThreatLevel = level,
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

    public static Room CreateRoom(string name = "Test Room")
    {
        return new Room(
            name,
            $"A description for {name}."
        );
    }
    
    public static Recipe CreateRecipe(
        string name,
        Rune rune,
        List<RecipeIngredient> ingredients)
    {
        return new Recipe
        {
            Name = name,
            Rune = rune,
            Ingredients = ingredients
        };
    }
    
    public static RecipeIngredient CreateRecipeIngredient(
        Ingredient ingredient,
        int quantity)
    {
        return new RecipeIngredient
        {
            Ingredient = ingredient,
            Quantity = quantity
        };
    }
}