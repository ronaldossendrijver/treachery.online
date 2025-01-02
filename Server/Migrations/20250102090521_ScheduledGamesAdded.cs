using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations
{
    /// <inheritdoc />
    public partial class ScheduledGamesAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameName",
                table: "ArchivedGames",
                type: "TEXT",
                maxLength: 128,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ScheduledGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GameId = table.Column<string>(type: "TEXT", maxLength: 36, nullable: true),
                    CreatorUserId = table.Column<int>(type: "INTEGER", nullable: false),
                    DateTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    NumberOfPlayers = table.Column<int>(type: "INTEGER", nullable: true),
                    MaximumTurns = table.Column<int>(type: "INTEGER", nullable: true),
                    Ruleset = table.Column<int>(type: "INTEGER", nullable: true),
                    AsyncPlay = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowedFactionsInPlay = table.Column<string>(type: "TEXT", nullable: true),
                    SubscribedUsers = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledGames", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledGames");

            migrationBuilder.DropColumn(
                name: "GameName",
                table: "ArchivedGames");
        }
    }
}
