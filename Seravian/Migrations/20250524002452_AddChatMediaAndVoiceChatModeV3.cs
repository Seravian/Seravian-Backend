using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMediaAndVoiceChatModeV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CombinedAnalysisResult",
                table: "ChatVoiceAnalyses",
                newName: "LLMFormattedInputFromCombinedAnalysisResult");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LLMFormattedInputFromCombinedAnalysisResult",
                table: "ChatVoiceAnalyses",
                newName: "CombinedAnalysisResult");
        }
    }
}
