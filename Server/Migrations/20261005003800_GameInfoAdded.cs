using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations;

[DbContext(typeof(TreacheryContext))]
[Migration("20261005003800_GameInfoAdded")]
public partial class GameInfoAdded : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "GameInfo",
            table: "PersistedGames",
            type: "TEXT",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "GameInfo",
            table: "PersistedGames");
    }
}
