using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class SeedRoomData : Migration
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
                {19, "Cursed Hall", "The air is colder here, and your footsteps echo too loudly, as if watched by unseen eyes.", null, 2, null, null},
                {20, "Divination Room", "Cracked crystal balls and scattered cards suggest omens too dark to bear.", 60, 59, null, 13},
                {21, "Dungeon", "Chains hang loosely from the damp, mossy walls of cells that remember every scream.", 52, 36, 15, 5},
                {22, "Gallery of Heroes", "Portraits gaze blankly, their faces faded and cracked beneath centuries of dust.", 28, null, 30, null},
                {23, "Garden Atrium", "Vines choke the statues and cracked fountains of a courtyard nature has reclaimed.", null, null, 39, null},
                {24, "Gatehouse", "The portcullis hangs at a crooked angle, groaning in the wind like a warning.", 14, null, null, 62},
                {25, "Great Hall", "Once a place of feasts, now an echoing ruin filled with shattered goblets and broken chairs.", 38, null, null, 43},
                {26, "Guardroom", "Empty weapon racks and a stale odor linger where sentries once gathered.", null, 3, null, 64},
                {27, "Hall of Mirrors", "Most mirrors are shattered, reflecting only fragments of those who dare pass through.", 29, null, 41, 14},
                {28, "Hall of Tapestries", "Moths feast on decaying fabric, the heroic scenes now just silhouettes and dust.", 6, 22, null, null},
                {29, "Kennels", "Cracked bones and rotting leashes remain in these empty cages.", null, 27, 53, 1},
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

        migrationBuilder.Sql("UPDATE Monsters SET RoomId = 3 WHERE Id = 1;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DELETE FROM Rooms;");
        migrationBuilder.Sql("UPDATE Monsters SET RoomId = NULL WHERE Id = 1;");
    }
}
