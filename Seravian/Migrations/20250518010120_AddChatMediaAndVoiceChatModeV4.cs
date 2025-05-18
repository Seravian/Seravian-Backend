using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMediaAndVoiceChatModeV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessageMedia_ChatsMessages_MessageId",
                table: "ChatMessageMedia");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatVoiceAnalysis_ChatsMessages_MessageId",
                table: "ChatVoiceAnalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatVoiceAnalysis",
                table: "ChatVoiceAnalysis");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessageMedia",
                table: "ChatMessageMedia");

            migrationBuilder.RenameTable(
                name: "ChatVoiceAnalysis",
                newName: "ChatVoiceAnalyses");

            migrationBuilder.RenameTable(
                name: "ChatMessageMedia",
                newName: "ChatsMessagesMedias");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatVoiceAnalyses",
                table: "ChatVoiceAnalyses",
                column: "MessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatsMessagesMedias",
                table: "ChatsMessagesMedias",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatsMessagesMedias_ChatsMessages_MessageId",
                table: "ChatsMessagesMedias",
                column: "MessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatVoiceAnalyses_ChatsMessages_MessageId",
                table: "ChatVoiceAnalyses",
                column: "MessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatsMessagesMedias_ChatsMessages_MessageId",
                table: "ChatsMessagesMedias");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatVoiceAnalyses_ChatsMessages_MessageId",
                table: "ChatVoiceAnalyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatVoiceAnalyses",
                table: "ChatVoiceAnalyses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatsMessagesMedias",
                table: "ChatsMessagesMedias");

            migrationBuilder.RenameTable(
                name: "ChatVoiceAnalyses",
                newName: "ChatVoiceAnalysis");

            migrationBuilder.RenameTable(
                name: "ChatsMessagesMedias",
                newName: "ChatMessageMedia");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatVoiceAnalysis",
                table: "ChatVoiceAnalysis",
                column: "MessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessageMedia",
                table: "ChatMessageMedia",
                column: "MessageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessageMedia_ChatsMessages_MessageId",
                table: "ChatMessageMedia",
                column: "MessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatVoiceAnalysis_ChatsMessages_MessageId",
                table: "ChatVoiceAnalysis",
                column: "MessageId",
                principalTable: "ChatsMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
