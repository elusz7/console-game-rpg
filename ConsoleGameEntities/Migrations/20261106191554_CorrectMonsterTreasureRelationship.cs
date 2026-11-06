using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class CorrectMonsterTreasureRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 UPDATE Items
                                 SET MonsterId = Monsters.Id
                                 FROM Items
                                 INNER JOIN Monsters ON Monsters.ItemId = Items.Id
                                 WHERE Items.MonsterId IS NULL;
                                 """);
            
            migrationBuilder.DropForeignKey(
                name: "FK_Monsters_Items_ItemId",
                table: "Monsters");

            migrationBuilder.DropIndex(
                name: "IX_Monsters_ItemId",
                table: "Monsters");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "Monsters");

            migrationBuilder.CreateIndex(
                name: "IX_Items_MonsterId",
                table: "Items",
                column: "MonsterId",
                unique: true,
                filter: "[MonsterId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Monsters_MonsterId",
                table: "Items",
                column: "MonsterId",
                principalTable: "Monsters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Monsters_MonsterId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_MonsterId",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "Monsters",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Monsters_ItemId",
                table: "Monsters",
                column: "ItemId",
                unique: true,
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Monsters_Items_ItemId",
                table: "Monsters",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
