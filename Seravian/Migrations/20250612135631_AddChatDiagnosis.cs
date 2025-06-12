using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatDiagnosis",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", maxLength: 10000, nullable: true),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartMessageId = table.Column<long>(type: "bigint", nullable: false),
                    ToMessageId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatDiagnosis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatDiagnosis_ChatsMessages_StartMessageId",
                        column: x => x.StartMessageId,
                        principalTable: "ChatsMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatDiagnosis_ChatsMessages_ToMessageId",
                        column: x => x.ToMessageId,
                        principalTable: "ChatsMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatDiagnosis_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatDiagnosis_ChatId",
                table: "ChatDiagnosis",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatDiagnosis_StartMessageId",
                table: "ChatDiagnosis",
                column: "StartMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatDiagnosis_ToMessageId",
                table: "ChatDiagnosis",
                column: "ToMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatDiagnosis");
        }
    }
}
