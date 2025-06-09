using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorIdentityVerificaitonV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "VerificationDate",
                table: "Doctors",
                newName: "VerifiedAtUtc");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Doctors",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Title",
                table: "Doctors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "VerificationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationRequests_Admins_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "Admins",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_VerificationRequests_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VerificationRequestsAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorVerificationRequestId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VerificationRequestsAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VerificationRequestsAttachments_VerificationRequests_DoctorVerificationRequestId",
                        column: x => x.DoctorVerificationRequestId,
                        principalTable: "VerificationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_DoctorId",
                table: "VerificationRequests",
                column: "DoctorId",
                unique: true,
                filter: "[Status] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequests_ReviewerId",
                table: "VerificationRequests",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_VerificationRequestsAttachments_DoctorVerificationRequestId",
                table: "VerificationRequestsAttachments",
                column: "DoctorVerificationRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VerificationRequestsAttachments");

            migrationBuilder.DropTable(
                name: "VerificationRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Doctors");

            migrationBuilder.RenameColumn(
                name: "VerifiedAtUtc",
                table: "Doctors",
                newName: "VerificationDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Doctors",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
