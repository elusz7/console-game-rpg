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
                { 1, "Entrance", "This is the room where your adventure begins.", 1, 2, null, null, 4 },
                { 2, "Hallway", "A long hallway with doors on either side.", null, null, 1, 3, null },
                { 3, "Treasure Room", "A room filled with treasure and monsters.", null, null, null, null, 2 },
                { 4, "Library", "A quiet library filled with ancient books.", null, null, null, 1, null }
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
