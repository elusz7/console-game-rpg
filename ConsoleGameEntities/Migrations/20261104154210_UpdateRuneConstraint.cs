using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRuneConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquippedRunes_InventoryId_RuneId_Tier",
                table: "EquippedRunes");

            migrationBuilder.CreateIndex(
                name: "IX_EquippedRunes_InventoryId_RuneId_Tier",
                table: "EquippedRunes",
                columns: new[] { "InventoryId", "RuneId", "Tier" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EquippedRunes_InventoryId_RuneId_Tier",
                table: "EquippedRunes");

            migrationBuilder.CreateIndex(
                name: "IX_EquippedRunes_InventoryId_RuneId_Tier",
                table: "EquippedRunes",
                columns: new[] { "InventoryId", "RuneId", "Tier" });
        }
    }
}
