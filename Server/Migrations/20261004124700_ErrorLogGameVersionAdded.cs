using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations;

[DbContext(typeof(TreacheryContext))]
[Migration("20261004124700_ErrorLogGameVersionAdded")]
public partial class ErrorLogGameVersionAdded : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "GameVersion",
            table: "ErrorLogs",
            type: "INTEGER",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GameVersion",
            table: "ErrorLogs");
    }
}
