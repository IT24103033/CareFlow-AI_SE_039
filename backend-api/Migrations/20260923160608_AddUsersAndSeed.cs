using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersAndSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PatientProfiles",
                columns: new[] { "Id", "BloodGroup", "CreatedAt", "DateOfBirth", "FullName", "MedicalHistorySummary" },
                values: new object[,]
                {
                    { new Guid("55555555-5555-5555-5555-555555555551"), "O+", new DateTime(2026, 9, 23, 16, 6, 7, 376, DateTimeKind.Utc).AddTicks(9277), new DateOnly(1985, 3, 12), "Sarah Jenkins", "No known allergies. Previous appendectomy." },
                    { new Guid("55555555-5555-5555-5555-555555555552"), "A-", new DateTime(2026, 9, 23, 16, 6, 7, 376, DateTimeKind.Utc).AddTicks(9291), new DateOnly(1972, 11, 5), "Marcus Thorne", "Type 2 Diabetes, Hypertension." },
                    { new Guid("55555555-5555-5555-5555-555555555553"), "B+", new DateTime(2026, 9, 23, 16, 6, 7, 376, DateTimeKind.Utc).AddTicks(9299), new DateOnly(1990, 7, 22), "Emily Chen", "Asthma, treated with inhalers." },
                    { new Guid("55555555-5555-5555-5555-555555555554"), "O-", new DateTime(2026, 9, 23, 16, 6, 7, 376, DateTimeKind.Utc).AddTicks(9303), new DateOnly(1950, 1, 30), "David Alaba", "Coronary artery disease, pacemaker fitted 2018." },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "AB+", new DateTime(2026, 9, 23, 16, 6, 7, 376, DateTimeKind.Utc).AddTicks(9309), new DateOnly(2005, 9, 14), "Fiona Gallagher", "None." }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Password", "Role", "Username" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "password", "Admin", "admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "password", "Doctor", "doctor" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "password", "Staff", "staff" }
                });

            migrationBuilder.InsertData(
                table: "Wards",
                columns: new[] { "Id", "Capacity", "OccupiedBeds", "WardNumber", "WardType" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444441"), 50, 45, "G-01", "General" },
                    { new Guid("44444444-4444-4444-4444-444444444442"), 15, 12, "I-01", "ICU" },
                    { new Guid("44444444-4444-4444-4444-444444444443"), 30, 20, "M-01", "Maternity" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), 25, 10, "P-01", "Pediatrics" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DeleteData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"));

            migrationBuilder.DeleteData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"));

            migrationBuilder.DeleteData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"));

            migrationBuilder.DeleteData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"));

            migrationBuilder.DeleteData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444441"));

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444442"));

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444443"));

            migrationBuilder.DeleteData(
                table: "Wards",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));
        }
    }
}
