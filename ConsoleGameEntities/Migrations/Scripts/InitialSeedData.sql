SET IDENTITY_INSERT Players ON;
INSERT INTO Players (Id, Name, Health, Experience)
VALUES
    (1, 'Sir Lancelot', 100, 0);
SET IDENTITY_INSERT Players OFF;

SET IDENTITY_INSERT Inventories ON;
INSERT INTO Inventories (Id, Gold, Capacity, PlayerId)
VALUES
    (1, 100, 34.7, 1);
SET IDENTITY_INSERT Inventories OFF;

SET IDENTITY_INSERT Monsters ON;
INSERT INTO Monsters (Id, Name, MonsterType, Health, AggressionLevel, Sneakiness)
VALUES
    (1, 'Bob Goblin', 'Goblin', 175, 10, 3);
SET IDENTITY_INSERT Monsters OFF;

SET IDENTITY_INSERT Abilities ON;
INSERT INTO Abilities (Id, Name, Description, AbilityType, Damage, Distance)
VALUES
    (1, 'Shove', 'Power Shove', 'ShoveAbility', 10, 5);
SET IDENTITY_INSERT Abilities OFF;

SET IDENTITY_INSERT Items ON;
INSERT INTO Items (Id, Name, Value, Description, Weight, InventoryId, ItemType, AttackPower, DefensePower, Durability)
VALUES
    (1, 'Starter Sword', 5.0, 'A basic sword', 12.4, 1, 'Weapon', 5, 0, 10),
    (2, 'Wooden Shield', 3.0, 'A cheap shield', 8.7, 1, 'Armor', 0, 3, 4),
    (3, 'Rusty Dagger', 0.7, 'A rusting dagger', 3.1, 1, 'Weapon', 2, 0, 3),
    (4, 'Leather Armor', 4.0, 'Basic leather armor', 4.3, 1, 'Armor', 0, 5, 10),
    (5, 'Iron Sword', 10.0, 'A solid iron sword', 6.5, null, 'Weapon', 8, 0, 15),
    (6, 'Steel Shield', 7.0, 'A strong steel shield', 7.5, null, 'Armor', 0, 5, 12),
    (7, 'Bronze Dagger', 1.5, 'A lightweight bronze dagger', 2.5, null, 'Weapon', 3, 0, 6),
    (8, 'Chainmail Armor', 6.5, 'Durable chainmail armor', 8.0, null, 'Armor', 0, 7, 18),
    (9, 'Golden Sword', 15.0, 'A golden sword with intricate designs', 5.0, null, 'Weapon', 12, 0, 20),
    (10, 'Platinum Shield', 12.0, 'A shield made of platinum', 6.0, null, 'Armor', 0, 8, 10),
    (11, 'Wooden Staff', 2.0, 'A simple wooden staff', 4.5, null, 'Weapon', 6, 0, 10),
    (12, 'Cloak of Invisibility', 25.0, 'A magical cloak that renders the wearer invisible', 1.2, null, 'Armor', 0, 0, 1),
    (13, 'Silver Dagger', 3.5, 'A sharp silver dagger', 3.0, null, 'Weapon', 6, 0, 8),
    (14, 'Fireproof Armor', 18.0, 'Armor that protects against fire damage', 10.0, null, 'Armor', 0, 12, 30),
    (15, 'Hunter''s Bow', 8.0, 'A bow used by expert hunters', 2.0, null, 'Weapon', 7, 0, 12),
    (16, 'Elven Shield', 9.0, 'An elegant shield used by elves', 5.5, null, 'Armor', 0, 5, 18),
    (17, 'Demon Blade', 22.0, 'A cursed blade with dark powers', 7.2, null, 'Weapon', 15, 0, 5),
    (18, 'Dragon Scale Armor', 40.0, 'Armor made from the scales of a dragon', 18.0, null, 'Armor', 0, 20, 40),
    (19, 'Vampire Fang Dagger', 11.0, 'A dagger with a fang from a vampire', 2.0, null, 'Weapon', 9, 0, 8),
    (20, 'Mage''s Cloak', 30.0, 'A cloak worn by powerful mages', 1.5, null, 'Armor', 0, 0, 2);
SET IDENTITY_INSERT Items OFF;


INSERT INTO PlayerAbilities (PlayersId, AbilitiesId)
VALUES
    (1, 1); 
