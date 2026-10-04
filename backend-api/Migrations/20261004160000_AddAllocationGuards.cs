using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAllocationGuards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wards_WardNumber",
                table: "Wards",
                column: "WardNumber",
                unique: true);

            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Admissions_PatientProfileId_Active"
                ON "Admissions" ("PatientProfileId")
                WHERE "DischargedAt" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Users_Username", table: "Users");
            migrationBuilder.DropIndex(name: "IX_Wards_WardNumber", table: "Wards");
            migrationBuilder.Sql("""DROP INDEX IF EXISTS "IX_Admissions_PatientProfileId_Active";""");
        }
    }
}
