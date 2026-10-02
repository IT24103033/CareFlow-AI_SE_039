using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientContactAndNotificationTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NotificationFailureReason",
                table: "Prescriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NotificationRetryCount",
                table: "Prescriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PatientProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "PatientProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                columns: new[] { "CreatedAt", "Email", "Phone" },
                values: new object[] { new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(162), null, null });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                columns: new[] { "CreatedAt", "Email", "Phone" },
                values: new object[] { new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(178), null, null });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                columns: new[] { "CreatedAt", "Email", "Phone" },
                values: new object[] { new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(181), null, null });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                columns: new[] { "CreatedAt", "Email", "Phone" },
                values: new object[] { new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(183), null, null });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "Email", "Phone" },
                values: new object[] { new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(186), null, null });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(97));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(102));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 2, 15, 1, 22, 468, DateTimeKind.Utc).AddTicks(107));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationFailureReason",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "NotificationRetryCount",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "PatientProfiles");

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9270));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9280));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9290));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9290));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9210));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 30, 9, 45, 10, 225, DateTimeKind.Utc).AddTicks(9220));
        }
    }
}
