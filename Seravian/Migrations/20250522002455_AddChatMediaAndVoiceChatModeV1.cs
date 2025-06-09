using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMediaAndVoiceChatModeV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CombinedAnalysisResult",
                table: "ChatVoiceAnalyses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CombinedAnalysisResult",
                table: "ChatVoiceAnalyses",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
