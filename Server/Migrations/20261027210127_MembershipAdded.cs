using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Treachery.Server.Migrations
{
    /// <inheritdoc />
    public partial class MembershipAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MembershipValidUntil",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Users",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MembershipValidUntil",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Users");
        }
    }
}
