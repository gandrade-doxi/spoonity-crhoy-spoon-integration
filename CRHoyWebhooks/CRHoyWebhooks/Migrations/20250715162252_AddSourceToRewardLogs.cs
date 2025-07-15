using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRHoyWebhooks.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceToRewardLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "RewardLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Source",
                table: "RewardLogs");
        }
    }
}
