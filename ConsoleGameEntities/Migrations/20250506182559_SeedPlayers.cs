using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedPlayers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Players;");
            migrationBuilder.Sql("DELETE FROM Inventories;");

            migrationBuilder.Sql("SET IDENTITY_INSERT Players ON;");
            migrationBuilder.InsertData(
            table: "Players",
            columns: new[]
            {
                "Id", "Name", "ArchetypeId", "Experience", "Level", "MaxHealth"
            },
            values: new object[,]
            {
                { 1, "Aravien Sunblade", 1, 1200, 3, 85 },
                { 2, "Nadira Valebright", 2, 3400, 5, 130 },
                { 3, "Kaenji Stormleaf", 3, 7200, 7, 95 },
                { 4, "Lusia Moonwhisper", 4, 150, 1, 60 },
                { 5, "Thabros Ironstride", 5, 5800, 6, 110 },
                { 6, "Dymitros Frostvein", 1, 9200, 8, 140 },
                { 7, "Meilin Starbloom", 2, 450, 2, 70 },
                { 8, "Karlos Flameborn", 3, 10100, 9, 125 },
                { 9, "A'minah Windgrace", 4, 2100, 4, 80 },
                { 10, "Bjornar Stoneheart", 5, 10600, 10, 150 },
                { 11, "Soraiya Duskrender", 1, 3000, 4, 90 },
                { 12, "Jalen of the Hollow Sky", 2, 5600, 6, 115 },
                { 13, "Yutono Spiritblade", 3, 250, 1, 55 },
                { 14, "Anikaa Dawnsinger", 4, 8000, 7, 100 },
                { 15, "Kwamar Tideborn", 5, 6200, 6, 135 },
                { 16, "Rykos Emberquill", 1, 4900, 5, 105 },
                { 17, "Helenya Greenseer", 2, 1600, 3, 78 },
                { 18, "Omari Voidwalker", 3, 9900, 9, 145 },
                { 19, "Zaniel Silvertide", 4, 3800, 5, 92 },
                { 20, "Jovarien Flamevein", 5, 7100, 7, 125 }
            });
            migrationBuilder.Sql("SET IDENTITY_INSERT Players OFF;");

            migrationBuilder.Sql("SET IDENTITY_INSERT Inventories ON;");
            migrationBuilder.InsertData(
            table: "Inventories",
            columns: new[]
            {
                "Id", "PlayerId", "Gold", "Capacity"
            },
            values: new object[,]
            {
                { 1, 1, 120, 30.0m },        
                { 2, 2, 350, 50.0m },        
                { 3, 3, 780, 40.0m },        
                { 4, 4, 25, 20.0m },         
                { 5, 5, 540, 45.0m },        
                { 6, 6, 980, 55.0m },       
                { 7, 7, 60, 25.0m },        
                { 8, 8, 1020, 50.0m },       
                { 9, 9, 200, 35.0m },       
                { 10, 10, 1500, 60.0m },    
                { 11, 11, 280, 32.0m },      
                { 12, 12, 600, 42.0m },     
                { 13, 13, 40, 20.0m },     
                { 14, 14, 820, 45.0m },    
                { 15, 15, 610, 50.0m },      
                { 16, 16, 470, 40.0m },     
                { 17, 17, 180, 28.0m },      
                { 18, 18, 990, 55.0m },       
                { 19, 19, 360, 38.0m },     
                { 20, 20, 700, 50.0m }       
            });
            migrationBuilder.Sql("SET IDENTITY_INSERT Inventories OFF;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
