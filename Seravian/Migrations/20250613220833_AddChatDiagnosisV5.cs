using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddChatDiagnosisV5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ChatDiagnoses");

            migrationBuilder.AddColumn<string>(
                name: "DiagnosedProblem",
                table: "ChatDiagnoses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FailureReason",
                table: "ChatDiagnoses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reasoning",
                table: "ChatDiagnoses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatDiagnosisPrescription",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatDiagnosisId = table.Column<long>(type: "bigint", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatDiagnosisPrescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatDiagnosisPrescription_ChatDiagnoses_ChatDiagnosisId",
                        column: x => x.ChatDiagnosisId,
                        principalTable: "ChatDiagnoses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatDiagnosisPrescription_ChatDiagnosisId",
                table: "ChatDiagnosisPrescription",
                column: "ChatDiagnosisId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatDiagnosisPrescription");

            migrationBuilder.DropColumn(
                name: "DiagnosedProblem",
                table: "ChatDiagnoses");

            migrationBuilder.DropColumn(
                name: "FailureReason",
                table: "ChatDiagnoses");

            migrationBuilder.DropColumn(
                name: "Reasoning",
                table: "ChatDiagnoses");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ChatDiagnoses",
                type: "nvarchar(max)",
                maxLength: 10000,
                nullable: true);
        }
    }
}
