using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class AddRooms : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "RoomId",
            table: "Monsters",
            type: "int",
            nullable: true);

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PlayerId = table.Column<int>(type: "int", nullable: true),
                NorthId = table.Column<int>(type: "int", nullable: true),
                SouthId = table.Column<int>(type: "int", nullable: true),
                WestId = table.Column<int>(type: "int", nullable: true),
                EastId = table.Column<int>(type: "int", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
                table.ForeignKey(
                    name: "FK_Rooms_Players_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Players",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_EastId",
                    column: x => x.EastId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_NorthId",
                    column: x => x.NorthId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_SouthId",
                    column: x => x.SouthId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
                table.ForeignKey(
                    name: "FK_Rooms_Rooms_WestId",
                    column: x => x.WestId,
                    principalTable: "Rooms",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Monsters_RoomId",
            table: "Monsters",
            column: "RoomId");

        migrationBuilder.CreateIndex(
            name: "IX_Rooms_PlayerId",
            table: "Rooms",
            column: "PlayerId");

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

        migrationBuilder.AddForeignKey(
            name: "FK_Monsters_Rooms_RoomId",
            table: "Monsters",
            column: "RoomId",
            principalTable: "Rooms",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Monsters_Rooms_RoomId",
            table: "Monsters");

        migrationBuilder.DropForeignKey(
            name: "FK_Players_Rooms_RoomId",
            table: "Players");

        migrationBuilder.DropTable(
            name: "Rooms");

        migrationBuilder.DropIndex(
            name: "IX_Monsters_RoomId",
            table: "Monsters");

        migrationBuilder.DropIndex(
            name: "IX_Players_RoomId",
            table: "Players");

        migrationBuilder.DropColumn(
            name: "RoomId",
            table: "Monsters");
    }
}
