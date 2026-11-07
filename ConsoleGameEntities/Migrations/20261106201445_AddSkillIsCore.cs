using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillIsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCore",
                table: "Skills",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql(
                "UPDATE Skills SET IsCore = 1 WHERE Id <= 212;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCore",
                table: "Skills");
        }
    }
}
