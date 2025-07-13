using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRHoyWebhooks.Migrations
{
    /// <inheritdoc />
    public partial class AddLastRewardedAtToSubscribedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastRewardedAt",
                table: "SubscribedUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastRewardedAt",
                table: "SubscribedUsers");
        }
    }
}
