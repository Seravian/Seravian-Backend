using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class v6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeUtc",
                table: "ChatsMessages",
                newName: "TimestampUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Chats",
                newName: "CreatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimestampUtc",
                table: "ChatsMessages",
                newName: "TimeUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Chats",
                newName: "CreatedAt");
        }
    }
}
