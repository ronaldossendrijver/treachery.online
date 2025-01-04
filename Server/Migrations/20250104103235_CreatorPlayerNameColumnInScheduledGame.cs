using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations
{
    /// <inheritdoc />
    public partial class CreatorPlayerNameColumnInScheduledGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatorPlayerName",
                table: "ScheduledGames",
                type: "TEXT",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatorPlayerName",
                table: "ScheduledGames");
        }
    }
}
