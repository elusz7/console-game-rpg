using Microsoft.EntityFrameworkCore.Migrations;
using static ConsoleGameEntities.Models.Entities.ModelEnums;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class InitialSeedData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("SET IDENTITY_INSERT Rooms ON;");
        migrationBuilder.InsertData(
            table: "Rooms",
            columns: new[] { "Id", "Name", "Description", "NorthId", "SouthId", "EastId", "WestId" },
            values: new object[,]
            {
            {1, "Entrance", "Dust-choked air hangs heavy in this crumbling threshold where ivy creeps through fractured stone.", 50, null, 29, null},
            {2, "Alchemy Lab", "Shattered glass and rusted vials litter the worktables of a forgotten alchemist’s sanctum.", 19, 7, null, null},
            {3, "Archives", "Moldy scrolls and torn tomes lie scattered in sagging shelves, whispering secrets no one hears.", 26, 48, null, null},
            {4, "Armorer’s Workshop", "Broken tools and half-finished helms gather dust where once the clang of metal rang.", 47, null, 50, null},
            {5, "Armory", "Racks stand empty or rusted, the last weapons long looted or rotted into disrepair.", null, null, 21, null},
            {6, "Observatory", "A cracked lens faces the heavens, its dome roof letting in rain and moonlight alike.", null, 28, null, 31},
            {7, "Bakery", "Crumbling ovens and petrified loaves remain in this kitchen where warmth once thrived.", 2, 57, 46, null},
            {8, "Ballroom", "A grand hall of cracked marble where shadows waltz to music only they can hear.", null, 46, 56, null},
            {9, "Banner Hall", "Faded, tattered banners droop from beams above, the symbols of fallen houses barely legible.", 63, 37, 18, null},
            {10, "Barracks", "Empty bunks and discarded armor echo with the ghostly silence of long-gone soldiers.", 11, null, 14, 45},
            {11, "Bathhouse", "Mildewed tiles and stagnant water mark this once-luxurious retreat for nobility.", null, 10, null, 59},
            {12, "Parapets", "Wind howls over broken stone, where sentinels once stood watch against unseen foes.", 44, null, null, null},
            {13, "Forge", "All that's left of a once-blazing furnace are cold coals and the soot-stained anvil where master smiths once shaped weapons and armor.", 56, null, 20, null},
            {14, "Blood Altar", "Dried stains mark the cracked stone of an altar no prayers have reached in ages.", null, 24, 27, 10},
            {15, "Buttery", "Empty casks lie broken and rotted, their contents long spoiled or stolen.", null, null, 43, 21},
            {16, "Chapel", "Pews are overturned and the altar shattered, yet a strange hush still clings to the air.", null, 54, 57, null},
            {17, "Council Chamber", "Chairs are askew around a dust-covered table where silence reigns over forgotten councils.", null, 49, null, 58},
            {18, "Counting House", "Ledger books lay open in moldy decay, their ink run into unreadable trails.", null, 33, null, 9},
            {19, "Cursed Hall", "The air is colder here, and your footsteps echo too loudly, as if watched by unseen eyes.", null, (int)ArmorType.Arms, null, null},
            {20, "Divination Room", "Cracked crystal balls and scattered cards suggest omens too dark to bear.", 60, 59, null, 13},
            {21, "Dungeon", "Chains hang loosely from the damp, mossy walls of cells that remember every scream.", 52, 36, 15, 5},
            {22, "Gallery of Heroes", "Portraits gaze blankly, their faces faded and cracked beneath centuries of dust.", 28, null, 30, null},
            {23, "Garden Atrium", "Vines choke the statues and cracked fountains of a courtyard nature has reclaimed.", null, null, 39, null},
            {24, "Gatehouse", "The portcullis hangs at a crooked angle, groaning in the wind like a warning.", 14, null, null, 62},
            {25, "Great Hall", "Once a place of feasts, now an echoing ruin filled with shattered goblets and broken chairs.", 38, null, null, 43},
            {26, "Guardroom", "Empty weapon racks and a stale odor linger where sentries once gathered.", null, 3, null, 64},
            {27, "Hall of Mirrors", "Most mirrors are shattered, reflecting only fragments of those who dare pass through.", 29, null, 41, 14},
            {28, "Hall of Tapestries", "Moths feast on decaying fabric, the heroic scenes now just silhouettes and dust.", 6, 22, null, null},
            {29, "Kennels", "Cracked bones and rotting leashes remain in these empty cages.", null, 27, 53, (int)ConsumableType.Durability},
            {30, "Kitchen", "The hearth is cold, and overturned pots lie like silent witnesses to a hasty departure.", null, null, 40, 22},
            {31, "Lady's Chamber", "Perfume bottles sit empty on a dust-coated vanity beneath a broken mirror.", null, null, 6, 54},
            {32, "Larder", "Only cobwebs and gnawed bones remain in the rotted shelving of this dark pantry.", 34, 53, 63, null},
            {33, "Laundry Room", "Tattered linens and rusted tubs tell of chores abandoned in haste.", 18, null, 42, 37},
            {34, "Leyline Nexus", "A flicker of residual magic pulses in the cracked runes etched into the stone floor.", null, 32, null, 61},
            {35, "Library", "Books decay on splintered shelves, their spines curling like old bark.", 46, null, 44, null},
            {36, "Lord's Bedchamber", "A grand bed lies collapsed under moth-eaten sheets, its canopy torn by time.", 21, null, null, null},
            {37, "Map Room", "Charts are scattered across the floor, faded and curling with age and neglect.", 9, null, 33, null},
            {38, "Mirror Prison", "Each mirror hums faintly, the glass fogged as though breathing from within.", 42, 25, null, null},
            {39, "Music Room", "A broken harp and shattered lute rest beneath a collapsed chandelier.", 45, 40, null, 23},
            {40, "Nursery", "Dolls stare lifelessly from moldy cribs, their eyes cracked and smiles faded.", 39, null, null, 30},
            {41, "Pantry", "Sacks long split open leave trails of dust and rodent droppings across the stone.", null, 52, null, 27},
            {42, "Portal Room", "The runes around the archway flicker faintly, yearning for power that no longer flows.", null, 38, null, 33},
            {43, "Runestone Chamber", "Stones lie dark and inert, their glyphs worn beyond recognition.", null, null, 25, 15},
            {44, "Sanctum of Light", "The stained glass windows are shattered, their once-holy glow now dim and scattered.", null, 12, null, 35},
            {45, "Scullery", "A rusted pump and piles of broken dishware mark this servant’s corner.", null, 39, 10, null},
            {46, "Servants’ Quarters", "Straw mattresses rot atop rusted frames in these forgotten sleeping quarters.", 8, 35, null, 7},
            {47, "Shadow Sanctum", "Even in daylight, this room remains dim, its corners alive with creeping dread.", null, 4, 49, 60},
            {48, "Small, Rundown Shrine", "Time has cracked the idol, but offerings of withered flowers remain at its feet.", 3, 60, null, null},
            {49, "Siege Workshop", "Catapult parts lie rusting beneath tarps, as if waiting for a war that never came.", 17, null, null, 47},
            {50, "Solar", "Once a cozy retreat, this room is now a sunlit shell filled with dust motes and silence.", null, 1, null, 4},
            {51, "Stables", "The scent of old hay lingers in empty stalls where hooves once thundered.", null, 56, null, null},
            {52, "Storerooms", "Barrels and crates sit unopened, their contents long spoiled or pillaged.", 41, 21, null, null},
            {53, "Temple of the Ancients", "Time has worn away the carvings, but a weight of reverence still clings to the stones.", 32, null, null, 29},
            {54, "Amphitheater", "Crumbling steps surround a weed-choked stage where no voice has echoed in years.", 16, null, 31, null},
            {55, "Torture Chamber", "Chains still hang from the walls, and a stained table sits center-stage in silence.", null, 61, null, null},
            {56, "Treasury", "Broken lockboxes lie looted and overturned, their treasures long since stolen.", 51, 13, null, 8},
            {57, "Vault", "The iron door is ajar, its interior picked clean or hiding one last secret.", 7, null, null, 16},
            {58, "Vault of Secrets", "Whispering winds circle this sealed chamber, as if guarding truths better left buried.", null, null, 17, null},
            {59, "War Room", "A strategy table lies overturned, figurines scattered like the bones of fallen kings.", 20, null, 11, null},
            {60, "Watchtower", "From the shattered windows, the lands below look just as desolate.", 48, 20, 47, null},
            {61, "Weather Room", "The air is still charged here, and strange wind patterns rustle parchments on the floor.", 55, null, 34, null},
            {62, "Wizard's Tower", "Books float in midair or lie in ash heaps—residue of a spell gone horribly wrong.", null, null, 24, null},
            {63, "Workshop", "Tools hang untouched on rusted hooks, each one a memory of the craftsman who left.", null, 9, null, 32},
            {64, "Greenhouse", "Overgrowth has claimed every inch, broken glass letting in weeds and wildflowers alike.", null, null, 26, null}
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Rooms OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Archetypes ON;");
        migrationBuilder.InsertData(
            table: "Archetypes",
            columns: new[] { "Id", "Name", "Description", "ArchetypeType", "HealthBase", "AttackBonus", "AttackMultiplier", "MagicBonus", "MagicMultiplier", "DefenseBonus", "DefenseMultiplier", "Speed", "SpeedMultiplier", "ResourceName", "MaxResource", "ResourceMultiplier", "RecoveryRate", "RecoveryGrowth", "ResistanceBonus", "ResistanceMultiplier" },
            values: new object[,]
            {
              //   Id  Name       Description                                                                                      ArchetypeType         Health  AtkB  AtkM   MagB  MagM   DefB  DefM   Spd  SpdM   Resource    MaxRes  ResM   RecRate  RecGrow
                { 1,  "Warrior",  "The human equivalent of a Swiss Army knife—predictable, dependable, mildly threatening.",      (int)ArchetypeType.Martial,  10,     3,    0.4M,  0,    0M,    2,    0.6M,  2,   0.2M,  "Stamina",      5,      0.3M,  2,        2 , 2, 0.3M},
                { 2,  "Bruiser","Muscle-first problem-solver. Brain sold separately.",                                          (int)ArchetypeType.Martial,  12,     3,    0.6M,  0,    0M,    2,    0.2M,  1,   0.4M,  "Stamina",      4,      0.4M,  1,        2, 1, 0.2M },
                { 3,  "Rogue",    "Fast, sneaky, and slightly allergic to fair fights.",                                          (int)ArchetypeType.Martial,   8,     2,    0.2M,  0,    0M,    3,    0.4M,  3,   0.6M,  "Stamina",       8,      0.5M,  1,        1, 3, 0.4M },
                { 4,  "Mage",     "Soft as a marshmallow, but throws lightning like Zeus with a grudge.",                        (int)ArchetypeType.Magical,   6,     0,    0M,    3,    0.5M,  2,    0.2M,  3,   0.4M,  "Mana",       8,      0.5M,  1,        1, 3, 0.6M },
                { 5,  "Cleric",   "Can smite, heal, and guilt-trip—truly holy triple threat.",                                   (int)ArchetypeType.Magical,   8,     0,    0M,    2,    0.3M,  3,    0.6M,  1,   0.2M,  "Mana",     5,      0.3M,  2,        2, 2, 0.5M }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Archetypes OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Monsters ON;");
        migrationBuilder.InsertData(
        table: "Monsters",
        columns: new[] { "Id", "Name", "Description", "Level", "MaxHealth", "ThreatLevel", "AggressionLevel", "DefensePower", "AttackPower", "DamageType", "Resistance", "MonsterType" },
        values: new object[,]
        {
            // Level 1 - Low Threat
            {1, "Goblin", "A small, quick creature with a mischievous streak and a crude blade.", 1, 6, (int)ThreatLevel.Low, 7, 3, 7, (int)DamageType.Martial, 2, "Monster"},
            {2, "Zombie", "A slow-moving undead with an insatiable hunger for flesh.", 1, 12, (int)ThreatLevel.Low, 3, 2, 5, (int)DamageType.Martial, 3, "Monster"},
            {3, "Skeleton", "A reanimated skeleton warrior wielding old weapons with eerie precision.", 1, 9, (int)ThreatLevel.Low, 5, 4, 6, (int)DamageType.Martial, 4, "Monster"},

            // Level 1 - Medium Threat
            {4, "Tunnel Spider", "A venomous spider that ambushes prey from the dark tunnels it calls home.", 1, 10, (int)ThreatLevel.Medium, 5, 4, 8, (int)DamageType.Martial, 2, "Monster"},
            {5, "Gnoll Scout", "A hyena-like humanoid that hunts in packs, using sharp weapons and speed.", 1, 8, (int)ThreatLevel.Medium, 6, 3, 9, (int)DamageType.Magical, 3, "Monster"}, 

            // Level 1 - High Threat
            {6, "Dire Wolf", "A large, feral wolf with fangs sharp enough to tear armor apart.", 1, 14, (int)ThreatLevel.High, 8, 5, 10, (int)DamageType.Martial, 3, "Monster"}, 

            // Level 4 - Low Threat
            {7, "Swamp Rat", "A giant rat that thrives in filth and swamps, surprisingly aggressive.", 4, 18, (int)ThreatLevel.Low, 4, 5, 6, (int)DamageType.Martial, 2, "Monster"},
            {8, "Fire Beetle", "A beetle that glows with inner fire, sometimes igniting its foes.", 4, 15, (int)ThreatLevel.Low, 5, 4, 6, (int)DamageType.Magical, 4, "Monster"},
            {9, "Ghoul", "An undead that paralyzes with its claws and feasts on the living.", 4, 16, (int)ThreatLevel.Low, 3, 6, 6, (int)DamageType.Martial, 3, "Monster"}, 

            // Level 4 - Medium Threat
            {10, "Banshee", "A wailing spirit that drains life and chills the soul with its voice.", 4, 22, (int)ThreatLevel.Medium, 6, 5, 9, (int)DamageType.Magical, 4, "Monster"}, 

            // Level 4 - High Threat
            {11, "Hellhound", "A blazing canine from the underworld that burns with infernal fire.", 4, 28, (int)ThreatLevel.High, 7, 7, 12, (int)DamageType.Martial, 6, "Monster"},

            // Level 7 - Low Threat
            {12, "Harpy", "A winged predator with a hypnotic song and sharp talons.", 7, 32, (int)ThreatLevel.Low, 4, 6, 9, (int)DamageType.Magical, 5, "Monster"},
            {13, "Orc", "A brutal warrior driven by rage and brute strength.", 7, 28, (int)ThreatLevel.Low, 5, 5, 10, (int)DamageType.Martial, 3, "Monster"},
            {14, "Animated Armor", "An enchanted suit of armor that patrols ancient ruins.", 7, 30, (int)ThreatLevel.Low, 7, 8, 11, (int)DamageType.Magical, 5, "Monster"}, 

            // Level 7 - Medium Threat
            {15, "Spectre", "An incorporeal ghost that saps life with every touch.", 7, 35, (int)ThreatLevel.Medium, 7, 6, 14, (int)DamageType.Magical, 4, "Monster"},

            // Level 7 - High Threat
            {16, "Werewolf", "A savage beast cursed to transform under the moon and shred its prey.", 7, 45, (int)ThreatLevel.High, 8, 8, 16, (int)DamageType.Martial, 6, "Monster"},

            // Level 1 - Elite
            {17, "Tiger", "A fearsome predator with unmatched agility and power.", 1, 20, (int)ThreatLevel.Elite, 10, 10, 13, (int)DamageType.Martial, 5, "EliteMonster"},
            {18, "Apprentice", "A novice spellcaster with surprising command over magic.", 1, 20, (int)ThreatLevel.Elite, 10, 5, 13, (int)DamageType.Magical, 10, "EliteMonster"},
            {19, "Kappa", "A trickster water spirit known for its martial skill and cunning.", 1, 20, (int)ThreatLevel.Elite, 10, 7, 13, (int)DamageType.Hybrid, 7, "EliteMonster"},

            // Level 4 - Elite
            {20, "Sabretooth Tiger", "An ancient beast with long fangs and a thunderous roar.", 4, 35, (int)ThreatLevel.Elite, 14, 13, 17, (int)DamageType.Martial, 8, "EliteMonster"},
            {21, "Witch", "A cunning spellcaster with a deep knowledge of curses and illusions.", 4, 35, (int)ThreatLevel.Elite, 14, 8, 17, (int)DamageType.Magical, 13, "EliteMonster"},
            {22, "Tengu", "A mystical bird warrior that blends swordplay and magic.", 4, 35, (int)ThreatLevel.Elite, 14, 10, 17, (int)DamageType.Hybrid, 10, "EliteMonster"},

            // Level 7 - Elite
            {23, "Weretiger", "A deadly shapeshifter with the strength of a beast and the mind of a hunter.", 7, 60, (int)ThreatLevel.Elite, 16, 25, 21, (int)DamageType.Martial, 16, "EliteMonster"},
            {24, "Coven Leader", "The head of a sinister coven, weaving powerful spells with deadly grace.", 7, 60, (int)ThreatLevel.Elite, 16, 16, 21, (int)DamageType.Magical, 25, "EliteMonster"},
            {25, "Yuki Onna", "A chilling spirit of winter, freezing her victims with a mere glance.", 7, 60, (int)ThreatLevel.Elite, 16, 20, 21, (int)DamageType.Hybrid, 20, "EliteMonster"},

            // Bosses
            {26, "Shadowed Nogitsune", "A deceitful fox spirit cloaked in shadow and illusion.", 10, 100, (int)ThreatLevel.Boss, 22, 31, 27, (int)DamageType.Hybrid, 31, "BossMonster" },
            {27, "Ancient Lich", "A powerful undead sorcerer whose dark magic corrupts the air itself.", 10, 100, (int)ThreatLevel.Boss, 22, 28, 27, (int)DamageType.Magical, 35, "BossMonster" },
            {28, "Elder Vampire", "An ancient predator who thrives on blood and rules the night.", 10, 100, (int)ThreatLevel.Boss, 22, 35, 27, (int)DamageType.Martial, 28, "BossMonster" }
        });
        migrationBuilder.Sql("SET IDENTITY_INSERT Monsters OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Items ON;");
        migrationBuilder.InsertData(
        table: "Items",
        columns: new[] { "Id", "Name", "Description", "Value", "Durability", "Weight", "RequiredLevel", "ItemType", "AttackPower", "DamageType", "DefensePower", "Resistance", "ArmorType", "Power", "ConsumableType" },
        values: new object[,]
        {
            {1, "Sword", "A basic sword with a couple knicks in it", 5.94, 10, 3.62, 1, ItemType.Weapon.ToString(), 5, (int)DamageType.Martial, null, null, null, null, null},
            {2, "Spear", "A basic spear that leaves splinters in your hands", 5.08, 10, 3.77, 1, ItemType.Weapon.ToString(), 3, (int)DamageType.Martial, null, null, null, null, null},
            {3, "Axe", "A basic axe that's a little top heavy", 5.11, 10, 3.78, 1, ItemType.Weapon.ToString(), 3, (int)DamageType.Martial, null, null, null, null, null},
            {4, "Bow", "A  basic bow with a fraying bowstring", 5.81, 10, 2.57, 1, ItemType.Weapon.ToString(), 5, (int)DamageType.Martial, null, null, null, null, null},
            {5, "Rod", "A basic rod with a gem that's probably fake", 5.29, 10, 4.3, 1, ItemType.Weapon.ToString(), 3, (int)DamageType.Magical, null, null, null, null, null},
            {6, "Scepter", "A basic scepter that doesn't even make you feel regal", 5.56, 10, 4.1, 1, ItemType.Weapon.ToString(), 5, (int)DamageType.Magical, null, null, null, null, null},
            {7, "Staff", "A basic staff that looks more like a broom", 5.77, 10, 4.05, 1, ItemType.Weapon.ToString(), 5, (int)DamageType.Magical, null, null, null, null, null},
            {8, "Wand", "A basic wand that's really just a stick from off the ground", 5.3, 10, 3.2, 1, ItemType.Weapon.ToString(), 4, (int)DamageType.Magical, null, null, null, null, null},
            {9, "Falchion", " A single-edged chopping sword.", 15.83, 7, 5.83, 3, ItemType.Weapon.ToString(), 10, (int)DamageType.Martial, null, null, null, null, null},
            {10, "Cutlass", "A curved blade ideal for close combat.", 16.91, 5, 6.19, 3, ItemType.Weapon.ToString(), 12, (int)DamageType.Martial, null, null, null, null, null},
            {11, "Spatha", "A long, straight Roman sword.", 17.2, 6, 6.35, 3, ItemType.Weapon.ToString(), 12, (int)DamageType.Martial, null, null, null, null, null},
            {12, "Javelin", "A light throwing spear, great for ranged attacks.", 15.41, 6, 5.99, 3, ItemType.Weapon.ToString(), 10, (int)DamageType.Martial, null, null, null, null, null},
            {13, "Boar Spear", "A stout spear with a crossbar to stop charging foes.", 18.7, 9, 5.82, 3, ItemType.Weapon.ToString(), 14, (int)DamageType.Martial, null, null, null, null, null},
            {14, "Leaf-Blade Spear", "A broad-headed spear, balanced for versatility.", 18.62, 8, 5.87, 3, ItemType.Weapon.ToString(), 14, (int)DamageType.Martial, null, null, null, null, null},
            {15, "Hatchet", "A lightweight, one-handed axe useful for quick strikes.", 18.77, 8, 5.78, 3, ItemType.Weapon.ToString(), 14, (int)DamageType.Martial, null, null, null, null, null},
            {16, "Woodcutter’s Axe", "Heavier than a hatchet, practical and robust.", 18.04, 9, 5.68, 3, ItemType.Weapon.ToString(), 13, (int)DamageType.Martial, null, null, null, null, null},
            {17, "Blacksmith’s Hammer", "A sturdy, blunt weapon originally meant for forging.", 18.32, 9, 5.97, 3, ItemType.Weapon.ToString(), 13, (int)DamageType.Martial, null, null, null, null, null},
            {18, "Shortbow", "Compact and fast, ideal for quick, close-range shots.", 18.75, 7, 6.02, 3, ItemType.Weapon.ToString(), 14, (int)DamageType.Martial, null, null, null, null, null},
            {19, "Hunting Bow", "A simple wooden bow used for survival and tracking prey.", 17.87, 6, 6.17, 3, ItemType.Weapon.ToString(), 13, (int)DamageType.Martial, null, null, null, null, null},
            {20, "Slingbow", "A primitive bow-sling hybrid for light, arcing shots.", 17.84, 7, 5.8, 3, ItemType.Weapon.ToString(), 13, (int)DamageType.Martial, null, null, null, null, null},
            {21, "Willow Wand", "A slender wand crafted from willow, responsive to healing or water magic.", 16.72, 8, 6.15, 3, ItemType.Weapon.ToString(), 11, (int)DamageType.Magical, null, null, null, null, null},
            {22, "Apprentice’s Rod", "A basic rod carved with beginner runes, used in training.", 17.96, 7, 5.62, 3, ItemType.Weapon.ToString(), 13, (int)DamageType.Magical, null, null, null, null, null},
            {23, "Glimmering Scepter", "A polished silver scepter that faintly glows in moonlight.", 17.39, 8, 6.32, 3, ItemType.Weapon.ToString(), 12, (int)DamageType.Magical, null, null, null, null, null},
            {24, "Driftwood Staff", "A crooked staff of sea-worn wood, favored by coastal mages.", 16.33, 5, 6.25, 3, ItemType.Weapon.ToString(), 11, (int)DamageType.Magical, null, null, null, null, null},
            {25, "Sparkwand", "Flickers with small sparks; ideal for novice evocation magic.", 16.32, 5, 5.73, 3, ItemType.Weapon.ToString(), 11, (int)DamageType.Magical, null, null, null, null, null},
            {26, "Longsword", "A balanced, versatile blade.", 39.71, 8, 8.81, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Martial, null, null, null, null, null},
            {27, "Scimitar", "A curved, elegant slashing weapon.", 42.3, 6, 8.58, 5, ItemType.Weapon.ToString(), 24, (int)DamageType.Martial, null, null, null, null, null},
            {28, "Gladius", "A short, powerful thrusting sword.", 45.94, 6, 8.8, 5, ItemType.Weapon.ToString(), 27, (int)DamageType.Martial, null, null, null, null, null},
            {29, "Glaive", "A polearm with a sword-like blade for sweeping strikes.", 44.49, 6, 8.92, 5, ItemType.Weapon.ToString(), 26, (int)DamageType.Martial, null, null, null, null, null},
            {30, "Trident", "A three-pronged spear used for both combat and ceremony.", 43.81, 9, 8.25, 5, ItemType.Weapon.ToString(), 25, (int)DamageType.Martial, null, null, null, null, null},
            {31, "Pike", "A very long spear designed for reach and crowd control.", 45.97, 8, 8.35, 5, ItemType.Weapon.ToString(), 27, (int)DamageType.Martial, null, null, null, null, null},
            {32, "Battleaxe", "A classic double-bladed axe designed for war.", 42.45, 9, 8.51, 5, ItemType.Weapon.ToString(), 24, (int)DamageType.Martial, null, null, null, null, null},
            {33, "Warhammer", "Compact with a blunt head, ideal for breaking through armor.", 40.85, 7, 8.95, 5, ItemType.Weapon.ToString(), 23, (int)DamageType.Martial, null, null, null, null, null},
            {34, "Bearded Axe", "A Norse-style axe with a hooked lower blade, great for hooking shields.", 40.67, 6, 7.58, 5, ItemType.Weapon.ToString(), 23, (int)DamageType.Martial, null, null, null, null, null},
            {35, "Longbow", "Great range and power, requires strength and skill to use effectively.", 39.76, 8, 7.86, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Martial, null, null, null, null, null},
            {36, "Recurve Bow", "Curved limbs for more power in a compact frame.", 39.83, 7, 8.28, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Martial, null, null, null, null, null},
            {37, "Crossbow", "Easy to aim and shoot, trades speed for precision and power.", 39.59, 5, 7.69, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Martial, null, null, null, null, null},
            {38, "Fireroot Wand", "Crafted from a tree struck by lightning; hums with residual heat.", 39.52, 6, 7.81, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Magical, null, null, null, null, null},
            {39, "Engraved Rod", "Heavy rod inlaid with arcane script that pulses faintly with power.", 44.75, 9, 7.95, 5, ItemType.Weapon.ToString(), 26, (int)DamageType.Magical, null, null, null, null, null},
            {40, "Sunmetal Scepter", "Forged from golden alloy, resonates with radiant energy.", 38.35, 5, 7.74, 5, ItemType.Weapon.ToString(), 21, (int)DamageType.Magical, null, null, null, null, null},
            {41, "Spiritwood Staff", "Crafted from an ancient tree touched by fey spirits.", 38.47, 6, 7.74, 5, ItemType.Weapon.ToString(), 21, (int)DamageType.Magical, null, null, null, null, null},
            {42, "Orb of Echoes", "A crystal orb set in a clawed base; amplifies voice and spell.", 39.71, 9, 8.94, 5, ItemType.Weapon.ToString(), 22, (int)DamageType.Magical, null, null, null, null, null},
            {43, "Claymore", "A large, two-handed greatsword.", 86.39, 9, 11.74, 7, ItemType.Weapon.ToString(), 40, (int)DamageType.Martial, null, null, null, null, null},
            {44, "Katana", "A finely crafted, razor-sharp sword.", 78.92, 6, 10.78, 7, ItemType.Weapon.ToString(), 36, (int)DamageType.Martial, null, null, null, null, null},
            {45, "Tachi", "A longer, cavalry-style curved sword.", 91.44, 7, 10.68, 7, ItemType.Weapon.ToString(), 43, (int)DamageType.Martial, null, null, null, null, null},
            {46, "Partisan", "A broad, winged spear designed to parry and thrust.", 87.93, 5, 11.14, 7, ItemType.Weapon.ToString(), 41, (int)DamageType.Martial, null, null, null, null, null},
            {47, "Halberd", "A spear-axe hybrid with cutting, hooking, and stabbing potential.", 89.57, 5, 11.38, 7, ItemType.Weapon.ToString(), 42, (int)DamageType.Martial, null, null, null, null, null},
            {48, "Yari", "A straight-bladed Japanese spear used for precise thrusts.", 87.98, 6, 10.75, 7, ItemType.Weapon.ToString(), 41, (int)DamageType.Martial, null, null, null, null, null},
            {49, "Executioner’s Axe", "A massive, heavy axe with a wide blade for powerful cleaves.", 88.1, 9, 11.3, 7, ItemType.Weapon.ToString(), 41, (int)DamageType.Martial, null, null, null, null, null},
            {50, "Maul", "A huge, sledge-like hammer used to smash enemies with sheer force.", 87.74, 5, 11.93, 7, ItemType.Weapon.ToString(), 41, (int)DamageType.Martial, null, null, null, null, null},
            {51, "Spiked Mace", "Technically a hammer-type weapon, perfect for brutal, crushing blows.", 80.87, 7, 10.91, 7, ItemType.Weapon.ToString(), 37, (int)DamageType.Martial, null, null, null, null, null},
            {52, "Composite Bow", "Made of layered materials, combining flexibility and power.", 79.22, 8, 11.38, 7, ItemType.Weapon.ToString(), 36, (int)DamageType.Martial, null, null, null, null, null},
            {53, "Flatbow", "A wide-limbed bow known for stability and accuracy.", 82.5, 6, 11.46, 7, ItemType.Weapon.ToString(), 38, (int)DamageType.Martial, null, null, null, null, null},
            {54, "Repeating Crossbow", "A rare, rapid-fire version with a loading mechanism.", 80.68, 6, 11.17, 7, ItemType.Weapon.ToString(), 37, (int)DamageType.Martial, null, null, null, null, null},
            {55, "Runed Wand", "Carved with channeling glyphs for focused spellcasting.", 93.25, 6, 11.2, 7, ItemType.Weapon.ToString(), 44, (int)DamageType.Magical, null, null, null, null, null},
            {56, "Voidstone Rod", "Made of obsidian-like stone; ideal for shadow or necrotic spells.", 79.07, 7, 11.11, 7, ItemType.Weapon.ToString(), 36, (int)DamageType.Magical, null, null, null, null, null},
            {57, "Moonrise Scepter", "Emits a pale glow under moonlight, enhances illusion magic.", 91.45, 5, 11.77, 7, ItemType.Weapon.ToString(), 43, (int)DamageType.Magical, null, null, null, null, null},
            {58, "Shaman’s Totem Staff", "Decorated with feathers and bones, connects to spirit realms.", 86.01, 5, 11.31, 7, ItemType.Weapon.ToString(), 40, (int)DamageType.Magical, null, null, null, null, null},
            {59, "Orb of Resonance", "Amplifies divination and communication spells.", 79.02, 5, 11.55, 7, ItemType.Weapon.ToString(), 36, (int)DamageType.Magical, null, null, null, null, null},
            {60, "Zweihander", "A massive, armor-breaking greatsword.", 150.51, 5, 14.54, 9, ItemType.Weapon.ToString(), 58, (int)DamageType.Martial, null, null, null, null, null},
            {61, "Rapier", "A refined duelist’s weapon.", 148.08, 5, 15.63, 9, ItemType.Weapon.ToString(), 57, (int)DamageType.Martial, null, null, null, null, null},
            {62, "Dao", "A curved sword with cultural and mystical roots.", 143.86, 6, 14.51, 9, ItemType.Weapon.ToString(), 55, (int)DamageType.Martial, null, null, null, null, null},
            {63, "Lucerne Hammer", "A Swiss polearm combining a hammer, spike, and hook. Excellent against armor.", 146.05, 6, 15.67, 9, ItemType.Weapon.ToString(), 56, (int)DamageType.Martial, null, null, null, null, null},
            {64, "Naginata", "A Japanese polearm with a curved blade, ideal for sweeping attacks and reach.", 152.82, 5, 15.6, 9, ItemType.Weapon.ToString(), 59, (int)DamageType.Martial, null, null, null, null, null},
            {65, "Voulge", "A broad-bladed polearm similar to a cleaver, effective in both slashing and thrusting.", 144.04, 8, 15.51, 9, ItemType.Weapon.ToString(), 55, (int)DamageType.Martial, null, null, null, null, null},
            {66, "Dane Axe", "A long-handled Viking axe, known for reach and deadly cuts.", 164.12, 8, 14.66, 9, ItemType.Weapon.ToString(), 64, (int)DamageType.Martial, null, null, null, null, null},
            {67, "Bec de Corbin", "A polehammer with a spiked beak, perfect for armored foes.", 157.39, 5, 15.29, 9, ItemType.Weapon.ToString(), 61, (int)DamageType.Martial, null, null, null, null, null},
            {68, "Labrys", "A ceremonial yet deadly double-headed axe from the Greek Amazons.", 162.14, 7, 14.85, 9, ItemType.Weapon.ToString(), 63, (int)DamageType.Martial, null, null, null, null, null},
            {69, "Elven Warbow", "Elegant and magically enhanced for speed and precision.", 161.89, 5, 15.24, 9, ItemType.Weapon.ToString(), 63, (int)DamageType.Martial, null, null, null, null, null},
            {70, "Stormpiercer", "A high-tension bow rumored to shoot with thunderous force.", 153.12, 7, 14.82, 9, ItemType.Weapon.ToString(), 59, (int)DamageType.Magical, null, null, null, null, null},
            {71, "Runed Crossbow", "A finely crafted crossbow inscribed with runes for extra power.", 148.47, 6, 14.49, 9, ItemType.Weapon.ToString(), 57, (int)DamageType.Magical, null, null, null, null, null},
            {72, "Wand of Stormcall", "Crackles with electricity; forged in a hurricane.", 148.6, 7, 14.76, 9, ItemType.Weapon.ToString(), 57, (int)DamageType.Magical, null, null, null, null, null},
            {73, "Rod of the Depths", "Deep green and blue rod; forged in the oceanic abyss.", 143.76, 6, 15.39, 9, ItemType.Weapon.ToString(), 55, (int)DamageType.Magical, null, null, null, null, null},
            {74, "Scepter of Authority", "Symbol of magical leadership, a true object of power.", 157.34, 7, 14.57, 9, ItemType.Weapon.ToString(), 61, (int)DamageType.Magical, null, null, null, null, null},
            {75, "Celestial Staff", "Carved from starlight wood, aligns with radiant and astral magic.", 146.02, 6, 14.19, 9, ItemType.Weapon.ToString(), 56, (int)DamageType.Magical, null, null, null, null, null},
            {76, "Orb of Chaos", "A swirling, shifting orb that draws out full magical ability.", 153.34, 9, 14.85, 9, ItemType.Weapon.ToString(), 59, (int)DamageType.Magical, null, null, null, null, null},
            {77, "Bracers", "Simple leather bracers that offer limited defense for your forearms.", 5.51, 5, 5.1, 1, ItemType.Armor.ToString(), null, null, 2, 1, (int)ArmorType.Arms, null, null},
            {78, "Cloth Sleeves", "Light cloth sleeves, offering no real protection but easy to wear.", 5.47, 5, 5.43, 1, ItemType.Armor.ToString(), null, null, 2, 1, (int)ArmorType.Arms, null, null},
            {79, "Leather Armor", "Basic armor made of flexible leather. It won't stop a heavy blow, but it’s better than nothing.", 5.18, 5, 5.3, 1, ItemType.Armor.ToString(), null, null, 2, 1, (int)ArmorType.Chest, null, null},
            {80, "Padded Armor", "Thickly quilted fabric designed to absorb blows, but still leaves you vulnerable to piercing strikes.", 5.09, 5, 4.85, 1, ItemType.Armor.ToString(), null, null, 1, 2, (int)ArmorType.Chest, null, null},
            {81, "Cloth Hood", "A soft hood, more for warmth than protection, but it does help hide your face.", 5.02, 5, 5.38, 1, ItemType.Armor.ToString(), null, null, 2, 1, (int)ArmorType.Head, null, null},
            {82, "Simple Helmet", "A basic helmet that covers your head, but offers little in the way of defense.", 5.35, 5, 5.39, 1, ItemType.Armor.ToString(), null, null, 1, 2, (int)ArmorType.Head, null, null},
            {83, "Cloth Leggings", "Light fabric leggings, not much defense, but they won’t slow you down.", 5.52, 5, 4.51, 1, ItemType.Armor.ToString(), null, null, 3, 0, (int)ArmorType.Legs, null, null},
            {84, "Leather Pants", "Sturdy pants made from tanned leather, offering minimal protection but decent mobility.", 5.37, 5, 4.81, 1, ItemType.Armor.ToString(), null, null, 0, 3, (int)ArmorType.Legs, null, null},
            {85, "Chain Sleeves", "Armored sleeves made of interwoven metal rings, offering better protection for your arms.", 16.14, 6, 8.87, 3, ItemType.Armor.ToString(), null, null, 0, 8, (int)ArmorType.Arms, null, null},
            {86, "Studded Leather Bracers", "Leather bracers with metal studs, offering increased protection for your forearms.", 16.45, 8, 9.21, 3, ItemType.Armor.ToString(), null, null, 2, 6, (int)ArmorType.Arms, null, null},
            {87, "Chain Shirt", "A shirt of interwoven metal rings, offering good protection without too much weight.", 16.03, 5, 9.08, 3, ItemType.Armor.ToString(), null, null, 2, 6, (int)ArmorType.Chest, null, null},
            {88, "Studded Leather Armor", "Leather armor with small metal studs, providing better defense than standard leather.", 16.48, 6, 8.56, 3, ItemType.Armor.ToString(), null, null, 0, 8, (int)ArmorType.Chest, null, null},
            {89, "Iron Helmet", "A heavy helmet made of iron, offering solid protection against blunt force.", 16.16, 5, 8.53, 3, ItemType.Armor.ToString(), null, null, 1, 7, (int)ArmorType.Head, null, null},
            {90, "Leather Cap", "A basic leather cap, not much for defense, but good for covering your head.", 16.35, 6, 9.31, 3, ItemType.Armor.ToString(), null, null, 2, 6, (int)ArmorType.Head, null, null},
            {91, "Scale Mail", "Metal scales sewn onto fabric, providing solid defense with a bit of flexibility.", 16.15, 5, 9.49, 3, ItemType.Armor.ToString(), null, null, 6, 2, (int)ArmorType.Legs, null, null},
            {92, "Studded Leather Pants", "Leather pants reinforced with metal studs, offering more defense than regular leather.", 16.21, 4, 9.05, 3, ItemType.Armor.ToString(), null, null, 5, 3, (int)ArmorType.Legs, null, null},
            {93, "Full Plate Bracers", "Bracers made of solid metal, offering maximum protection for your forearms.", 42.04, 7, 12.07, 5, ItemType.Armor.ToString(), null, null, 10, 8, (int)ArmorType.Arms, null, null},
            {94, "Reinforced Leather Armguards", "Leather armguards reinforced with metal, offering a balance of flexibility and defense.", 42.21, 6, 12.4, 5, ItemType.Armor.ToString(), null, null, 11, 7, (int)ArmorType.Arms, null, null},
            {95, "Breastplate", "A sturdy metal chestplate that provides good protection without restricting movement too much.", 42.03, 8, 11.96, 5, ItemType.Armor.ToString(), null, null, 11, 7, (int)ArmorType.Chest, null, null},
            {96, "Half-Plate", "A suit of armor that covers the torso and shoulders with a mix of plate and chain, offering solid protection.", 41.99, 4, 12.13, 5, ItemType.Armor.ToString(), null, null, 5, 13, (int)ArmorType.Chest, null, null},
            {97, "Steel Helm", "A heavy, steel helmet that offers excellent protection for your head.", 41.53, 3, 12.17, 5, ItemType.Armor.ToString(), null, null, 15, 3, (int)ArmorType.Head, null, null},
            {98, "Visored Helmet", "A helmet with a movable visor, offering good head protection with added versatility.", 42.5, 8, 11.91, 5, ItemType.Armor.ToString(), null, null, 4, 14, (int)ArmorType.Head, null, null},
            {99, "Chainmail Pants", "A pair of pants made of interwoven metal rings, offering defense for your legs without too much weight.", 42.06, 8, 11.64, 5, ItemType.Armor.ToString(), null, null, 4, 14, (int)ArmorType.Legs, null, null},
            {100, "Plate Mail Greaves", "Heavy, full-leg armor made of solid metal, offering excellent protection for your legs.", 41.91, 6, 11.75, 5, ItemType.Armor.ToString(), null, null, 5, 13, (int)ArmorType.Legs, null, null},
            {101, "Full Plate Gauntlets", "Heavy, full-arm gauntlets made of plate metal, providing top-notch protection for your hands and wrists.", 92.8, 5, 15.45, 7, ItemType.Armor.ToString(), null, null, 2, 31, (int)ArmorType.Arms, null, null},
            {102, "Mithral Bracers", "Bracers made from mithral, offering excellent protection without the weight of regular metal.", 92.13, 3, 15.15, 7, ItemType.Armor.ToString(), null, null, 22, 11, (int)ArmorType.Arms, null, null},
            {103, "Dragonhide Armor", "Armor crafted from the hide of a dragon, offering both protection and a touch of the dragon’s magical resistance.", 92.74, 8, 15.29, 7, ItemType.Armor.ToString(), null, null, 29, 4, (int)ArmorType.Chest, null, null},
            {104, "Plate Armor", "Full-body armor made of solid metal plates, providing superior defense but limiting mobility.", 92.66, 8, 14.61, 7, ItemType.Armor.ToString(), null, null, 0, 33, (int)ArmorType.Chest, null, null},
            {105, "Dragonbone Crown", "A crown made from the bones of a dragon, offering a magical aura and protection for the wearer’s head.", 92.82, 5, 15.2, 7, ItemType.Armor.ToString(), null, null, 22, 11, (int)ArmorType.Head, null, null},
            {106, "Full Helm", "A complete, solid helmet that covers the entire head, offering maximum protection.", 92.58, 7, 15.11, 7, ItemType.Armor.ToString(), null, null, 3, 30, (int)ArmorType.Head, null, null},
            {107, "Mithral Leggings", "Leggings made from lightweight mithral, offering full protection without slowing you down.", 92.87, 8, 15.22, 7, ItemType.Armor.ToString(), null, null, 28, 5, (int)ArmorType.Legs, null, null},
            {108, "Plate Leggings", "Full-leg armor made of plate metal, offering solid protection for your legs.", 92.54, 3, 14.96, 7, ItemType.Armor.ToString(), null, null, 2, 31, (int)ArmorType.Legs, null, null},
            {109, "Celestial Bracers", "Bracers imbued with celestial magic, offering protection and a hint of divine grace.", 174.66, 7, 20.6, 9, ItemType.Armor.ToString(), null, null, 46, 6, (int)ArmorType.Arms, null, null},
            {110, "Enchanted Gauntlets", "Gauntlets imbued with magic that enhances your strength and offers protection to your hands.", 174.89, 8, 20.46, 9, ItemType.Armor.ToString(), null, null, 5, 47, (int)ArmorType.Arms, null, null},
            {111, "Adamantine Armor", "Armor made of the nearly indestructible metal, offering superior protection against physical damage.", 174.42, 4, 20.48, 9, ItemType.Armor.ToString(), null, null, 16, 36, (int)ArmorType.Chest, null, null},
            {112, "Celestial Armor", "Armor imbued with celestial magic, offering divine protection that shines with a faint light.", 174.76, 7, 20.22, 9, ItemType.Armor.ToString(), null, null, 17, 35, (int)ArmorType.Chest, null, null},
            {113, "Crown of the Storm", "A crown that crackles with storm energy, offering protection against lightning and thunder.", 174.78, 6, 20.17, 9, ItemType.Armor.ToString(), null, null, 9, 43, (int)ArmorType.Head, null, null},
            {114, "Helm of the Ancients", "A helm with ancient runes, offering both physical protection and a link to long-forgotten powers.", 175.16, 7, 20.31, 9, ItemType.Armor.ToString(), null, null, 29, 23, (int)ArmorType.Head, null, null},
            {115, "Adamantine Leggings", "Leggings made from adamantine, nearly impervious to damage, offering unrivaled protection for your legs.", 174.68, 3, 20.79, 9, ItemType.Armor.ToString(), null, null, 4, 48, (int)ArmorType.Legs, null, null},
            {116, "Dragonscale Plate Greaves", "Greaves made from the scales of a dragon, offering both protection and a connection to the dragon’s power.", 174.74, 7, 20.82, 9, ItemType.Armor.ToString(), null, null, 41, 11, (int)ArmorType.Legs, null, null},
            {117, "Cracked Amethyst Shard", "A broken piece of amethyst with a jagged edge and soft glow.", 3.07, 1, 2.23, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {118, "Pewter Bracelet", "A dull grey bracelet with faint etchings of vines.", 3.67, 1, 2.13, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {119, "Tiny Amber Chip", "A fingernail-sized shard of amber that glows faintly in sunlight.", 2.12, 1, 3.153, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {120, "Copper Ring with a Glass Bead", "A simple ring with a cloudy red bead set in dented copper.", 3.88, 1, 4.01, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {121, "Carved Wooden Brooch", "A small brooch shaped like a leaf, whittled with care.", 3.16, 1, 3.02, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {122, "Polished River Stone Pendant", "A smooth stone on a leather cord, warm from being worn often.", 3.36, 1, 4.54, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {123, "Simple Bone Necklace", "Animal bones strung together—crude, but oddly charming.", 2.60, 1, 2.61, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {124, "Woven Hair Bracelet", "Intertwined strands of human and horsehair, braided tightly.", 3.87, 1, 4.29, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {125, "Obsidian Fragment", "Razor-thin volcanic glass, sharp and mirror-dark.", 3.62, 1, 3.42, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {126, "Shell and String Anklet", "Small seashells knotted onto a bit of twine—smells faintly of salt.", 3.46, 1, 4.00, 1, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {127, "Bronze Armlet with Leaf Etching", "A broad band engraved with curling vine patterns.", 13.96, 1, 2.78, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {128, "Cut Quartz Crystal", "A clear crystal with many facets, catching light like fire.", 9.08, 1, 4.38, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {129, "Jasper-inlaid Brooch", "A copper brooch with blood-red jasper set in the center.", 11.02, 1, 4.63, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {130, "Malachite Cameo", "A green and black stone carved with a woman’s profile.", 17.93, 1, 3.67, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {131, "Coral Pendant in Silver Wire", "A pink coral sprig wrapped carefully in silver filigree.", 14.33, 1, 4.18, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {132, "Silver Ring with Tiny Opal", "A tarnished silver band with a clouded opal inset.", 15.21, 1, 4.50, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {133, "Miniature Portrait Locket", "A thumb-sized locket with a faded painting of a smiling child.", 7.07, 1, 4.79, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {134, "Carved Ivory Comb", "An ornate comb made from old ivory, yellowed with age.", 16.95, 1, 3.91, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {135, "Lapis Lazuli Beads", "Deep blue beads strung tightly together, flecked with gold.", 6.79, 1, 2.15, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {136, "Agate Ring with Swirl Pattern", "The swirling lines in the agate resemble smoke in water.", 16.78, 1, 4.78, 3, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {137, "Ornate Silver Tiara", "A crescent-shaped tiara, polished to a mirror shine.", 37.48, 1, 4.92, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {138, "Gold Chain with Sapphire Chip", "A delicate chain with a bright blue stone like a droplet of sky.", 29.58, 1, 3.91, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {139, "Fire Opal Pendant", "A gem like living fire, set in a blackened iron pendant.", 39.55, 1, 4.60, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {140, "Ruby-Capped Stickpin", "A gentleman’s pin with a deep red gem atop a gold needle.", 28.86, 1, 4.41, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {141, "Enamel Pendant with Mythical Scene", "A painted pendant showing a dragon flying over a city.", 28.19, 1, 3.84, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {142, "Platinum Cufflink Set", "A pair of etched cufflinks bearing a forgotten noble crest.", 19.14, 1, 4.46, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {143, "Miniature Gold-Gilded Idol", "A palm-sized statue of a laughing deity, covered in flaking gold.", 36.12, 1, 4.62, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {144, "Jeweled Hairpin with Amethyst", "A silver hairpin tipped with a cut amethyst shaped like a flame.", 25.47, 1, 3.59, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {145, "Cloisonné Belt Buckle", "A wide buckle with tiny colored glass panels sealed in gold.", 27.82, 1, 3.38, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {146, "Serpentine Stone Ring", "This band of green stone feels oddly warm to the touch.", 24.62, 1, 4.32, 5, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {147, "Jeweled Circlet with Emerald Inlay", "A thin band of gold set with three flawless emeralds.", 59.71, 1, 3.82, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {148, "Tourmaline Choker", "A tight band of black velvet adorned with purple tourmaline.", 61.25, 1, 4.50, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {149, "Carved Obsidian Relic", "An idol with strange angles that glint sharply in dim light.", 36.99, 1, 4.80, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {150, "Decorative Ivory Horn with Gem Studs", "Etched and gilded, it no longer sounds—but still stuns.", 44.39, 1, 3.56, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {151, "Gold Bracelet with Elven Script", "Etched in curling script, it reads: “May you always return.”", 47.32, 1, 2.57, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {152, "Ornate Cloak Clasp with Garnet", "A heavy clasp shaped like a dragon’s wing, red gem glinting.", 42.25, 1, 4.16, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {153, "Enchanted Pearl Earring", "Said to whisper dreams to the wearer when worn while sleeping.", 58.92, 1, 4.71, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {154, "Amber Medallion with Insect Trapped Inside", "A winged creature forever frozen in golden sap.", 49.47, 1, 4.32, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {155, "Ancient Coin Necklace", "A string of worn coins from dead empires, pierced and looped together.", 52.86, 1, 4.88, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {156, "Chrysoberyl Fang Talisman", "A beast’s tooth polished and pierced, hung on fine silk cord.", 57.38, 1, 3.91, 7, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {157, "Embroidered Sash with Silver Thread", "The thread catches light like frost—its origin unknown.", 67.75, 1, 4.79, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {158, "Jade Leaf Amulet", "Shaped like a curling fern, this amulet smells faintly of earth.", 73.42, 1, 4.57, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {159, "Black Opal Pendant", "Dark as a storm cloud, with veins of impossible color within.", 88.91, 1, 4.28, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {160, "Twin Serpent Armband", "Two gold snakes twine around the arm, eyes inlaid with rubies.", 92.86, 1, 4.36, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {161, "Enchanted Cloak Chain with Moonstone Clasps", "Glows slightly under moonlight—once belonged to a seer.", 85.61, 1, 4.65, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {162, "Ancient Tiara of the Wastes", "Forged of gold and bone—rumored to ward off desert spirits.", 91.35, 1, 4.74, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {163, "Runed Gold Medallion", "The rune changes when no one looks at it.", 97.25, 1, 4.93, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {164, "Gilded Chain of the Last Empire", "Forged for royalty, its links whisper names of the dead.", 86.43, 1, 3.77, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {165, "Opalized Fossil Pendant", "The stone glitters, showing the fossil of a fern curled inside.", 81.27, 1, 4.39, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {166, "Cracked Diadem of Forgotten Queens", "Mended, but humming faintly with some old, bitter song.", 77.94, 1, 4.12, 9, ItemType.Valuable.ToString(), null, null, null, null, null, null, null},
            {167, "Potion of Healing", "A small vial of red liquid", 15, 1, 1, 1, ItemType.Consumable.ToString(), null, null, null, null, null, 5, (int)ConsumableType.Health},
            {168, "Potion of Greater Healing", "A medium vial of red liquid", 40, 1, 2, 3, ItemType.Consumable.ToString(), null, null, null, null, null, 15, (int)ConsumableType.Health},
            {169, "Potion of Master Healing", "A large vial of red liquid", 85, 1, 3, 5, ItemType.Consumable.ToString(), null, null, null, null, null, 30, (int)ConsumableType.Health},
            {170, "Potion of Supreme Healing", "A small vial of red liquid with a gold topper.", 145, 1, 4, 7, ItemType.Consumable.ToString(), null, null, null, null, null, 50, (int)ConsumableType.Health},
            {171, "Potion of Durability", "A small vial of blue liquid", 15, 1, 1, 1, ItemType.Consumable.ToString(), null, null, null, null, null, 3, (int)ConsumableType.Durability},
            {172, "Potion of Greater Durability", "A medium vial of blue liquid", 35, 1, 2, 3, ItemType.Consumable.ToString(), null, null, null, null, null, 6, (int)ConsumableType.Durability},
            {173, "Potion of Master Durability", "A large vial of blue liquid", 55, 1, 3, 5, ItemType.Consumable.ToString(), null, null, null, null, null, 9, (int)ConsumableType.Durability},
            {174, "Potion of Supreme Durability", "A small vial of blue liquid with a gold topper", 85, 1, 4, 7, ItemType.Consumable.ToString(), null, null, null, null, null, 12, (int)ConsumableType.Durability},
            {175, "Potion of Restoration", "A small vial of green liquid", 15, 1, 1, 1, ItemType.Consumable.ToString(), null, null, null, null, null, 3, (int)ConsumableType.Resource},
            {176, "Potion of Greater Restoration", "A medium vial of green liquid", 35, 1, 2, 3, ItemType.Consumable.ToString(), null, null, null, null, null, 6, (int)ConsumableType.Resource},
            {177, "Potion of Master Restoration", "A large vial of green liquid", 55, 1, 3, 5, ItemType.Consumable.ToString(), null, null, null, null, null, 9, (int)ConsumableType.Resource},
            {178, "Potion of Supreme Restoration", "A small vial of green liquid with a gold topper", 85, 1, 4, 7, ItemType.Consumable.ToString(), null, null, null, null, null, 12, (int)ConsumableType.Resource}
        });
        migrationBuilder.Sql("SET IDENTITY_INSERT Items OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Skills ON;");
        migrationBuilder.InsertData(
        table: "Skills",
        columns: new[]
        {
           "Id", "ArchetypeId", "MonsterId", "Name", "Description", "RequiredLevel", "Cost", "Power", "Cooldown", "TargetType", "SkillType", "SkillCategory", "DamageType", "Duration", "StatAffected", "SupportEffect"
        },
        values: new object[,]
        {
            { 1, 1, null, "Power Slash", "A heavy swing aimed at breaking defenses.", 1, 3, 7, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 3, 2, null, "Gut Punch", "A brutal strike to wind the enemy.", 1, 3, 6, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 5, 3, null, "Quick Stab", "A lightning-fast strike aimed at weak points.", 1, 3, 8, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 7, 4, null, "Ember Spark", "A focused bolt of flame.", 1, 3, 6, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 9, 5, null, "Radiant Bolt", "A divine light strikes the foe.", 1, 3, 9, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 2, 1, null, "Staggering Blow", "A powerful hit that leaves enemies reeling.", 1, 3, 9, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 4, 2, null, "Rampage Swing", "Wild, reckless overhead strike.", 1, 3, 11, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 6, 3, null, "Smoke Blade", "Strike and vanish in the same breath.", 1, 3, 10, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 8, 4, null, "Ice Lance", "A chilling projectile of ice.", 1, 3, 11, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 10, 5, null, "Thunder Hymn", "A burst of holy thunder.", 1, 3, 13, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 11, 1, null, "Whirlwind Strike", "A spinning attack that clears a path through enemies.", 3, 10, 33, 8, (int)TargetType.AllEnemies, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 12, 1, null, "Battle Cry", "A fearsome roar that inspires and boosts your Attack.", 2, 3, 3, 4, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 13, 2, null, "Earthshatter Slam", "Uses sheer force to disrupt the battlefield.", 3, 10, 33, 8, (int)TargetType.AllEnemies, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 14, 2, null, "Intimidating Roar", "A loud cry that makes enemies hesitate.", 2, 3, 3, 4, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 15, 3, null, "Shadow Dance", "A blur of movement and blades.", 3, 10, 33, 8, (int)TargetType.AllEnemies, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 16, 3, null, "Smoke Bomb", "A cloud of smoke obscures enemy vision.", 2, 3, 3, 4, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 17, 4, null, "Elemental Surge", "The elements leap to your call", 3, 10, 33, 8, (int)TargetType.AllEnemies, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 18, 4, null, "Mana Infusion", "The mana in the air is absorbed into your body.", 2, 3, 3, 4, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Magic, (int)SupportEffectType.Boost },
            { 19, 5, null, "Divine Judgment", "Summon the Wrath of the heavens.", 3, 10, 33, 8, (int)TargetType.AllEnemies, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 20, 5, null, "Blessing of Light", "You are blessed by the heavens.", 2, 3, 3, 4, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Health, (int)SupportEffectType.Boost },
            { 21, 1, null, "Slashing Wind", "The sword cuts through the wind and strikes all enemies.", 2, 5, 24, 3, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 22, 1, null, "Cleaving Strike", "Wind up and swing. You'll hit somebody.", 3, 6, 32, 5, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 23, 1, null, "Defender's Stance", "Plant your feet like a tree.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 24, 2, null, "Ground Slam", "Stomp your foot and shake the ground", 3, 5, 26, 4, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 25, 2, null, "Rampaging Charge", "Wind up, close your eyes, and charge.", 3, 6, 29, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 26, 2, null, "Unstoppable Fury", "Feel emotions. Channel emotions.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 27, 3, null, "Backstab", "Slip past your enemy's defenses and stab!", 3, 5, 27, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 28, 3, null, "Shadowstrike", "Strike with the power of the shadows.", 3, 6, 31, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 29, 3, null, "Evasion", "Move your feet faster, don't get hit!", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 30, 4, null, "Flame Wave", "Launch a wave of flame at your enemies", 3, 6, 28, 4, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 31, 4, null, "Arcane Missile", "Shoot forth a misslile of magic power.", 3, 7, 34, 5, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 32, 4, null, "Arcane Shield", "Magic hardens over you like a second skin.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Resistance, (int)SupportEffectType.Boost },
            { 33, 5, null, "Divine Smite", "Call down your god to vanquish a foe", 3, 6, 30, 4, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 34, 5, null, "Holy Light", "A bright light bursts forth from your hands", 3, 5, 25, 4, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 35, 5, null, "Sacred Ward", "Divine light emanates from your skin.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 36, 1, null, "Iron Tempest", "Your blades move fast like a tempest.", 4, 8, 40, 6, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 37, 1, null, "Blade Rush", "Dash through enemies, slicing each in your path", 5, 10, 50, 7, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 38, 1, null, "Punishing Blow", "A crushing attack", 5, 9, 45, 6, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 39, 2, null, "Savage Uppercut", "A brutal upward punch", 5, 9, 45, 6, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 40, 2, null, "Brutal Lunge", "Charge at a foe with reckless abandon.", 5, 10, 49, 7, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 41, 2, null, "Bonecrusher", "A heavy strike aimed at breaking bones and armor alike.", 5, 11, 54, 8, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 42, 3, null, "Fan of Knives", "Throw of flurry of knives in all directions. Where did they even come from?", 4, 8, 40, 6, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 43, 3, null, "Bleeding Strike", "Target a spot that's going to hurt. A lot.", 5, 9, 45, 6, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 44, 3, null, "Ambush", "Leap with surprising agility to surprise your foes.", 6, 11, 55, 8, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 45, 4, null, "Chain Lightning", "Lightning arcs through all enemies", 5, 10, 50, 7, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 46, 4, null, "Frostbite", "Ice creeps over an enemy", 4, 8, 40, 6, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 47, 4, null, "Arcane Detonation", "Create an arcane explosion", 5, 9, 45, 6, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 48, 5, null, "Judgment Flame", "A holy fire that burns all enemies", 5, 9, 45, 6, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 49, 5, null, "Heaven's Hammer", "A divine hammer to crush a foe", 6, 11, 55, 8, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 50, 5, null, "Blinding Radiance", "Divine light blinds the room. Wear sunglasses next time.", 5, 10, 50, 7, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 51, 1, null, "Titan Cleave", "Deliver a devastating wide swing that shatters defenses.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 52, 1, null, "Executioner’s Strike", "A brutal blow aimed to finish weakened foes.", 8, 15, 75, 11, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 53, 1, null, "Warrior’s Cry", "Let out a mighty roar, boosting your Attack.", 4, 7, 7, 9, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 54, 1, null, "Threatening Stance", "Reduce enemy Attack with an intimidating posture.", 5, 9, 9, 12, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 55, 2, null, "Bonequake Slam", "Crush the ground and crack bones with raw force.", 8, 15, 75, 11, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 56, 2, null, "Fury Breaker", "Focus all your strength into a single armor-breaking punch.", 7, 13, 65, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 57, 2, null, "Iron Skin", "Harden your body, greatly boosting your Defense.", 4, 7, 7, 9, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 58, 2, null, "Staggering Roar", "A guttural yell that weakens enemy Speed.", 5, 9, 9, 12, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 59, 3, null, "Death Blossom", "Spin through enemies, blades flying like petals.", 6, 12, 60, 9, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 60, 3, null, "Throatpiercer", "A ruthless critical strike aimed at vital spots.", 7, 14, 70, 10, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 61, 3, null, "Adrenaline Surge", "Focus your energy to increase Speed sharply.", 4, 7, 7, 9, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 62, 3, null, "Crippling Poison", "Coat your blades in poison that lowers enemy’s Speed.", 5, 9, 9, 12, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 63, 4, null, "Cataclysm Ray", "Unleash a focused magical beam of destruction.", 7, 14, 70, 10, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 64, 4, null, "Voidstorm", "Summon a chaotic storm of arcane energy.", 7, 13, 65, 9, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 65, 4, null, "Arcane Shell", "Wrap yourself in magical wards to boost Resistance.", 4, 7, 7, 9, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Resistance, (int)SupportEffectType.Boost },
            { 66, 4, null, "Mind Burn", "Temporarily sap a foe’s Magic stat with mental feedback.", 5, 9, 9, 12, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Magic, (int)SupportEffectType.Reduce },
            { 67, 5, null, "Divine Lance", "Launch a radiant spear that sears with holy fire.", 7, 13, 65, 9, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 68, 5, null, "Radiant Pulse", "Send a blinding wave of light through the battlefield.", 6, 12, 60, 9, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 69, 5, null, "Holy Guard", "Call divine protection to raise Resistance.", 4, 7, 7, 9, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 70, 5, null, "Weaken Spirit", "Reduce the enemy’s Magic with divine suppression.", 5, 9, 9, 12, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Magic, (int)SupportEffectType.Reduce },
            { 71, 1, null, "Crushing Arc", "A heavy swing aimed to break multiple enemies at once.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 72, 1, null, "Rallying Cry", "Inspire yourself with battle fury, boosting your Attack.", 6, 11, 11, 14, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 73, 1, null, "Stone Focus", "Center your stance, increasing your Defense significantly.", 7, 13, 13, 17, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 74, 2, null, "Juggernaut Slam", "A powerful, ground-shaking charge that flattens enemies.", 8, 15, 75, 11, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 75, 2, null, "Brutal Intimidation", "Frighten enemies into weakness, reducing their Defense.", 6, 11, 11, 14, (int)TargetType.AllEnemies, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Reduce },
            { 76, 2, null, "Grounded Strength", "Focus inward to boost both Attack slightly.", 7, 13, 13, 17, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 77, 3, null, "Blackout Strike", "A quick blow to a pressure point, aimed to disorient.", 7, 13, 65, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 78, 3, null, "Vanish", "Slip into the shadows, increasing Speed sharply.", 6, 11, 11, 14, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 79, 3, null, "Saboteur's Trick", "Apply dirty fighting tricks to reduce enemy Attack.", 7, 13, 13, 17, (int)TargetType.AllEnemies, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 80, 4, null, "Comet Shard", "Drop a magical meteor chunk onto a foe with crushing force.", 7, 14, 70, 10, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 81, 4, null, "Temporal Haste", "Manipulate time to boost your Speed dramatically.", 6, 11, 11, 14, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 82, 4, null, "Spell Leak", "Disrupt enemy flow with magic, lowering their Attack stat.", 7, 13, 13, 17, (int)TargetType.AllEnemies, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 83, 5, null, "Purifying Flame", "Send out cleansing fire that damages the impure.", 7, 13, 65, 9, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 84, 5, null, "Divine Endurance", "Call upon holy strength to raise your Defense.", 6, 11, 11, 14, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 85, 5, null, "Lightshard Weakening", "Reduce enemy Resistance with light-fused prayers.", 7, 13, 13, 17, (int)TargetType.AllEnemies, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Resistance, (int)SupportEffectType.Reduce },
            { 86, 1, null, "Thunder Cleave", "A massive sweeping strike infused with thunderous force.", 8, 15, 75, 11, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 87, 1, null, "Precision Slam", "A perfectly timed hit that bypasses some enemy defense.", 7, 14, 70, 10, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 88, 1, null, "Unyielding Will", "Strengthen your resolve, raising Resistance.", 9, 17, 17, 22, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Resistance, (int)SupportEffectType.Boost },
            { 89, 2, null, "Skullbreaker", "A bone-crunching blow aimed at disabling the enemy.", 8, 15, 75, 11, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 90, 2, null, "Spiked Collision", "Slam enemies with brutal force, causing splash damage.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 91, 2, null, "War Frenzy", "Go into a frenzy, increasing your Attack greatly.", 9, 17, 17, 22, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 92, 3, null, "Blade Fan", "A flurry of blades hits multiple targets.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 93, 3, null, "Eviscerate", "A swift strike to vital organs for major damage.", 8, 15, 75, 11, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 94, 3, null, "Crippling Dust", "A thrown powder blinds and slows your enemy.", 9, 17, 17, 22, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 95, 4, null, "Arc Lightning", "Chain lightning arcs between enemies.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 96, 4, null, "Void Pulse", "A pulse of chaotic magic that damages and confuses.", 8, 15, 75, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 97, 4, null, "Arcane Barrier", "Erect a shield that raises Resistance.", 9, 17, 17, 22, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Resistance, (int)SupportEffectType.Boost },
            { 98, 5, null, "Sacred Eruption", "Burst of divine light harms all foes.", 7, 14, 70, 10, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 99, 5, null, "Judgement Spear", "Hurl a holy spear that pierces defenses.", 8, 15, 75, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 100, 5, null, "Blessing of Speed", "Call down divine wind to boost Speed.", 9, 17, 17, 22, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 101, 1, null, "Meteor Crash", "Leap and strike the ground with explosive force.", 9, 17, 85, 12, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 102, 2, null, "Earthshatter", "A ground-shaking slam that damages and dazes.", 9, 17, 85, 12, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 103, 3, null, "Death Lotus", "A deadly spinning flurry of blades.", 9, 17, 85, 12, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 104, 4, null, "Rift Spear", "Summon a spear from another plane to skewer your foe.", 9, 17, 85, 12, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 105, 5, null, "Beacon Burst", "Radiate divine energy that burns evil.", 9, 17, 85, 12, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 106, 1, null, "Dragon’s End", "Channel all strength into a devastating final blow.", 10, 20, 100, 14, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 107, 2, null, "Colossus Crush", "Slam with titanic force, obliterating armor and flesh.", 10, 20, 100, 14, (int)TargetType.AllEnemies, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 108, 3, null, "Night Reaper", "Unleash a precise, lethal combo on a single target.", 10, 20, 100, 14, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 109, 4, null, "Starfall", "Bring down stars to scorch all enemies in radiant fire.", 10, 20, 100, 14, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 110, 5, null, "Divine Wrath", "Summon the full might of your god to smite the wicked.", 10, 20, 100, 14, (int)TargetType.AllEnemies, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 111, null, null, "Swift Strike", "A quick strike that catches the enemy off guard.", 1, 1, 8, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 112, null, null, "Flame Dart", "A small burst of fire aimed at the enemy.", 1, 1, 9, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 113, null, null, "Shield Bash", "A defensive bash with your shield to stagger the enemy.", 1, 1, 10, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 114, null, null, "Frostbite", "A blast of cold energy to freeze the enemy in place.", 1, 1, 11, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 115, null, null, "Wind Slash", "A slash of wind that cuts through enemies.", 1, 1, 12, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 116, null, null, "Arcane Bolt", "A bolt of pure arcane energy shot from your hand.", 1, 1, 13, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 117, null, null, "Power Strike", "A heavy strike that deals massive damage.", 1, 1, 13, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 118, null, null, "Elemental Blast", "A surge of elemental energy strikes the target.", 1, 1, 12, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 119, null, null, "Slash", "A quick, basic attack dealing moderate damage.", 1, 1, 10, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 120, null, null, "Fireball", "A small fireball that explodes on impact.", 1, 1, 11, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 121, null, null, "Brutal Uppercut", "A savage uppercut that knocks foes off their footing.", 3, 1, 25, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 122, null, null, "Shadow Lance", "A piercing spear of dark energy hurled with malice.", 3, 1, 27, 4, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 123, null, null, "Bone Crusher", "A bone-shattering blow aimed at limbs.", 3, 1, 28, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 124, null, null, "Inferno Shot", "A searing blaze of fire races toward the target.", 3, 1, 30, 4, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 125, null, null, "Iron Claw", "A crushing claw attack that rends armor.", 3, 1, 26, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 126, null, null, "Ice Spike", "A jagged spike of ice erupts under the foe.", 2, 1, 24, 3, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 127, null, null, "Skull Bash", "A brutal headbutt aimed at disorientation.", 2, 1, 23, 3, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 128, null, null, "Thunder Coil", "Electricity coils around the enemy and discharges violently.", 3, 1, 29, 4, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 129, null, null, "Savage Rend", "Tears into the target with wild abandon.", 3, 1, 27, 4, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 130, null, null, "Void Pulse", "A blast of void energy disrupts the target's body and mind.", 3, 1, 28, 4, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 131, null, null, "Titan's Fist", "A mighty punch that crushes through enemies with overwhelming force.", 6, 1, 55, 8, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 132, null, null, "Arcane Fury", "Unleashes a flurry of destructive arcane energy in all directions.", 6, 1, 58, 8, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 133, null, null, "Dragon's Roar", "A deafening roar that sends shockwaves through enemies.", 6, 1, 62, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 134, null, null, "Lightning Surge", "A surge of lightning that strikes multiple enemies at once.", 5, 1, 54, 8, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 135, null, null, "Earthquake Stomp", "A powerful stomp that causes the ground to tremble and shake.", 6, 1, 60, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 136, null, null, "Frost Nova", "A burst of freezing energy radiates outward, slowing and damaging enemies.", 6, 1, 56, 8, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 137, null, null, "Hellfire Blast", "Summons a massive explosion of fire and brimstone.", 6, 1, 64, 9, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 138, null, null, "Stone Crush", "Crushes the enemy with a giant boulder summoned from the earth.", 6, 1, 63, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 139, null, null, "Tornado Strike", "Creates a small tornado that sweeps through enemies, causing massive damage.", 6, 1, 61, 9, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 140, null, null, "Solar Flare", "A concentrated burst of solar energy blinds and burns enemies.", 6, 1, 59, 8, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 141, null, null, "Phoenix's Wrath", "Unleashes the fiery fury of the Phoenix, burning all enemies in range.", 9, 1, 85, 12, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 142, null, null, "Titan's Fury", "Summons the might of the Titans to deal colossal damage to enemies.", 9, 1, 90, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 143, null, null, "Meteor Storm", "Summons a storm of meteors that rains down fiery destruction on enemies.", 10, 1, 95, 14, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 144, null, null, "Blizzard Gale", "A powerful storm of ice and wind that freezes and damages enemies in its path.", 9, 1, 87, 12, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 145, null, null, "Vortex Slash", "A swirling slash of wind and energy that cuts through all foes in its path.", 9, 1, 92, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 146, null, null, "Divine Smite", "A mighty strike from the heavens that delivers devastating holy damage.", 8, 1, 80, 11, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 147, null, null, "Void Blast", "A burst of dark energy that tears apart the fabric of space, damaging enemies.", 9, 1, 93, 13, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 148, null, null, "Thunderclap", "A massive thunderous shockwave that stuns and damages enemies.", 9, 1, 88, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 149, null, null, "Arcane Barrage", "Fires a barrage of magical bolts, each dealing substantial damage.", 9, 1, 94, 13, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 150, null, null, "Dragon's Might", "The power of a dragon surges through the user, causing massive damage to enemies.", 10, 1, 99, 14, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 151, null, null, "Eclipse Beam", "Fires a concentrated beam of pure darkness that obliterates enemies in its path.", 13, 1, 130, 19, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 152, null, null, "Cataclysm", "A devastating attack that causes the earth to crack open and crush enemies.", 15, 1, 145, 21, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 153, null, null, "Heaven's Wrath", "A divine strike from the heavens that deals massive holy damage to all foes.", 14, 1, 140, 20, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 154, null, null, "Arcane Fury", "Unleashes a powerful burst of raw magical energy,  causing massive area damage.", 14, 1, 135, 19, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 155, null, null, "Endless Storm", "Summons an unstoppable storm that causes continuous damage to all nearby enemies.", 12, 1, 120, 17, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 156, null, null, "Savage Pounce", "Leap at your target with primal fury.", 4, 10, 37, 9, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 157, null, null, "Roar of Ruin", "Unleash a deafening roar that terrifies your enemies.", 4, 10, 43, 11, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 158, null, null, "Hexstorm", "Summon a storm of curses that damages all enemies.", 4, 10, 39, 10, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 159, null, null, "Witchfire", "Launch a barrage of purple and black flames.", 4, 10, 42, 11, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 160, null, null, "Blood Moon Ritual", "The room turns red as blood as it drains the vitality of your enemies.", 4, 10, 39, 10, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 161, null, null, "Featherstorm", "Unleash a flurry of razor-sharp feathers in all directions.", 5, 10, 49, 12, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 162, null, null, "Gale Evasion", "Vanish into a gust of wind and freely attack.", 4, 10, 36, 9, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 163, null, null, "Sky Cleave", "Dive from above with a devastating strike.", 4, 10, 40, 10, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 164, null, null, "Ancient Instinct", "Enter a feral trance, unleash your full potential.", 5, 10, 47, 12, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 165, null, null, "Dark Pact", "Call upon the greater demon you serve and unleash hell.", 5, 10, 54, 14, (int)TargetType.SingleEnemy, SkillType.UltimateSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 166, null, null, "Adrenal Boost", "Momentarily increases your attack.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 167, null, null, "Ironhide", "Temporarily hardens skin to increase defense.", 3, 6, 6, 8, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 168, null, null, "Focused Mind", "Raises magic power through deep concentration.", 2, 4, 4, 5, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Magic, (int)SupportEffectType.Boost },
            { 169, null, null, "Shield Break", "Reduces the target's physical defense.", 7, 13, 13, 17, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Reduce },
            { 170, null, null, "Arcane Drain", "Weakens the target’s magical resistance.", 8, 16, 16, 21, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Resistance, (int)SupportEffectType.Reduce },
            { 171, null, null, "Vigor Surge", "Surge of health energizes the body.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Health, (int)SupportEffectType.Boost },
            { 172, null, null, "Sharpen Claws", "Attack power increases with honed edges.", 4, 8, 8, 10, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 173, null, null, "Mental Shackles", "Suppresses enemy magic.", 2, 3, 3, 4, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Magic, (int)SupportEffectType.Reduce },
            { 174, null, null, "Stone Skin", "Physical defense greatly improves.", 3, 6, 6, 8, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 175, null, null, "Mind Fog", "Reduces target’s magic resistance.", 2, 3, 3, 4, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 2, (int)StatType.Resistance, (int)SupportEffectType.Reduce },
            { 176, null, null, "Overcharge", "Greatly boosts magic temporarily.", 7, 13, 13, 17, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Magic, (int)SupportEffectType.Boost },
            { 177, null, null, "Rage Howl", "Raises attack power and intimidates foes.", 6, 12, 12, 16, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 178, null, null, "Armor Melt", "Reduces enemy defense significantly.", 4, 7, 7, 9, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Defense, (int)SupportEffectType.Reduce },
            { 179, null, null, "Phase Step", "Boosts speed with temporal magic.", 3, 5, 5, 7, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Speed, (int)SupportEffectType.Boost },
            { 180, null, null, "Chill Veil", "Slows enemy movements.", 4, 7, 7, 9, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 4, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 181, null, null, "Lifebloom", "Health regenerates for a short time.", 5, 9, 9, 12, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Health, (int)SupportEffectType.Boost },
            { 182, null, null, "Desolation Mark", "Reduces enemy attack slightly.", 3, 6, 6, 8, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 3, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 183, null, null, "Titan Skin", "Massively boosts defense.", 6, 11, 11, 14, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 184, null, null, "Stasis Bind", "Greatly slows the enemy.", 6, 11, 11, 14, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 185, null, null, "Spellflux", "Reduces enemy magic output significantly.", 7, 13, 13, 17, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Magic, (int)SupportEffectType.Reduce },
            { 186, null, 26, "Thousand Masks", "Unleash a storm of illusions, each striking with deadly precision.", 10, 10, 80, 12, (int)TargetType.SingleEnemy, SkillType.BossSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 187, null, 27, "Soul Cataclysm", "Rend the battlefield asunder with necrotic force and cursed souls.", 10, 10, 85, 12, (int)TargetType.SingleEnemy, SkillType.BossSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 188, null, 28, "Crimson Requiem", "Drain the blood and vitality of all enemies, healing yourself with their life force.", 10, 10, 75, 12, (int)TargetType.SingleEnemy, SkillType.BossSkill.ToString(), (int)SkillCategory.Ultimate, (int)DamageType.Hybrid, null, null, null },
            { 189, null, 26, "Void Fang", "Sink fangs into the target, draining both health and light.", 9, 1, 94, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 190, null, 26, "Foxfire Burst", "Launch an explosive orb of ghostly blue fire.", 7, 1, 69, 10, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 191, null, 26, "Shadow Mirage", "Strike from multiple illusory angles at once.", 9, 1, 92, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 192, null, 26, "Night Curse", "Infuse the enemy with cursed magic that saps strength.", 9, 1, 92, 13, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 193, null, 26, "Howl of Dread", "Let out a piercing cry that disrupts the soul.", 7, 1, 66, 9, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 194, null, 27, "Necrotic Blast", "Unleash a blast of dark energy that erodes the target's life force.", 8, 1, 78, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 195, null, 27, "Soul Leech", "Drain the very soul of the target, healing the lich for a portion of the damage dealt.", 7, 1, 71, 10, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 196, null, 27, "Withering Touch", "Strike with a touch that decays the target's flesh and spirit.", 9, 1, 92, 13, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 197, null, 27, "Death's Embrace", "Summon an ethereal hand to grasp the target, dealing dark damage over time.", 8, 1, 75, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 198, null, 27, "Dark Convergence", "Gather the energies of the void to collapse on the enemy, dealing massive damage.", 9, 1, 86, 12, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 199, null, 28, "Blood Feast", "Drain the blood of your target, healing yourself for a portion of the damage dealt.", 9, 1, 94, 13, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 200, null, 28, "Crimson Fury", "Unleash a flurry of rapid strikes that tear into your opponent's flesh.", 7, 1, 69, 10, (int)TargetType.SingleEnemy, SkillType.MartialSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Martial, null, null, null },
            { 201, null, 28, "Vampiric Eclipse", "Call upon the darkness to shield you and deal damage to nearby enemies.", 8, 1, 82, 12, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 202, null, 28, "Sanguine Vortex", "Summon a vortex of blood and dark magic, pulling enemies toward you and dealing damage.", 8, 1, 80, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 203, null, 28, "Night's Kiss", "Unleash a powerful burst of shadow magic that drains the life force of your target.", 8, 1, 80, 11, (int)TargetType.SingleEnemy, SkillType.MagicSkill.ToString(), (int)SkillCategory.Basic, (int)DamageType.Magical, null, null, null },
            { 204, null, 26, "Eternal Trickery", "Reduce the target's defense, leaving them vulnerable to attacks.", 10, 1, 20, 26, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Reduce },
            { 205, null, 26, "Illusionary Mist", "Create an illusionary mist that confuses enemies, reducing their accuracy.", 10, 1, 20, 26, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Reduce },
            { 206, null, 26, "Vengeful Spirit", "Increase your attack power as you harness the energy of vengeful spirits.", 8, 1, 16, 21, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 207, null, 27, "Necrotic Surge", "Buff your resistance to magical attacks, making you more durable against magic.", 7, 1, 14, 18, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Resistance, (int)SupportEffectType.Boost },
            { 208, null, 27, "Curse of Decay", "Inflict a curse on an enemy, reducing their speed over time.", 10, 1, 19, 25, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Speed, (int)SupportEffectType.Reduce },
            { 209, null, 27, "Soul Drain", "Drain the life force from your enemies, healing yourself while reducing their health.", 9, 1, 18, 23, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Health, (int)SupportEffectType.Reduce },
            { 210, null, 28, "Blood Shield", "Create a shield that absorbs damage, granting you extra defense.", 7, 1, 14, 18, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Defense, (int)SupportEffectType.Boost },
            { 211, null, 28, "Dark Blessing", "Enhance your attack power with the blessing of darkness, increasing damage dealt.", 8, 1, 16, 21, (int)TargetType.Self, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Attack, (int)SupportEffectType.Boost },
            { 212, null, 28, "Siphon Vitality", "Siphon the vitality from a nearby enemy, reducing their strength while restoring your own health.", 7, 1, 14, 18, (int)TargetType.SingleEnemy, SkillType.SupportSkill.ToString(), (int)SkillCategory.Support, null, 5, (int)StatType.Health, (int)SupportEffectType.Reduce }
        });
        migrationBuilder.Sql("SET IDENTITY_INSERT Skills OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT Players ON;");
        migrationBuilder.InsertData(
        table: "Players",
        columns: new[]
        {
            "Id", "Name", "ArchetypeId", "Level", "MaxHealth"
        },
        values: new object[,]
        {
            { 1, "Aravien Sunblade", 1, 3, 85 },
            { 2, "Nadira Valebright", 2, 5, 130 },
            { 3, "Kaenji Stormleaf", 3, 7, 95 },
            { 4, "Lusia Moonwhisper", 4, 1, 60 },
            { 5, "Thabros Ironstride", 5, 6, 110 },
            { 6, "Dymitros Frostvein", 1, 8, 140 },
            { 7, "Meilin Starbloom", 2, 2, 70 },
            { 8, "Karlos Flameborn", 3, 9, 125 },
            { 9, "A'minah Windgrace", 4, 4, 80 },
            { 10, "Bjornar Stoneheart", 5, 10, 150 },
            { 11, "Soraiya Duskrender", 1,  4, 90 },
            { 12, "Jalen of the Hollow Sky", 2, 6, 115 },
            { 13, "Yutono Spiritblade", 3, 1, 55 },
            { 14, "Anikaa Dawnsinger", 4, 7, 100 },
            { 15, "Kwamar Tideborn", 5, 6, 135 },
            { 16, "Rykos Emberquill", 1, 5, 105 },
            { 17, "Helenya Greenseer", 2, 3, 78 },
            { 18, "Omari Voidwalker", 3, 9, 145 },
            { 19, "Zaniel Silvertide", 4, 5, 92 },
            { 20, "Jovarien Flamevein", 5, 7, 125 }
        });
        migrationBuilder.Sql("SET IDENTITY_INSERT Players OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Inventories ON;");
        migrationBuilder.InsertData(
        table: "Inventories",
        columns: new[]
        {
            "Id", "PlayerId", "Gold", "Capacity"
        },
        values: new object[,]
        {
            { 1, 1, 120, 30.0m },
            { 2, 2, 350, 50.0m },
            { 3, 3, 780, 40.0m },
            { 4, 4, 25, 20.0m },
            { 5, 5, 540, 45.0m },
            { 6, 6, 980, 55.0m },
            { 7, 7, 60, 25.0m },
            { 8, 8, 1020, 50.0m },
            { 9, 9, 200, 35.0m },
            { 10, 10, 1500, 60.0m },
            { 11, 11, 280, 32.0m },
            { 12, 12, 600, 42.0m },
            { 13, 13, 40, 20.0m },
            { 14, 14, 820, 45.0m },
            { 15, 15, 610, 50.0m },
            { 16, 16, 470, 40.0m },
            { 17, 17, 180, 28.0m },
            { 18, 18, 990, 55.0m },
            { 19, 19, 360, 38.0m },
            { 20, 20, 700, 50.0m }
        });
        migrationBuilder.Sql("SET IDENTITY_INSERT Inventories OFF;");

        migrationBuilder.Sql("SET IDENTITY_INSERT Runes ON");
        migrationBuilder.InsertData(
            table: "Runes",
            columns: new[] { "Id", "Name", "RuneType", "Element", "Rarity", "Tier", "Power", "Duration" },
            values: new object[,]
            {
                { 1, "Ember Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Common, 1, 2, 3 },
                { 2, "Fire Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 3, "Blaze Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 1, 4, 3 },
                { 4, "Lava Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 1, 5, 3 },
                { 5, "Inferno Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 6, "Phoenix Rune", "WeaponRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 7, "Ember Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Common, 1, 2, null },
                { 8, "Fire Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Uncommon, 1, 3, null },
                { 9, "Blaze Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Rare, 1, 4, null },
                { 10, "Lava Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Epic, 1, 5, null },
                { 11, "Inferno Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Legendary, 1, 7, null },
                { 12, "Phoenix Rune", "ArmorRune", (int)ElementType.Fire, (int)RarityLevel.Mythic, 1, 10, null },

                { 13, "Frost Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Common, 1, 2, 3 },
                { 14, "Ice Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 15, "Hail Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 1, 4, 3 },
                { 16, "Glacier Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 1, 5, 3 },
                { 17, "Blizzard Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 18, "Ymir Rune", "WeaponRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 19, "Frost Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Common, 1, 2, null },
                { 20, "Ice Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Uncommon, 1, 3, null },
                { 21, "Hail Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Rare, 1, 4, null },
                { 22, "Glacier Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Epic, 1, 5, null },
                { 23, "Blizzard Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Legendary, 1, 7, null },
                { 24, "Ymir Rune", "ArmorRune", (int)ElementType.Ice, (int)RarityLevel.Mythic, 1, 10, null },

                { 25, "Spark Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 1, 2, 3 },
                { 26, "Jolt Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 27, "Lightning Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 1, 4, 3 },
                { 28, "Storm Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 1, 5, 3 },
                { 29, "Tempest Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 30, "Raijin Rune", "WeaponRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 31, "Spark Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Common, 1, 2, null },
                { 32, "Jolt Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Uncommon, 1, 3, null },
                { 33, "Lightning Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Rare, 1, 4, null },
                { 34, "Storm Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Epic, 1, 5, null },
                { 35, "Tempest Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Legendary, 1, 7, null },
                { 36, "Raijin Rune", "ArmorRune", (int)ElementType.Lightning, (int)RarityLevel.Mythic, 1, 10, null },

                { 37, "Seed Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Common, 1, 2, 3 },
                { 38, "Nature Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 39, "Thorn Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 1, 4, 3 },
                { 40, "Beast Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 1, 5, 3 },
                { 41, "Quake Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 42, "Gaia Rune", "WeaponRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 43, "Seed Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Common, 1, 2, null },
                { 44, "Nature Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Uncommon, 1, 3, null },
                { 45, "Thorn Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Rare, 1, 4, null },
                { 46, "Beast Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Epic, 1, 5, null },
                { 47, "Quake Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Legendary, 1, 7, null },
                { 48, "Gaia Rune", "ArmorRune", (int)ElementType.Nature, (int)RarityLevel.Mythic, 1, 10, null },

                { 49, "Bright Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 1, 2, 3 },
                { 50, "Energy Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 51, "Radiant Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 1, 4, 3 },
                { 52, "Solar Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 1, 5, 3 },
                { 53, "Divine Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 54, "Seraph Rune", "WeaponRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 55, "Bright Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Common, 1, 2, null },
                { 56, "Energy Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Uncommon, 1, 3, null },
                { 57, "Radiant Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Rare, 1, 4, null },
                { 58, "Solar Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Epic, 1, 5, null },
                { 59, "Divine Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Legendary, 1, 7, null },
                { 60, "Seraph Rune", "ArmorRune", (int)ElementType.Radiance, (int)RarityLevel.Mythic, 1, 10, null },

                { 61, "Shadow Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 1, 2, 3 },
                { 62, "Gloom Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 1, 3, 3 },
                { 63, "Abyss Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 1, 4, 3 },
                { 64, "Void Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 1, 5, 3 },
                { 65, "Nether Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 1, 7, 3 },
                { 66, "Nyx Rune", "WeaponRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 1, 10, 3 },

                { 67, "Shadow Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Common, 1, 2, null },
                { 68, "Gloom Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Uncommon, 1, 3, null },
                { 69, "Abyss Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Rare, 1, 4, null },
                { 70, "Void Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Epic, 1, 5, null },
                { 71, "Nether Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Legendary, 1, 7, null },
                { 72, "Nyx Rune", "ArmorRune", (int)ElementType.Abyssal, (int)RarityLevel.Mythic, 1, 10, null }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Runes OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients ON");
        migrationBuilder.InsertData(
            table: "Ingredients",
            columns: new[] { "Id", "Name", "ComponentType" },
            values: new object[,]
            {
                { 1, "Fading Essence", (int)ComponentType.Essence }, //for runes at raritylevel.common
                { 2, "Stable Essence", (int)ComponentType.Essence }, //uncommon
                { 3, "Potent Essence", (int)ComponentType.Essence }, //rare
                { 4, "Refined Essence", (int)ComponentType.Essence }, //epic
                { 5, "Pristine Essence", (int)ComponentType.Essence }, //legendary
                { 6, "Eternal Essence", (int)ComponentType.Essence }, //mythic

                { 7, "Flame Shard", (int)ComponentType.Core }, //for runes of elementtype.fire
                { 8, "Arctic Shard", (int)ComponentType.Core }, //ice
                { 9, "Plasma Shard", (int)ComponentType.Core }, //lightning
                { 10, "Wild Shard", (int)ComponentType.Core }, //nature
                { 11, "Brilliant Shard", (int)ComponentType.Core }, //radiance
                { 12, "Sinister Shard", (int)ComponentType.Core }, //Abyssal
                //common monster parts
                { 13, "Bone Fragment", (int)ComponentType.MonsterPart },
                { 14, "Cracked Fang", (int)ComponentType.MonsterPart },
                { 15, "Tattered Hide", (int)ComponentType.MonsterPart },
                { 16, "Jagged Scale", (int)ComponentType.MonsterPart },
                { 17, "Brittle Talon", (int)ComponentType.MonsterPart },
                { 18, "Dulled Spike", (int)ComponentType.MonsterPart },
                { 19, "Sticky Ichor", (int)ComponentType.MonsterPart },
                { 20, "Crushed Eye Orb", (int)ComponentType.MonsterPart },
                { 21, "Thin Membrane", (int)ComponentType.MonsterPart },
                { 22, "Shriveled Gland", (int)ComponentType.MonsterPart },
                { 23, "Residual Puff", (int)ComponentType.MonsterPart },
                { 24, "Dull Residue", (int)ComponentType.MonsterPart },
                { 25, "Flickering Tendril", (int)ComponentType.MonsterPart },
                { 26, "Cloth Scrap", (int)ComponentType.MonsterPart },
                { 27, "Old Iron Nail", (int)ComponentType.MonsterPart },
                { 28, "Broken Weapon Bit", (int)ComponentType.MonsterPart },
                { 29, "Fraying Rope Fiber", (int)ComponentType.MonsterPart },
                //uncommon monster parts
                { 30, "Polished Fang", (int)ComponentType.MonsterPart },
                { 31, "Tough Claw", (int)ComponentType.MonsterPart },
                { 32, "Glinting Horn", (int)ComponentType.MonsterPart },
                { 33, "Sharpened Talon", (int)ComponentType.MonsterPart },
                { 34, "Razor Feather", (int)ComponentType.MonsterPart },
                { 35, "Pulsing Tendril", (int)ComponentType.MonsterPart },
                { 36, "Coiled Muscle Fiber", (int)ComponentType.MonsterPart },
                { 37, "Venom Gland", (int)ComponentType.MonsterPart },
                { 38, "Gleaming Husk", (int)ComponentType.MonsterPart },
                { 39, "Sap Cluster", (int)ComponentType.MonsterPart },
                { 40, "Luminous Web Strand", (int)ComponentType.MonsterPart },
                { 41, "Condensed Ichor", (int)ComponentType.MonsterPart },
                { 42, "Wispy Ether Silver", (int)ComponentType.MonsterPart },
                { 43, "Sturdy Hide", (int)ComponentType.MonsterPart },
                { 44, "Chipped Crystal", (int)ComponentType.MonsterPart },
                { 45, "Corrosive Slime Chunk", (int)ComponentType.MonsterPart },
                { 46, "Seared Scale", (int)ComponentType.MonsterPart },
                //rare monster parts
                { 47, "Eldritch Scale", (int)ComponentType.MonsterPart },
                { 48, "Bloodstained Claw", (int)ComponentType.MonsterPart },
                { 49, "Void-Touched Horn", (int)ComponentType.MonsterPart },
                { 50, "Celestial Feather", (int)ComponentType.MonsterPart },
                { 51, "Molten Core Shard", (int)ComponentType.MonsterPart },
                { 52, "Spectral Tendril", (int)ComponentType.MonsterPart },
                { 53, "Dragonbone Splinter", (int)ComponentType.MonsterPart },
                { 54, "Stormforged Spike", (int)ComponentType.MonsterPart },
                { 55, "Runebound Eye Orb", (int)ComponentType.MonsterPart },
                { 56, "Phantom Gland", (int)ComponentType.MonsterPart },
                { 57, "Titanhide Fragment", (int)ComponentType.MonsterPart },
                { 58, "Enchanted Venom Sac", (int)ComponentType.MonsterPart },
                { 59, "Crystalline Heartstone", (int)ComponentType.MonsterPart },
                { 60, "Abyssal Tendril", (int)ComponentType.MonsterPart },
                //epic monster parts
                { 61, "Phoenix Feather", (int)ComponentType.MonsterPart },
                { 62, "Titan’s Fang", (int)ComponentType.MonsterPart },
                { 63, "Ethereal Scale", (int)ComponentType.MonsterPart },
                { 64, "Infernal Claw", (int)ComponentType.MonsterPart },
                { 65, "Celestial Horn", (int)ComponentType.MonsterPart },
                { 66, "Runic Spine", (int)ComponentType.MonsterPart },
                { 67, "Dragonheart Shard", (int)ComponentType.MonsterPart },
                { 68, "Stormcaller’s Talon", (int)ComponentType.MonsterPart },
                { 69, "Void Crystal", (int)ComponentType.MonsterPart },
                { 70, "Soulbound Tendril", (int)ComponentType.MonsterPart },
                { 71, "Eclipse Eye", (int)ComponentType.MonsterPart },
                { 72, "Ancient Gland", (int)ComponentType.MonsterPart },
                { 73, "Primordial Hide", (int)ComponentType.MonsterPart },
                { 74, "Arcane Venom Sac", (int)ComponentType.MonsterPart },
                { 75, "Celestial Heartstone", (int)ComponentType.MonsterPart },
                //legendary monster parts
                { 76, "Eternal Dragon Scale", (int)ComponentType.MonsterPart },
                { 77, "Celestial Phoenix Claw", (int)ComponentType.MonsterPart },
                { 78, "Void Reaper Fang", (int)ComponentType.MonsterPart },
                { 79, "Titanheart Core", (int)ComponentType.MonsterPart },
                { 80, "Stormforged Talon", (int)ComponentType.MonsterPart },
                { 81, "Soulfire Essence", (int)ComponentType.MonsterPart },
                { 82, "Astral Horn Fragment", (int)ComponentType.MonsterPart },
                { 83, "Inferno Heartstone", (int)ComponentType.MonsterPart },
                { 84, "Primordial Bone Shard", (int)ComponentType.MonsterPart },
                { 85, "Voidwalker Tendril", (int)ComponentType.MonsterPart },
                { 86, "Eclipse Crystal Shard", (int)ComponentType.MonsterPart },
                { 87, "Mythic Venom Sac", (int)ComponentType.MonsterPart },
                { 88, "Celestial Heart of Gaia", (int)ComponentType.MonsterPart },
                { 89, "Dragonlord’s Fang", (int)ComponentType.MonsterPart },
                { 90, "Phoenix Soul Shard", (int)ComponentType.MonsterPart },
                //mythic monster parts
                // Fire
                { 91, "Emberwyrm Core", (int)ComponentType.MonsterPart },
                { 92, "Flareheart Shard", (int)ComponentType.MonsterPart },
                // Ice
                { 93, "Frostborn Crystal", (int)ComponentType.MonsterPart },
                { 94, "Glacierfang Fragment", (int)ComponentType.MonsterPart },
                // Lightning
                { 95, "Stormspark Tendril", (int)ComponentType.MonsterPart },
                { 96, "Thunderstrike Claw", (int)ComponentType.MonsterPart },
                // Nature
                { 97, "Verdant Heartwood", (int)ComponentType.MonsterPart },
                { 98, "Wildroot Bark", (int)ComponentType.MonsterPart },
                // Radiance
                { 99, "Solaris Ember", (int)ComponentType.MonsterPart },
                { 100, "Radiant Vein", (int)ComponentType.MonsterPart },
                // Abyssal
                { 101, "Nethercore Essence", (int)ComponentType.MonsterPart },
                { 102, "Shadowvein Tendril", (int)ComponentType.MonsterPart }
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT Ingredients OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT Recipes ON");
        migrationBuilder.InsertData(
             table: "Recipes",
             columns: new[] { "Id", "Name", "RuneId" },
             values: new object[,]
             {
                { 1, "Ember Rune (Weapon)", 1 },
                { 2, "Ember Rune (Armor)", 7 },
                { 3, "Fire Rune (Weapon)", 2 },
                { 4, "Fire Rune (Armor)", 8 },
                { 5, "Blaze Rune (Weapon)", 3 },
                { 6, "Blaze Rune (Armor)", 9 },
                { 7, "Lava Rune (Weapon)", 4 },
                { 8, "Lava Rune (Armor)", 10 },

                { 9, "Inferno Rune (Weapon)", 5 },
                { 10, "Inferno Rune (Armor)", 11 },

                { 11, "Phoenix Rune (Weapon)", 6 },
                { 12, "Phoenix Rune (Armor)", 12 },

                { 13, "Frost Rune (Weapon)", 13 },
                { 14, "Frost Rune (Armor)", 19 },
                { 15, "Ice Rune (Weapon)", 14 },
                { 16, "Ice Rune (Armor)", 20 },
                { 17, "Hail Rune (Weapon)", 15 },
                { 18, "Hail Rune (Armor)", 21 },
                { 19, "Glacier Rune (Weapon)", 16 },
                { 20, "Glacier Rune (Armor)", 22 },

                { 21, "Blizzard Rune (Weapon)", 17 },
                { 22, "Blizzard Rune (Armor)", 23 },

                { 23, "Ymir Rune (Weapon)", 18 },
                { 24, "Ymir Rune (Armor)", 24 },

                { 25, "Spark Rune (Weapon)", 25 },
                { 26, "Spark Rune (Armor)", 31 },
                { 27, "Jolt Rune (Weapon)", 26 },
                { 28, "Jolt Rune (Armor)", 32 },
                { 29, "Lightning Rune (Weapon)", 27 },
                { 30, "Lightning Rune (Armor)", 33 },
                { 31, "Storm Rune (Weapon)", 28 },
                { 32, "Storm Rune (Armor)", 34 },

                { 33, "Tempest Rune (Weapon)", 29 },
                { 34, "Tempest Rune (Armor)", 35 },

                { 35, "Raijin Rune (Weapon)", 30 },
                { 36, "Raijin Rune (Armor)", 36 },

                { 37, "Seed Rune (Weapon)", 37 },
                { 38, "Seed Rune (Armor)", 43 },
                { 39, "Nature Rune (Weapon)", 38 },
                { 40, "Nature Rune (Armor)", 44 },
                { 41, "Thorn Rune (Weapon)", 39 },
                { 42, "Thorn Rune (Armor)", 45 },
                { 43, "Beast Rune (Weapon)", 40 },
                { 44, "Beast Rune (Armor)", 46 },

                { 45, "Quake Rune (Weapon)", 41 },
                { 46, "Quake Rune (Armor)", 47 },

                { 47, "Gaia Rune (Weapon)", 42 },
                { 48, "Gaia Rune (Armor)", 48 },

                { 49, "Bright Rune (Weapon)", 49 },
                { 50, "Bright Rune (Armor)", 55 },
                { 51, "Energy Rune (Weapon)", 50 },
                { 52, "Energy Rune (Armor)", 56 },
                { 53, "Radiant Rune (Weapon)", 51 },
                { 54, "Radiant Rune (Armor)", 57 },
                { 55, "Solar Rune (Weapon)", 52 },
                { 56, "Solar Rune (Armor)", 58 },

                { 57, "Divine Rune (Weapon)", 53 },
                { 58, "Divine Rune (Armor)", 59 },

                { 59, "Seraph Rune (Weapon)", 54 },
                { 60, "Seraph Rune (Armor)", 60 },

                { 61, "Shadow Rune (Weapon)", 61 },
                { 62, "Shadow Rune (Armor)", 67 },
                { 63, "Gloom Rune (Weapon)", 62 },
                { 64, "Gloom Rune (Armor)", 68 },
                { 65, "Abyss Rune (Weapon)", 63 },
                { 66, "Abyss Rune (Armor)", 69 },
                { 67, "Void Rune (Weapon)", 64 },
                { 68, "Void Rune (Armor)", 70 },

                { 69, "Nether Rune (Weapon)", 65 },
                { 70, "Nether Rune (Armor)", 71 },

                { 71, "Nyx Rune (Weapon)", 66 },
                { 72, "Nyx Rune (Armor)", 72 }
             });
        migrationBuilder.Sql("SET IDENTITY_INSERT Recipes OFF");

        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients ON");
        migrationBuilder.InsertData(
            table: "RecipeIngredients",
            columns: new[] { "Id", "RecipeId", "IngredientId", "Quantity" },
            values: new object[,]
            {
                // Ember Rune (Weapon)
                { 1, 1, 1, 3 }, // 3 Fading Essence
                { 2, 1, 7, 1 }, // 1 Flame Shard
                { 3, 1, 17, 3 }, // 3 Brittle Talon
                // Ember Rune (Armor)
                { 4, 2, 1, 3 }, // 3 Fading Essence
                { 5, 2, 7, 1 }, //  1 Flame Shard
                { 6, 2, 23, 3 }, // 3 Residual Puff
                //Frost Rune (Weapon)
                { 7, 13, 1, 3 }, // 3 Fading Essence
                { 8, 13, 8, 1 }, // 1 Arctic Shard
                { 9, 13, 19, 3 }, // 3 Thin Membrane
                // Frost Rune (Armor)
                { 10, 14, 1, 3 }, // 3 Fading Essence
                { 11, 14, 8, 1 }, // 1 Arctic Shard
                { 12, 14, 24, 3 }, // 3 Dull Residue
                //Spark Rune (Weapon)
                { 13, 25, 1, 3 }, // 3 Fading Essence
                { 14, 25, 9, 1 }, // 1 Plasma Shard
                { 15, 25, 20, 3 }, // 3 Shriveled Gland
                // Spark Rune (Armor)
                { 16, 26, 1, 3 }, // 3 Fading Essence
                { 17, 26, 9, 1 }, // 1 Plasma Shard
                { 18, 26, 25, 3 }, // 3 Flickering Tendril
                //Seed Rune (Weapon)
                { 19, 37, 1, 3 }, // 3 Fading Essence
                { 20, 37, 10, 1 }, // 1 Wild Shard
                { 21, 37, 26, 3 }, // 3 Dulled Spike
                // Seed Rune (Armor)
                { 22, 38, 1, 3 }, // 3 Fading Essence
                { 23, 38, 10, 1 }, // 1 Wild Shard
                { 24, 38, 29, 3 }, // 3 Fraying Rope Fiber
                //Bright Rune (Weapon)
                { 25, 49, 1, 3 }, // 3 Fading Essence
                { 26, 49, 11, 1 }, // 1 Brilliant Shard
                { 27, 49, 27, 3 }, // 3 Cloth Scrap
                // Bright Rune (Armor)
                { 28, 50, 1, 3 }, // 3 Fading Essence
                { 29, 50, 11, 1 }, // 1 Brilliant Shard
                { 30, 50, 13, 3 }, // 3 Bone Fragment
                //Shadow Rune (Weapon)
                { 31, 61, 1, 3 }, // 3 Fading Essence
                { 32, 61, 12, 1 }, // 1 Sinister Shard
                { 33, 61, 28, 3 }, // 3 Broken Weapon Bit
                //Shadow Rune (Armor)
                { 34, 62, 1, 3 }, // 3 Fading Essence
                { 35, 62, 12, 1 }, // 1 Sinister Shard
                { 36, 62, 16, 3 }, // 3 Jagged Scale

                // Fire Rune (Weapon)
                { 37, 3, 2, 5 }, // Stable Essence
                { 38, 3, 7, 3 }, // Flame Shard
                { 39, 3, 14, 5 }, // Cracked Fang
                { 40, 3, 30, 3 }, // Polished Fang
                // Fire Rune (Armor)
                { 41, 4, 2, 5 },
                { 42, 4, 7, 3 },
                { 43, 4, 13, 5 }, // Bone Fragment
                { 44, 4, 31, 3 }, // Tough Claw
                // Ice Rune (Weapon)
                { 45, 15, 2, 5 },
                { 46, 15, 8, 3 },
                { 47, 15, 16, 5 }, // Jagged Scale
                { 48, 15, 33, 3 }, // Sharpened Talon
                // Ice Rune (Armor)
                { 49, 16, 2, 5 },
                { 50, 16, 8, 3 },
                { 51, 16, 15, 5 }, // Tattered Hide
                { 52, 16, 34, 3 }, // Razor Feather
                // Jolt Rune (Weapon)
                { 53, 27, 2, 5 },
                { 54, 27, 9, 3 },
                { 55, 27, 21, 5 }, // Thin Membrane
                { 56, 27, 35, 3 }, // Pulsing Tendril
                // Jolt Rune (Armor)
                { 57, 28, 2, 5 },
                { 58, 28, 9, 3 },
                { 59, 28, 22, 5 }, // Shriveled Gland
                { 60, 28, 36, 3 }, // Coiled Muscle Fiber
                // Nature Rune (Weapon)
                { 61, 39, 2, 5 },
                { 62, 39, 10, 3 },
                { 63, 39, 18, 5 }, // Dulled Spike
                { 64, 39, 37, 3 }, // Venom Gland
                // Nature Rune (Armor)
                { 65, 40, 2, 5 },
                { 66, 40, 10, 3 },
                { 67, 40, 19, 5 }, // Sticky Ichor
                { 68, 40, 39, 3 }, // Sap Cluster
                // Energy Rune (Weapon)
                { 69, 51, 2, 5 },
                { 70, 51, 11, 3 },
                { 71, 51, 17, 5 }, // Brittle Talon
                { 72, 51, 32, 3 }, // Glinting Horn
                // Energy Rune (Armor)
                { 73, 52, 2, 5 },
                { 74, 52, 11, 3 },
                { 75, 52, 20, 5 }, // Crushed Eye Orb
                { 76, 52, 40, 3 }, // Luminous Web Strand
                // Gloom Rune (Weapon)
                { 77, 63, 2, 5 },
                { 78, 63, 12, 3 },
                { 79, 63, 23, 5 }, // Residual Puff
                { 80, 63, 38, 3 }, // Gleaming Husk
                // Gloom Rune (Armor)
                { 81, 64, 2, 5 },
                { 82, 64, 12, 3 },
                { 83, 64, 24, 5 }, // Dull Residue
                { 84, 64, 41, 3 },  // Condensed Ichor

                // Blaze Rune (Weapon)
                { 85, 5, 3, 7 },   // Potent Essence
                { 86, 5, 7, 5 },   // Flame Shard
                { 87, 5, 30, 5 },  // Polished Fang
                { 88, 5, 51, 3 },  // Molten Core Shard
                { 89, 5, 47, 1 },  // Eldritch Scale
                // Blaze Rune (Armor)
                { 90, 6, 3, 7 },
                { 91, 6, 7, 5 },
                { 92, 6, 31, 5 },  // Tough Claw
                { 93, 6, 55, 3 },  // Runebound Eye Orb
                { 94, 6, 48, 1 },  // Bloodstained Claw
                // Hail Rune (Weapon)
                { 95, 17, 3, 7 },
                { 96, 17, 8, 5 },
                { 97, 17, 33, 5 }, // Sharpened Talon
                { 98, 17, 56, 3 }, // Phantom Gland
                { 99, 17, 48, 1 }, // Bloodstained Claw
                // Hail Rune (Armor)
                { 100, 18, 3, 7 },
                { 101, 18, 8, 5 },
                { 102, 18, 34, 5 }, // Razor Feather
                { 103, 18, 57, 3 }, // Titanhide Fragment
                { 104, 18, 49, 1 }, // Void-Touched Horn
                // Lightning Rune (Weapon)
                { 105, 29, 3, 7 },
                { 106, 29, 9, 5 },
                { 107, 29, 35, 5 }, // Pulsing Tendril
                { 108, 29, 54, 3 }, // Stormforged Spike
                { 109, 29, 49, 1 }, // Void-Touched Horn
                // Lightning Rune (Armor)
                { 110, 30, 3, 7 },
                { 111, 30, 9, 5 },
                { 112, 30, 36, 5 }, // Coiled Muscle Fiber
                { 113, 30, 53, 3 }, // Dragonbone Splinter
                { 114, 30, 50, 1 }, // Celestial Feather
                // Thorn Rune (Weapon)
                { 115, 41, 3, 7 },
                { 116, 41, 10, 5 },
                { 117, 41, 37, 5 }, // Venom Gland
                { 118, 41, 58, 3 }, // Enchanted Venom Sac
                { 119, 41, 57, 1 }, // Titanhide Fragment
                // Thorn Rune (Armor)
                { 120, 42, 3, 7 },
                { 121, 42, 10, 5 },
                { 122, 42, 39, 5 }, // Sap Cluster
                { 123, 42, 52, 3 }, // Spectral Tendril
                { 124, 42, 46, 1 }, // Seared Scale
                // Radiant Rune (Weapon)
                { 125, 53, 3, 7 },
                { 126, 53, 11, 5 },
                { 127, 53, 40, 5 }, // Luminous Web Strand
                { 128, 53, 59, 3 }, // Crystalline Heartstone
                { 129, 53, 50, 1 }, // Celestial Feather
                // Radiant Rune (Armor)
                { 130, 54, 3, 7 },
                { 131, 54, 11, 5 },
                { 132, 54, 42, 5 }, // Wispy Ether Silver
                { 133, 54, 53, 3 }, // Dragonbone Splinter
                { 134, 54, 55, 1 }, // Runebound Eye Orb
                // Abyss Rune (Weapon)
                { 135, 65, 3, 7 },
                { 136, 65, 12, 5 },
                { 137, 65, 41, 5 }, // Condensed Ichor
                { 138, 65, 60, 3 }, // Abyssal Tendril
                { 139, 65, 52, 1 }, // Spectral Tendril
                // Abyss Rune (Armor)
                { 140, 66, 3, 7 },
                { 141, 66, 12, 5 },
                { 142, 66, 43, 5 }, // Sturdy Hide
                { 143, 66, 59, 3 }, // Crystalline Heartstone
                { 144, 66, 54, 1 },  // Stormforged Spike

                // Lava Rune (Weapon: 7)
                { 145, 7, 4, 9 },  // Refined Essence
                { 146, 7, 7, 7 }, // Flame Core
                { 147, 7, 47, 3 }, // Eldritch Scale
                { 148, 7, 51, 7 }, // Molten Core Shard
                { 149, 7, 61, 2 }, // Phoenix Feather
                { 150, 7, 64, 3 }, // Infernal Claw
                // Lava Rune (Armor: 8)
                { 151, 8, 4, 9 },
                { 152, 8, 7, 7 },
                { 153, 8, 48, 3 }, // Bloodstained Claw
                { 154, 8, 53, 7 }, // Dragonbone Splinter
                { 155, 8, 62, 2 }, // Titan’s Fang
                { 156, 8, 67, 3 }, // Dragonheart Shard
                // Glacier Rune (Weapon: 19)
                { 157, 19, 4, 9 },
                { 158, 19, 8, 7 }, // Arctic Core
                { 159, 19, 49, 3 }, // Void-Touched Horn
                { 160, 19, 55, 7 }, // Runebound Eye Orb
                { 161, 19, 63, 2 }, // Ethereal Scale
                { 162, 19, 66, 3 }, // Runic Spine
                // Glacier Rune (Armor: 20)
                { 163, 20, 4, 9 },
                { 164, 20, 8, 7 },
                { 165, 20, 50, 3 }, // Celestial Feather
                { 166, 20, 56, 7 }, // Phantom Gland
                { 167, 20, 65, 2 }, // Celestial Horn
                { 168, 20, 72, 3 }, // Ancient Gland
                // Storm Rune (Weapon: 31)
                { 169, 31, 4, 9 },
                { 170, 31, 9, 7 }, // Plasma Core
                { 171, 31, 52, 3 }, // Spectral Tendril
                { 172, 31, 54, 7 }, // Stormforged Spike
                { 173, 31, 68, 2 }, // Stormcaller’s Talon
                { 174, 31, 70, 3 }, // Soulbound Tendril
                // Storm Rune (Armor: 32)
                { 175, 32, 4, 9 },
                { 176, 32, 9, 7 },
                { 177, 32, 57, 3 }, // Titanhide Fragment
                { 178, 32, 58, 7 }, // Enchanted Venom Sac
                { 179, 32, 69, 2 }, // Void Crystal
                { 180, 32, 73, 3 }, // Primordial Hide
                // Beast Rune (Weapon: 43)
                { 181, 43, 4, 9 },
                { 182, 43, 10, 7 }, // Wild Core
                { 183, 43, 59, 3 }, // Crystalline Heartstone
                { 184, 43, 60, 7 }, // Abyssal Tendril
                { 185, 43, 71, 2 }, // Eclipse Eye
                { 186, 43, 74, 3 }, // Arcane Venom Sac
                // Beast Rune (Armor: 44)
                { 187, 44, 4, 9 },
                { 188, 44, 10, 7 },
                { 189, 44, 47, 3 }, // Eldritch Scale (reused)
                { 190, 44, 48, 7 }, // Bloodstained Claw (reused)
                { 191, 44, 62, 2 }, // Titan’s Fang (reused)
                { 192, 44, 75, 3 }, // Celestial Heartstone
                // Solar Rune (Weapon: 55)
                { 193, 55, 4, 9 },
                { 194, 55, 11, 7 }, // Brilliant Core
                { 195, 55, 49, 3 }, // Void-Touched Horn (reused)
                { 196, 55, 50, 7 }, // Celestial Feather (reused)
                { 197, 55, 63, 2 }, // Ethereal Scale (reused)
                { 198, 55, 61, 3 }, // Phoenix Feather (reused)
                // Solar Rune (Armor: 56)
                { 199, 56, 4, 9 },
                { 200, 56, 11, 7 },
                { 201, 56, 51, 3 }, // Molten Core Shard (reused)
                { 202, 56, 53, 7 }, // Dragonbone Splinter (reused)
                { 203, 56, 67, 2 }, // Dragonheart Shard (reused)
                { 204, 56, 75, 3 }, // Celestial Heartstone (reused)
                // Void Rune (Weapon: 67)
                { 205, 67, 4, 9 },
                { 206, 67, 12, 7 }, // Sinister Core
                { 207, 67, 58, 3 }, // Enchanted Venom Sac (reused)
                { 208, 67, 52, 7 }, // Spectral Tendril (reused)
                { 209, 67, 66, 2 }, // Runic Spine (reused)
                { 210, 67, 69, 3 }, // Void Crystal (reused)
                // Void Rune (Armor: 68)
                { 211, 68, 4, 9 },
                { 212, 68, 12, 7 },
                { 213, 68, 54, 3 }, // Stormforged Spike (reused)
                { 214, 68, 55, 7 }, // Runebound Eye Orb (reused)
                { 215, 68, 70, 2 }, // Soulbound Tendril (reused)
                { 216, 68, 73, 3 },  // Primordial Hide (reused)

                // Inferno Rune
                { 217, 9, 5, 11 },
                { 218, 9, 7, 9 },
                { 219, 9, 76, 1 },
                { 220, 9, 83, 2 },
                { 221, 9, 89, 1 },
                { 222, 10, 5, 11 },
                { 223, 10, 7, 9 },
                { 224, 10, 76, 2 },
                { 225, 10, 83, 1 },
                { 226, 10, 89, 1 },
                // Blizzard Rune
                { 227, 21, 5, 11 },
                { 228, 21, 8, 9 },
                { 229, 21, 77, 1 },
                { 230, 21, 86, 2 },
                { 231, 21, 90, 1 },
                { 232, 22, 5, 11 },
                { 233, 22, 8, 9 },
                { 234, 22, 77, 1 },
                { 235, 22, 86, 2 },
                { 236, 22, 90, 1 },
                // Tempest Rune
                { 237, 33, 5, 11 },
                { 238, 33, 9, 9 },
                { 239, 33, 80, 1 },
                { 240, 33, 81, 1 },
                { 241, 33, 96, 2 },
                { 242, 34, 5, 11 },
                { 243, 34, 9, 9 },
                { 244, 34, 80, 1 },
                { 245, 34, 81, 2 },
                { 246, 34, 96, 1 },
                // Quake Rune
                { 247, 45, 5, 11 },
                { 248, 45, 10, 9 },
                { 249, 45, 84, 1 },
                { 250, 45, 88, 1 },
                { 251, 45, 98, 2 },
                { 252, 46, 5, 11 },
                { 253, 46, 10, 9 },
                { 254, 46, 84, 2 },
                { 255, 46, 88, 1 },
                { 256, 46, 98, 1 },
                // Divine Rune
                { 257, 57, 5, 11 },
                { 258, 57, 11, 9 },
                { 259, 57, 82, 1 },
                { 260, 57, 90, 2 },
                { 261, 57, 75, 1 },
                { 262, 58, 5, 11 },
                { 263, 58, 11, 9 },
                { 264, 58, 82, 1 },
                { 265, 58, 90, 1 },
                { 266, 58, 75, 2 },
                // Nether Rune
                { 267, 69, 5, 11 },
                { 268, 69, 12, 9 },
                { 269, 69, 78, 2 },
                { 270, 69, 85, 1 },
                { 271, 69, 86, 1 },
                { 272, 70, 5, 11 },
                { 273, 70, 12, 9 },
                { 274, 70, 78, 1 },
                { 275, 70, 85, 2 },
                { 276, 70, 86, 1 },

                // Phoenix Rune
                { 277, 11, 6, 15 },
                { 278, 11, 7, 15 },
                { 279, 11, 83, 5 },
                { 280, 11, 89, 5 },
                { 281, 11, 91, 1 }, // Emberwyrm Core (weapon)
                { 282, 12, 6, 15 },
                { 283, 12, 7, 15 },
                { 284, 12, 83, 5 },
                { 285, 12, 89, 5 },
                { 286, 12, 92, 1 }, // Flareheart Shard (armor)
                // Ymir Rune
                { 287, 23, 6, 15 },
                { 288, 23, 8, 15 },
                { 289, 23, 86, 5 },
                { 290, 23, 90, 5 },
                { 291, 23, 93, 1 }, // Frostborn Crystal (weapon)
                { 292, 24, 6, 15 },
                { 293, 24, 8, 15 },
                { 294, 24, 86, 5 },
                { 295, 24, 90, 5 },
                { 296, 24, 94, 1 }, // Glacierfang Fragment (armor)
                // Raijin Rune
                { 297, 35, 6, 15 },
                { 298, 35, 9, 15 },
                { 299, 35, 81, 5 },
                { 300, 35, 96, 5 },
                { 301, 35, 95, 1 }, // Stormspark Tendril (weapon)
                { 302, 36, 6, 15 },
                { 303, 36, 9, 15 },
                { 304, 36, 81, 5 },
                { 305, 36, 96, 5 },
                { 306, 36, 96, 1 }, // Thunderstrike Claw (armor)
                // Gaia Rune
                { 307, 47, 6, 15 },
                { 308, 47, 10, 15 },
                { 309, 47, 84, 5 },
                { 310, 47, 98, 5 },
                { 311, 47, 97, 1 }, // Verdant Heartwood (weapon)
                { 312, 48, 6, 15 },
                { 313, 48, 10, 15 },
                { 314, 48, 84, 5 },
                { 315, 48, 98, 5 },
                { 316, 48, 98, 1 }, // Wildroot Bark (armor)
                // Seraph Rune
                { 317, 59, 6, 15 },
                { 318, 59, 1, 15 },
                { 319, 59, 82, 5 },
                { 320, 59, 90, 5 },
                { 321, 59, 99, 1 }, // Solaris Ember (weapon)
                { 322, 60, 6, 15 },
                { 323, 60, 11, 15 },
                { 324, 60, 82, 5 },
                { 325, 60, 90, 5 },
                { 326, 60, 100, 1 }, // Radiant Vein (armor)
                // Nyx Rune
                { 327, 71, 6, 15 },
                { 328, 71, 12, 15 },
                { 329, 71, 85, 5 },
                { 330, 71, 86, 5 },
                { 331, 71, 101, 1 }, // Nethercore Essence (weapon)
                { 332, 72, 6, 15 },
                { 333, 72, 12, 15 },
                { 334, 72, 85, 5 },
                { 335, 72, 86, 5 },
                { 336, 72, 102, 1 }  // Shadowvein Tendril (armor)
            });
        migrationBuilder.Sql("SET IDENTITY_INSERT RecipeIngredients OFF");

        // Seed MonsterDrops table with essence drops
        var elementTypes = Enumerable.Range(0, 6); // 0 to 5
        var essenceDropTable = new List<object[]>();

        var essenceDropRatesByThreat = new Dictionary<int, Dictionary<int, decimal>> // new Dictionary<threatLevel, Dictionary<essenceId, dropRate>>
        {
            [0] = new() { [1] = 25.0M, [2] = 10.0M, [3] = 1.0M },
            [1] = new() { [1] = 30.0M, [2] = 15.0M, [3] = 5.0M, [4] = 1.0M },
            [2] = new() { [1] = 35.0M, [2] = 20.0M, [3] = 10.0M, [4] = 5.0M, [5] = 1.0M },
            [3] = new() { [1] = 40.0M, [2] = 25.0M, [3] = 15.0M, [4] = 10.0M, [5] = 5.0M, [6] = 1.0M },
            [4] = new() { [1] = 60.0M, [2] = 35.0M, [3] = 25.0M, [4] = 20.0M, [5] = 10.0M, [6] = 5.0M }
        };

        foreach (var threatLevel in essenceDropRatesByThreat.Keys)
        {
            foreach (var element in elementTypes)
            {
                foreach (var kvp in essenceDropRatesByThreat[threatLevel])
                {
                    essenceDropTable.Add(new object[] { element, threatLevel, kvp.Key, kvp.Value });
                }
            }
        }

        // Seed MonsterDrops table with core drops
        var coreIdByElement = new Dictionary<ElementType, int>
        {
            { ElementType.Fire, 7 },
            { ElementType.Ice, 8 },
            { ElementType.Lightning, 9 },
            { ElementType.Nature, 10 },
            { ElementType.Radiance, 11 },
            { ElementType.Abyssal, 12 }
        };        

        var coreDropRatesByThreat = new List<decimal>()
        {
            10.0M, // ThreatLevel 0, 10%
            20.0M, // ThreatLevel 1, 20%
            30.0M, // ThreatLevel 2, 30%
            40.0M, // ThreatLevel 3, 40%
            50.0M, // ThreatLevel 4, 50%
        };

        var coreDropTable = new List<object[]>();

        for (int threatLevel = 0; threatLevel < coreDropRatesByThreat.Count; threatLevel++)
        {
            foreach (ElementType element in elementTypes)
            {
                if (!coreIdByElement.TryGetValue(element, out int coreId))
                    continue; // Skip elements without a mapped core

                coreDropTable.Add(new object[] { (int)element, threatLevel, coreId, coreDropRatesByThreat[threatLevel] });
            }
        }

        //Seed MonsterDrops table with monster part drops
        // Threat Level 0, Common Ingredients (13-29), drop rate 30%
        // Threat Level 0, Uncommon Ingredients (30-46), drop rate 15%
        // Threat Level 0, Rare Ingredients (47-60), drop rate 5%

        // Threat Level 1, Common Ingredients (13-29), drop rate 40%
        // Threat Level 1, Uncommon Ingredients (30-46), drop rate 30%
        // Threat Level 1, Rare Ingredients (47-60), drop rate 15%
        // Threat Level 1, Epic Ingredients (61-75), drop rate 5%

        // Threat Level 2, Common Ingredients (13-29), drop rate 20%
        // Threat Level 2, Uncommon Ingredients (30-46), drop rate 40%
        // Threat Level 2, Rare Ingredients (47-60), drop rate 30%
        // Threat Level 2, Epic Ingredients (61-75), drop rate 15%
        // Threat Level 2, Legendary Ingredients (76-90), drop rate 5%

        // Threat Level 3, Uncommon Ingredients (30-46), drop rate 20%
        // Threat Level 3, Rare Ingredients (47-60), drop rate 40%
        // Threat Level 3, Epic Ingredients (61-75), drop rate 30%
        // Threat Level 3, Legendary Ingredients (76-90), drop rate 10%
        // Threat Level 3, Mythic Ingredients (91-105), drop rate 1%

        // Threat Level 4, Rare Ingredients (47-60), drop rate 20%
        // Threat Level 4, Epic Ingredients (61-75), drop rate 40%
        // Threat Level 4, Legendary Ingredients (76-90), drop rate 30%
        // Threat Level 4, Mythic Ingredients (91-102), drop rate 5%

        var ingredientTiers = new Dictionary<RarityLevel, (int start, int end)>
        {
            { RarityLevel.Common, (13, 29) },
            { RarityLevel.Uncommon, (30, 46) },
            { RarityLevel.Rare, (47, 60) },
            { RarityLevel.Epic, (61, 75) },
            { RarityLevel.Legendary, (76, 90) },
            { RarityLevel.Mythic, (91, 102) },
        };

        var monsterPartDropsByThreat = new Dictionary<int, List<(RarityLevel rarity, decimal dropRate)>>()
        {
            [0] = new() {
                (RarityLevel.Common, 30.0M),
                (RarityLevel.Uncommon, 15.0M),
                (RarityLevel.Rare, 5.0M),
            },
            [1] = new() {
                (RarityLevel.Common, 40.0M),
                (RarityLevel.Uncommon, 30.0M),
                (RarityLevel.Rare, 15.0M),
                (RarityLevel.Epic, 5.0M),
            },
            [2] = new() {
                (RarityLevel.Common, 20.0M),
                (RarityLevel.Uncommon, 40.0M),
                (RarityLevel.Rare, 30.0M),
                (RarityLevel.Epic, 15.0M),
                (RarityLevel.Legendary, 5.0M),
            },
            [3] = new() {
                (RarityLevel.Uncommon, 20.0M),
                (RarityLevel.Rare, 40.0M),
                (RarityLevel.Epic, 30.0M),
                (RarityLevel.Legendary, 10.0M),
                (RarityLevel.Mythic, 1.0M),
            },
            [4] = new() {
                (RarityLevel.Rare, 20.0M),
                (RarityLevel.Epic, 40.0M),
                (RarityLevel.Legendary, 30.0M),
                (RarityLevel.Mythic, 5.0M),
            },
        };

        var monsterPartDropTable = new List<object[]>();
        var baseElements = elementTypes.ToList();

        foreach (var threatEntry in monsterPartDropsByThreat)
        {
            int threatLevel = threatEntry.Key;
            var rarityDrops = threatEntry.Value;

            foreach (var (rarity, dropRate) in rarityDrops)
            {
                var (startId, endId) = ingredientTiers[rarity];
                int count = endId - startId + 1;

                // Repeat element list to match ingredient count
                var expandedElements = Enumerable.Range(0, count)
                .Select(i => baseElements[i % baseElements.Count])
                .OrderBy(_ => Random.Shared.Next())
                .ToList();

                int index = 0;
                for (int id = startId; id <= endId; id++)
                {
                    var element = expandedElements[index++];
                    monsterPartDropTable.Add(new object[] { element, threatLevel, id, dropRate });
                }
            }
        }

        // Combine all drop tables
        var combinedDropTable = essenceDropTable
            .Concat(coreDropTable)
            .Concat(monsterPartDropTable)
            .ToList();

        // Insert into MonsterDrops table
        migrationBuilder.InsertData(
            table: "MonsterDrops",
            columns: new[] { "Element", "ThreatLevel", "IngredientId", "DropRate" },
            values: To2DArray(combinedDropTable));

        //seed monsters with elements
        var elements = Enumerable.Range(0, 7); // 0-5 are elements, 6 = "None"

        for (int monsterId = 1; monsterId < 29; monsterId++)
        {
            var power = Random.Shared.Next(0, 3);
            var randElement = elements.ElementAt(Random.Shared.Next(elements.Count()));
            var randElement2 = elements.ElementAt(Random.Shared.Next(elements.Count()));

            while (randElement2 == randElement)
            {
                randElement2 = elements.ElementAt(Random.Shared.Next(elements.Count()));
            }

            int? attackElement = randElement == 6 ? (int?)null : randElement;
            int? vulnElement = randElement2 == 6 ? (int?)null : randElement2;
            int? elementalPower = attackElement == null ? (int?)null : power;

            string sql = $@"
                UPDATE Monsters
                SET 
                    AttackElement = {(attackElement?.ToString() ?? "NULL")},
                    ElementalPower = {(elementalPower?.ToString() ?? "NULL")},
                    Vulnerability = {(vulnElement?.ToString() ?? "NULL")}
                WHERE Id = {monsterId};
            ";

            migrationBuilder.Sql(sql);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM Archetypes");
        migrationBuilder.Sql("DELETE FROM Players");
        migrationBuilder.Sql("DELETE FROM Rooms");
        migrationBuilder.Sql("DELETE FROM Monsters");
        migrationBuilder.Sql("DELETE FROM Items");
        migrationBuilder.Sql("DELETE FROM Inventories");
        migrationBuilder.Sql("DELETE FROM Skills");
        migrationBuilder.Sql("DELETE FROM RecipeIngredients");
        migrationBuilder.Sql("DELETE FROM Recipes");
        migrationBuilder.Sql("DELETE FROM Runes");
        migrationBuilder.Sql("DELETE FROM Ingredients");
        migrationBuilder.Sql("DELETE FROM MonsterDrops");
    }

    private static object[,] To2DArray(List<object[]> data)
    {
        int rows = data.Count;
        int cols = data[0].Length;
        var result = new object[rows, cols];
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                result[i, j] = data[i][j];
        return result;
    }
}
