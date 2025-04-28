using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class RemovePlayerRoomAbilities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Players_PlayerId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "PlayerAbilities");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_PlayerId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "PlayerId",
                table: "Rooms");            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerId",
                table: "Rooms",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerAbilities",
                columns: table => new
                {
                    AbilitiesId = table.Column<int>(type: "int", nullable: false),
                    PlayersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAbilities", x => new { x.AbilitiesId, x.PlayersId });
                    table.ForeignKey(
                        name: "FK_PlayerAbilities_Abilities_AbilitiesId",
                        column: x => x.AbilitiesId,
                        principalTable: "Abilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAbilities_Players_PlayersId",
                        column: x => x.PlayersId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_PlayerId",
                table: "Rooms",
                column: "PlayerId",
                unique: true,
                filter: "[PlayerId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAbilities_PlayersId",
                table: "PlayerAbilities",
                column: "PlayersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Players_PlayerId",
                table: "Rooms",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id");
        }
    }
}
