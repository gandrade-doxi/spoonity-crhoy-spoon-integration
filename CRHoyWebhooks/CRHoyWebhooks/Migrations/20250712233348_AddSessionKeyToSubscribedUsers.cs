using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRHoyWebhooks.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionKeyToSubscribedUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionKey",
                table: "SubscribedUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionKey",
                table: "SubscribedUsers");
        }
    }
}
