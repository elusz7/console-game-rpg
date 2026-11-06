using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations
{
    /// <inheritdoc />
    public partial class AddItemIsCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCore",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.Sql(
                "UPDATE Items SET IsCore = 1 WHERE Id <= 178;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCore",
                table: "Items");
        }
    }
}
