using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTriageAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "TriageRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "TriageRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TriageAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CloudinaryPublicId = table.Column<string>(type: "text", nullable: false),
                    UploaderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriageAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriageAttachments_TriageRecords_TriageRecordId",
                        column: x => x.TriageRecordId,
                        principalTable: "TriageRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_TriageRecords_AttachmentId",
                table: "TriageRecords",
                column: "AttachmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TriageAttachments_TriageRecordId",
                table: "TriageAttachments",
                column: "TriageRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_TriageRecords_TriageAttachments_AttachmentId",
                table: "TriageRecords",
                column: "AttachmentId",
                principalTable: "TriageAttachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TriageRecords_TriageAttachments_AttachmentId",
                table: "TriageRecords");

            migrationBuilder.DropTable(
                name: "TriageAttachments");

            migrationBuilder.DropIndex(
                name: "IX_TriageRecords_AttachmentId",
                table: "TriageRecords");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "TriageRecords");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "TriageRecords",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9710));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9730));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9730));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9730));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9660));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9670));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 9, 28, 16, 37, 31, 807, DateTimeKind.Utc).AddTicks(9670));
        }
    }
}
