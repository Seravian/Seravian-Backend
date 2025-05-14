using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMediaAndVoiceChatMode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMessageMedia",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    MediaType = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileSizeInBytes = table.Column<long>(type: "bigint", nullable: true),
                    Transcription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SEREmotionAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FaceAnalysis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CombinedAnalysisResult = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageMedia", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessageMedia_ChatsMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatsMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatVoiceAnalysis",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false),
                    Transcription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SEREmotionAnalysis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatVoiceAnalysis", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatVoiceAnalysis_ChatsMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatsMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessageMedia");

            migrationBuilder.DropTable(
                name: "ChatVoiceAnalysis");
        }
    }
}
