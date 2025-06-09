using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorIdentityVerificaitonV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VerificationRequests_Admins_ReviewerId",
                table: "VerificationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationRequests_Doctors_DoctorId",
                table: "VerificationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_VerificationRequestsAttachments_VerificationRequests_DoctorVerificationRequestId",
                table: "VerificationRequestsAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VerificationRequestsAttachments",
                table: "VerificationRequestsAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VerificationRequests",
                table: "VerificationRequests");

            migrationBuilder.RenameTable(
                name: "VerificationRequestsAttachments",
                newName: "DoctorsVerificationRequestsAttachments");

            migrationBuilder.RenameTable(
                name: "VerificationRequests",
                newName: "DoctorsVerificationRequests");

            migrationBuilder.RenameIndex(
                name: "IX_VerificationRequestsAttachments_DoctorVerificationRequestId",
                table: "DoctorsVerificationRequestsAttachments",
                newName: "IX_DoctorsVerificationRequestsAttachments_DoctorVerificationRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_VerificationRequests_ReviewerId",
                table: "DoctorsVerificationRequests",
                newName: "IX_DoctorsVerificationRequests_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_VerificationRequests_DoctorId",
                table: "DoctorsVerificationRequests",
                newName: "IX_DoctorsVerificationRequests_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorsVerificationRequestsAttachments",
                table: "DoctorsVerificationRequestsAttachments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DoctorsVerificationRequests",
                table: "DoctorsVerificationRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorsVerificationRequests_Admins_ReviewerId",
                table: "DoctorsVerificationRequests",
                column: "ReviewerId",
                principalTable: "Admins",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorsVerificationRequests_Doctors_DoctorId",
                table: "DoctorsVerificationRequests",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorsVerificationRequestsAttachments_DoctorsVerificationRequests_DoctorVerificationRequestId",
                table: "DoctorsVerificationRequestsAttachments",
                column: "DoctorVerificationRequestId",
                principalTable: "DoctorsVerificationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorsVerificationRequests_Admins_ReviewerId",
                table: "DoctorsVerificationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorsVerificationRequests_Doctors_DoctorId",
                table: "DoctorsVerificationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_DoctorsVerificationRequestsAttachments_DoctorsVerificationRequests_DoctorVerificationRequestId",
                table: "DoctorsVerificationRequestsAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorsVerificationRequestsAttachments",
                table: "DoctorsVerificationRequestsAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DoctorsVerificationRequests",
                table: "DoctorsVerificationRequests");

            migrationBuilder.RenameTable(
                name: "DoctorsVerificationRequestsAttachments",
                newName: "VerificationRequestsAttachments");

            migrationBuilder.RenameTable(
                name: "DoctorsVerificationRequests",
                newName: "VerificationRequests");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorsVerificationRequestsAttachments_DoctorVerificationRequestId",
                table: "VerificationRequestsAttachments",
                newName: "IX_VerificationRequestsAttachments_DoctorVerificationRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorsVerificationRequests_ReviewerId",
                table: "VerificationRequests",
                newName: "IX_VerificationRequests_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_DoctorsVerificationRequests_DoctorId",
                table: "VerificationRequests",
                newName: "IX_VerificationRequests_DoctorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VerificationRequestsAttachments",
                table: "VerificationRequestsAttachments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VerificationRequests",
                table: "VerificationRequests",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationRequests_Admins_ReviewerId",
                table: "VerificationRequests",
                column: "ReviewerId",
                principalTable: "Admins",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationRequests_Doctors_DoctorId",
                table: "VerificationRequests",
                column: "DoctorId",
                principalTable: "Doctors",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VerificationRequestsAttachments_VerificationRequests_DoctorVerificationRequestId",
                table: "VerificationRequestsAttachments",
                column: "DoctorVerificationRequestId",
                principalTable: "VerificationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
