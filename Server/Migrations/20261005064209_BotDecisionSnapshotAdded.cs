using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations
{
    /// <inheritdoc />
    public partial class BotDecisionSnapshotAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GameId",
                table: "ErrorLogs",
                type: "TEXT",
                maxLength: 36,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ErrorLogSnapshots",
                columns: table => new
                {
                    ErrorLogEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    GameState = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ErrorLogSnapshots", x => x.ErrorLogEntryId);
                    table.ForeignKey(
                        name: "FK_ErrorLogSnapshots_ErrorLogs_ErrorLogEntryId",
                        column: x => x.ErrorLogEntryId,
                        principalTable: "ErrorLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ErrorLogSnapshots");

            migrationBuilder.DropColumn(
                name: "GameId",
                table: "ErrorLogs");
        }
    }
}
