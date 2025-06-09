using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class AddRecipes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Ingredients table
        migrationBuilder.CreateTable(
            name: "Ingredients",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IngredientType = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Ingredients", x => x.Id);
            });

        // Recipes table
        migrationBuilder.CreateTable(
            name: "Recipes",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                RuneId = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recipes", x => x.Id);
                table.ForeignKey(
                    name: "FK_Recipes_Runes_RuneId",
                    column: x => x.RuneId,
                    principalTable: "Runes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // RecipeIngredients join table
        migrationBuilder.CreateTable(
            name: "RecipeIngredients",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                RecipeId = table.Column<int>(type: "int", nullable: false),
                IngredientId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                table.ForeignKey(
                    name: "FK_RecipeIngredients_Recipes_RecipeId",
                    column: x => x.RecipeId,
                    principalTable: "Recipes",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_RecipeIngredients_Ingredients_IngredientId",
                    column: x => x.IngredientId,
                    principalTable: "Ingredients",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        // add recipe id to Runes table
        migrationBuilder.AddColumn<int>(
            name: "RecipeId",
            table: "Runes",
            type: "int",
            nullable: true);

        // Indexes
        migrationBuilder.CreateIndex(
            name: "IX_Recipes_RuneId",
            table: "Recipes",
            column: "RuneId");

        migrationBuilder.CreateIndex(
            name: "IX_RecipeIngredients_RecipeId",
            table: "RecipeIngredients",
            column: "RecipeId");

        migrationBuilder.CreateIndex(
            name: "IX_RecipeIngredients_IngredientId",
            table: "RecipeIngredients",
            column: "IngredientId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RecipeIngredients");

        migrationBuilder.DropTable(
            name: "Recipes");

        migrationBuilder.DropTable(
            name: "Ingredients");
    }

}
