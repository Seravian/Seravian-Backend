using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorIdentityVerificaitonV6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorsVerificationRequests_DoctorId",
                table: "DoctorsVerificationRequests");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsVerificationRequests_DoctorId",
                table: "DoctorsVerificationRequests",
                column: "DoctorId",
                unique: true,
                filter: "[Status] IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DoctorsVerificationRequests_DoctorId",
                table: "DoctorsVerificationRequests");

            migrationBuilder.CreateIndex(
                name: "IX_DoctorsVerificationRequests_DoctorId",
                table: "DoctorsVerificationRequests",
                column: "DoctorId",
                unique: true,
                filter: "[Status] = 0");
        }
    }
}
