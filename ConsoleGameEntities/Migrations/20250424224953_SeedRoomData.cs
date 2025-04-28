using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedRoomData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("SET IDENTITY_INSERT Rooms ON;");
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "Id", "Name", "Description", "PlayerId", "NorthId", "SouthId", "EastId", "WestId" },
                values: new object[,]
                {
                    { 1, "Entrance", "This is the room where your adventure begins.", null, 2, null, null, null },
                    { 2, "Hallway", "A long hallway with doors on either side.", null, 5, 1, 3, 11 },
                    { 3, "Library", "A quiet library filled with ancient books.", null, 6, null, 4, 2 },
                    { 4, "Study", "A small study with a flickering candle.", null, 7, null, null, 3 },
                    { 5, "Gallery", "An art gallery displaying ancient artifacts.", null, 8, 2, null, null },
                    { 6, "Treasure Room", "A room filled with treasure and monsters.", null, 9, 3, 7, null },
                    { 7, "Secret Room", "A hidden room with strange inscriptions.", null, 10, 4, null, 6 },
                    { 8, "Observatory", "A domed room with a massive telescope.", null, null, 5, 9, null },
                    { 9, "Armory", "An old armory filled with dusty weapons.", null, null, 6, 10, 8 },
                    { 10, "Dungeon", "A dark and damp dungeon with rusty cells.", null, null, 7, null, 9 },
                    { 11, "Garden", "An overgrown garden with a broken fountain.", null, 12, null, 2, null },
                    { 12, "Shrine", "A shrine with offerings to forgotten gods.", null, 13, 11, null, null },
                    { 13, "Crypt", "A chilling crypt lined with stone coffins.", null, 14, 12, null, null },
                    { 14, "Underground River", "A rushing river flowing underground.", null, null, 13, null, null }
                });
            migrationBuilder.Sql("SET IDENTITY_INSERT Rooms OFF;");

            migrationBuilder.Sql("UPDATE Monsters SET RoomId = 3 WHERE Id = 1;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Rooms WHERE Id IN (1, 2, 3, 4);");
            migrationBuilder.Sql("UPDATE Monsters SET RoomId = NULL WHERE Id = 1;");
        }
    }
}
