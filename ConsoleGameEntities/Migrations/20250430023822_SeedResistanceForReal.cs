using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class SeedResistanceForReal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            List<decimal> multipliers = new List<decimal> { 0.3M, 0.2M, 0.4M, 0.6M, 0.5M };
            List<int> bonus = new List<int> { 2, 1, 3, 3, 2 };

            for (int i = 0; i < multipliers.Count; i++)
            {
                migrationBuilder.Sql($@"
                    UPDATE Archetypes
                    SET ResistanceMultiplier = {multipliers[i]}, ResistanceBonus = {bonus[i]}
                    WHERE Id = {i + 1};");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int i = 1; i <= 5; i++)
            {
                migrationBuilder.Sql($@"
                    UPDATE Archetypes
                    SET ResistanceMultiplier = 0, ResistanceBonus = 0
                    WHERE Id = {i};");
            }
        }
    }
}
