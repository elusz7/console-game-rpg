using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleGameEntities.Migrations;

public partial class RoomSeedData : BaseMigration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        RunSql(migrationBuilder);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        RunSqlRollback(migrationBuilder);
    }
}
