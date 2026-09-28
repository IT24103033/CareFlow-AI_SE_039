using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareFlowAI.API.Migrations
{
    public partial class AddTriageReviewHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TriageReviewHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TriageRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    SymptomsAtReview = table.Column<string>(type: "text", nullable: false),
                    AgentWorkflowStateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TriageReviewHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TriageReviewHistories_AgentWorkflows_AgentWorkflowStateId",
                        column: x => x.AgentWorkflowStateId,
                        principalTable: "AgentWorkflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TriageReviewHistories_TriageRecords_TriageRecordId",
                        column: x => x.TriageRecordId,
                        principalTable: "TriageRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TriageReviewHistories_AgentWorkflowStateId",
                table: "TriageReviewHistories",
                column: "AgentWorkflowStateId");

            migrationBuilder.CreateIndex(
                name: "IX_TriageReviewHistories_TriageRecordId",
                table: "TriageReviewHistories",
                column: "TriageRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TriageReviewHistories");
        }
    }
}
