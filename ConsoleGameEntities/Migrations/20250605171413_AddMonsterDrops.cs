using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class AddMonsterDrops : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.PrimaryKey("PK_DropsByElementThreat", x => new { x.Element, x.ThreatLevel, x.IngredientId });

                    table.ForeignKey(
                        name: "FK_DropsByElementThreat_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MonsterDrops_IngredientId",
                table: "MonsterDrops",
                column: "IngredientId");

            //add new monster variables
            migrationBuilder.AddColumn<int>(
                name: "AttackElement",
                table: "Monsters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ElementalPower",
                table: "Monsters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Vulnerability",
                table: "Monsters",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonsterDrops");
        }
    }
}
