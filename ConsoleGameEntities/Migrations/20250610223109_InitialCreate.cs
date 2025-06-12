using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Archetypes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                HealthBase = table.Column<int>(type: "int", nullable: false),
                AttackBonus = table.Column<int>(type: "int", nullable: false),
                MagicBonus = table.Column<int>(type: "int", nullable: false),
                DefenseBonus = table.Column<int>(type: "int", nullable: false),
                ResistanceBonus = table.Column<int>(type: "int", nullable: false),
                Speed = table.Column<int>(type: "int", nullable: false),
                ArchetypeType = table.Column<int>(type: "int", nullable: false),
                ResourceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MaxResource = table.Column<int>(type: "int", nullable: false),
                RecoveryRate = table.Column<int>(type: "int", nullable: false),
                AttackMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                MagicMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                DefenseMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                ResistanceMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                SpeedMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                ResourceMultiplier = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                RecoveryGrowth = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Archetypes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Ingredients",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ComponentType = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ingredients", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                NorthId = table.Column<int>(type: "int", nullable: true),
                SouthId = table.Column<int>(type: "int", nullable: true),
                WestId = table.Column<int>(type: "int", nullable: true),
                EastId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_EastId",
                    column: x => x.EastId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_NorthId",
                    column: x => x.NorthId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_SouthId",
                    column: x => x.SouthId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_WestId",
                    column: x => x.WestId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Runes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RuneType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Power = table.Column<int>(type: "int", nullable: false),
                Tier = table.Column<int>(type: "int", nullable: false),
                Element = table.Column<int>(type: "int", nullable: false),
                Rarity = table.Column<int>(type: "int", nullable: false),
                RecipeId = table.Column<int>(type: "int", nullable: true),
                Duration = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Runes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "MonsterDrops",
            columns: table => new
            {
                Element = table.Column<int>(type: "int", nullable: false),
                ThreatLevel = table.Column<int>(type: "int", nullable: false),
                IngredientId = table.Column<int>(type: "int", nullable: false),
                DropRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_MonsterDrops", x => new { x.Element, x.ThreatLevel, x.IngredientId });
                table.ForeignKey(
                    name: "FK_MonsterDrops_Ingredients_IngredientId",
                    column: x => x.IngredientId,
                    principalTable: "Ingredients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Players",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Level = table.Column<int>(type: "int", nullable: false),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                MaxHealth = table.Column<int>(type: "int", nullable: false),
                ArchetypeId = table.Column<int>(type: "int", nullable: false),
                RoomId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Players", x => x.Id);
                table.ForeignKey(
                    name: "FK_Players_Archetypes_ArchetypeId",
                    column: x => x.ArchetypeId,
                    principalTable: "Archetypes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Players_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Recipes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RuneId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recipes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Recipes_Runes_RuneId",
                    column: x => x.RuneId,
                    principalTable: "Runes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Inventories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Gold = table.Column<int>(type: "int", nullable: false),
                Capacity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                PlayerId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Inventories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Inventories_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RecipeIngredients",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RecipeId = table.Column<int>(type: "int", nullable: false),
                IngredientId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                table.ForeignKey(
                    name: "FK_RecipeIngredients_Ingredients_IngredientId",
                    column: x => x.IngredientId,
                    principalTable: "Ingredients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_RecipeIngredients_Recipes_RecipeId",
                    column: x => x.RecipeId,
                    principalTable: "Recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Items",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Durability = table.Column<int>(type: "int", nullable: false),
                Weight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                RequiredLevel = table.Column<int>(type: "int", nullable: false),
                ItemType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                InventoryId = table.Column<int>(type: "int", nullable: true),
                MonsterId = table.Column<int>(type: "int", nullable: true),
                DefensePower = table.Column<int>(type: "int", nullable: true),
                Resistance = table.Column<int>(type: "int", nullable: true),
                ArmorType = table.Column<int>(type: "int", nullable: true),
                Armor_RuneId = table.Column<int>(type: "int", nullable: true),
                Power = table.Column<int>(type: "int", nullable: true),
                ConsumableType = table.Column<int>(type: "int", nullable: true),
                AttackPower = table.Column<int>(type: "int", nullable: true),
                DamageType = table.Column<int>(type: "int", nullable: true),
                Weapon_RuneId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Items", x => x.Id);
                table.ForeignKey(
                    name: "FK_Items_Inventories_InventoryId",
                    column: x => x.InventoryId,
                    principalTable: "Inventories",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Items_Runes_Armor_RuneId",
                    column: x => x.Armor_RuneId,
                    principalTable: "Runes",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Items_Runes_Weapon_RuneId",
                    column: x => x.Weapon_RuneId,
                    principalTable: "Runes",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "Monsters",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Level = table.Column<int>(type: "int", nullable: false),
                MaxHealth = table.Column<int>(type: "int", nullable: false),
                ThreatLevel = table.Column<int>(type: "int", nullable: false),
                AggressionLevel = table.Column<int>(type: "int", nullable: false),
                MonsterType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RoomId = table.Column<int>(type: "int", nullable: true),
                ItemId = table.Column<int>(type: "int", nullable: true),
                DefensePower = table.Column<int>(type: "int", nullable: false),
                AttackPower = table.Column<int>(type: "int", nullable: false),
                DamageType = table.Column<int>(type: "int", nullable: false),
                Resistance = table.Column<int>(type: "int", nullable: false),
                AttackElement = table.Column<int>(type: "int", nullable: true),
                ElementalPower = table.Column<int>(type: "int", nullable: true),
                Vulnerability = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Monsters", x => x.Id);
                table.ForeignKey(
                    name: "FK_Monsters_Items_ItemId",
                    column: x => x.ItemId,
                    principalTable: "Items",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_Monsters_Rooms_RoomId",
                    column: x => x.RoomId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "Skills",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RequiredLevel = table.Column<int>(type: "int", nullable: false),
                Cost = table.Column<int>(type: "int", nullable: false),
                Power = table.Column<int>(type: "int", nullable: false),
                Cooldown = table.Column<int>(type: "int", nullable: false),
                TargetType = table.Column<int>(type: "int", nullable: false),
                SkillType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                SkillCategory = table.Column<int>(type: "int", nullable: false),
                ArchetypeId = table.Column<int>(type: "int", nullable: true),
                MonsterId = table.Column<int>(type: "int", nullable: true),
                DamageType = table.Column<int>(type: "int", nullable: true),
                Duration = table.Column<int>(type: "int", nullable: true),
                StatAffected = table.Column<int>(type: "int", nullable: true),
                SupportEffect = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Skills", x => x.Id);
                table.ForeignKey(
                    name: "FK_Skills_Archetypes_ArchetypeId",
                    column: x => x.ArchetypeId,
                    principalTable: "Archetypes",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Skills_Monsters_MonsterId",
                    column: x => x.MonsterId,
                    principalTable: "Monsters",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Inventories_PlayerId",
            table: "Inventories",
            column: "PlayerId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Items_Armor_RuneId",
            table: "Items",
            column: "Armor_RuneId");

        migrationBuilder.CreateIndex(
            name: "IX_Items_InventoryId",
            table: "Items",
            column: "InventoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Items_Weapon_RuneId",
            table: "Items",
            column: "Weapon_RuneId");

        migrationBuilder.CreateIndex(
            name: "IX_MonsterDrops_IngredientId",
            table: "MonsterDrops",
            column: "IngredientId");

        migrationBuilder.CreateIndex(
            name: "IX_Monsters_ItemId",
            table: "Monsters",
            column: "ItemId",
            unique: true,
            filter: "[ItemId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Monsters_RoomId",
            table: "Monsters",
            column: "RoomId");

        migrationBuilder.CreateIndex(
            name: "IX_Players_ArchetypeId",
            table: "Players",
            column: "ArchetypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Players_RoomId",
            table: "Players",
            column: "RoomId");

        migrationBuilder.CreateIndex(
            name: "IX_RecipeIngredients_IngredientId",
            table: "RecipeIngredients",
            column: "IngredientId");

        migrationBuilder.CreateIndex(
            name: "IX_RecipeIngredients_RecipeId",
            table: "RecipeIngredients",
            column: "RecipeId");

        migrationBuilder.CreateIndex(
            name: "IX_Recipes_RuneId",
            table: "Recipes",
            column: "RuneId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_EastId",
            table: "Rooms",
            column: "EastId");

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_NorthId",
            table: "Rooms",
            column: "NorthId");

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_SouthId",
            table: "Rooms",
            column: "SouthId");

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_WestId",
            table: "Rooms",
            column: "WestId");

        migrationBuilder.CreateIndex(
            name: "IX_Skills_ArchetypeId",
            table: "Skills",
            column: "ArchetypeId");

        migrationBuilder.CreateIndex(
            name: "IX_Skills_MonsterId",
            table: "Skills",
            column: "MonsterId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "MonsterDrops");

        migrationBuilder.DropTable(
            name: "RecipeIngredients");

        migrationBuilder.DropTable(
            name: "Skills");

        migrationBuilder.DropTable(
            name: "Ingredients");

        migrationBuilder.DropTable(
            name: "Recipes");

        migrationBuilder.DropTable(
            name: "Monsters");

        migrationBuilder.DropTable(
            name: "Items");

        migrationBuilder.DropTable(
            name: "Inventories");

        migrationBuilder.DropTable(
            name: "Runes");

        migrationBuilder.DropTable(
            name: "Players");

        migrationBuilder.DropTable(
            name: "Archetypes");

        migrationBuilder.DropTable(
            name: "Rooms");
    }
}
