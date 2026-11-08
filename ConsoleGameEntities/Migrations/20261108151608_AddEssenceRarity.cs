using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class AddEssenceRarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EssenceRarity",
                table: "Ingredients",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE Ingredients
                SET EssenceRarity = CASE Id
                    WHEN 1 THEN 0 -- Common
                    WHEN 2 THEN 1 -- Uncommon
                    WHEN 3 THEN 2 -- Rare
                    WHEN 4 THEN 3 -- Epic
                    WHEN 5 THEN 4 -- Legendary
                    WHEN 6 THEN 5 -- Mythic
                END
                WHERE Id BETWEEN 1 AND 6
                  AND ComponentType = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EssenceRarity",
                table: "Ingredients");
        }
    }
}