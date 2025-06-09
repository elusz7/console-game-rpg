using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class AddRunes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Weapon_RuneId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Armor_RuneId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Runes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Duration = table.Column<int>(type: "int", nullable: true),
                    RuneType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Power = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Element = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ArmorRuneId",
                table: "Items",
                column: "Armor_RuneId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_WeaponRuneId",
                table: "Items",
                column: "Weapon_RuneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Runes_ArmorRuneId",
                table: "Items",
                column: "Armor_RuneId",
                principalTable: "Runes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Runes_WeaponRuneId",
                table: "Items",
                column: "Weapon_RuneId",
                principalTable: "Runes",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Rune_ArmorRuneId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Rune_WeaponRuneId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "Runes");

            migrationBuilder.DropIndex(
                name: "IX_Items_WeaponRuneId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ArmorRuneId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Armor_RuneId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Weapon_RuneId",
                table: "Items");
        }
    }
}
