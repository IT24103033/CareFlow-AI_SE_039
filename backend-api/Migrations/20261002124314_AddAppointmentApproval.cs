using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "Appointments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByDoctorId",
                table: "Appointments",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ApprovedByDoctorId",
                table: "Appointments");
        }
    }
}
