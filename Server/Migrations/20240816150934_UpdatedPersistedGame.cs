using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPersistedGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Token",
                table: "PersistedGames");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAsyncPlayMessageSent",
                table: "PersistedGames",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAsyncPlayMessageSent",
                table: "PersistedGames");

            migrationBuilder.AddColumn<string>(
                name: "Token",
                table: "PersistedGames",
                type: "TEXT",
                maxLength: 32,
                nullable: true);
        }
    }
}
