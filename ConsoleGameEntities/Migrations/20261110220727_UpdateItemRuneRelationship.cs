using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateItemRuneRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquippedRunes_Items_ItemId",
                table: "EquippedRunes");

            migrationBuilder.DropIndex(
                name: "IX_EquippedRunes_ItemId",
                table: "EquippedRunes");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "EquippedRunes",
                newName: "WeaponId");

            migrationBuilder.AddColumn<int>(
                name: "ArmorId",
                table: "EquippedRunes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EquippedRunes_ArmorId",
                table: "EquippedRunes",
                column: "ArmorId",
                unique: true,
                filter: "[ArmorId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EquippedRunes_WeaponId",
                table: "EquippedRunes",
                column: "WeaponId",
                unique: true,
                filter: "[WeaponId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_EquippedRune_OneOwner",
                table: "EquippedRunes",
                sql: "(ArmorId IS NOT NULL AND WeaponId IS NULL) OR (ArmorId IS NULL AND WeaponId IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_EquippedRunes_Items_ArmorId",
                table: "EquippedRunes",
                column: "ArmorId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_EquippedRunes_Items_WeaponId",
                table: "EquippedRunes",
                column: "WeaponId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EquippedRunes_Items_ArmorId",
                table: "EquippedRunes");

            migrationBuilder.DropForeignKey(
                name: "FK_EquippedRunes_Items_WeaponId",
                table: "EquippedRunes");

            migrationBuilder.DropIndex(
                name: "IX_EquippedRunes_ArmorId",
                table: "EquippedRunes");

            migrationBuilder.DropIndex(
                name: "IX_EquippedRunes_WeaponId",
                table: "EquippedRunes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_EquippedRune_OneOwner",
                table: "EquippedRunes");

            migrationBuilder.DropColumn(
                name: "ArmorId",
                table: "EquippedRunes");

            migrationBuilder.RenameColumn(
                name: "WeaponId",
                table: "EquippedRunes",
                newName: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_EquippedRunes_ItemId",
                table: "EquippedRunes",
                column: "ItemId",
                unique: true,
                filter: "[ItemId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_EquippedRunes_Items_ItemId",
                table: "EquippedRunes",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
