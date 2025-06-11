using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Seravian.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionBookingV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SessionBookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PatientIsAvailableFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientIsAvailableToUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DoctorNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PatientNote = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionBookings_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionBookings_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_DoctorId",
                table: "SessionBookings",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionBookings_PatientId",
                table: "SessionBookings",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionBookings");
        }
    }
}
