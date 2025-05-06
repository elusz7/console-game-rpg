using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class UpdateItems : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ItemId",
            table: "Monsters",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ArmorType",
            table: "Items",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "DamageType",
            table: "Items",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "Power",
            table: "Items",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "ConsumableType",
            table: "Items",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "MonsterId",
            table: "Items",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RequiredLevel",
            table: "Items",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateIndex(
            name: "IX_Monsters_ItemId",
            table: "Monsters",
            column: "ItemId");

        migrationBuilder.AddForeignKey(
            name: "FK_Monsters_Items_ItemId",
            table: "Monsters",
            column: "ItemId",
            principalTable: "Items",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Monsters_Items_ItemId",
            table: "Monsters");

        migrationBuilder.DropIndex(
            name: "IX_Monsters_ItemId",
            table: "Monsters");

        migrationBuilder.DropColumn(
            name: "ItemId",
            table: "Monsters");

        migrationBuilder.DropColumn(
            name: "ArmorType",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "DamageType",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "MonsterId",
            table: "Items");

        migrationBuilder.DropColumn(
            name: "RequiredLevel",
            table: "Items");
    }
}
