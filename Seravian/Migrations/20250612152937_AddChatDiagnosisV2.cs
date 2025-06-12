using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatDiagnosisV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnosis_ChatsMessages_StartMessageId",
                table: "ChatDiagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnosis_ChatsMessages_ToMessageId",
                table: "ChatDiagnosis");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnosis_Chats_ChatId",
                table: "ChatDiagnosis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatDiagnosis",
                table: "ChatDiagnosis");

            migrationBuilder.RenameTable(
                name: "ChatDiagnosis",
                newName: "ChatDiagnoses");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnosis_ToMessageId",
                table: "ChatDiagnoses",
                newName: "IX_ChatDiagnoses_ToMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnosis_StartMessageId",
                table: "ChatDiagnoses",
                newName: "IX_ChatDiagnoses_StartMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnosis_ChatId",
                table: "ChatDiagnoses",
                newName: "IX_ChatDiagnoses_ChatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatDiagnoses",
                table: "ChatDiagnoses",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnoses_ChatsMessages_StartMessageId",
                table: "ChatDiagnoses",
                column: "StartMessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnoses_ChatsMessages_ToMessageId",
                table: "ChatDiagnoses",
                column: "ToMessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnoses_Chats_ChatId",
                table: "ChatDiagnoses",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnoses_ChatsMessages_StartMessageId",
                table: "ChatDiagnoses");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnoses_ChatsMessages_ToMessageId",
                table: "ChatDiagnoses");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatDiagnoses_Chats_ChatId",
                table: "ChatDiagnoses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatDiagnoses",
                table: "ChatDiagnoses");

            migrationBuilder.RenameTable(
                name: "ChatDiagnoses",
                newName: "ChatDiagnosis");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnoses_ToMessageId",
                table: "ChatDiagnosis",
                newName: "IX_ChatDiagnosis_ToMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnoses_StartMessageId",
                table: "ChatDiagnosis",
                newName: "IX_ChatDiagnosis_StartMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_ChatDiagnoses_ChatId",
                table: "ChatDiagnosis",
                newName: "IX_ChatDiagnosis_ChatId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatDiagnosis",
                table: "ChatDiagnosis",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnosis_ChatsMessages_StartMessageId",
                table: "ChatDiagnosis",
                column: "StartMessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnosis_ChatsMessages_ToMessageId",
                table: "ChatDiagnosis",
                column: "ToMessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatDiagnosis_Chats_ChatId",
                table: "ChatDiagnosis",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
