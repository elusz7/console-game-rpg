using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    public partial class DropSkillCheckConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Skill_OnlyOneOwner",
                table: "Skills");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
