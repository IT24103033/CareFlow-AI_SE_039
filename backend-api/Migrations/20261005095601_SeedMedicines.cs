using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedMedicines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0362aed7-94a2-4084-b6c4-3fa065793b48"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0a5e9ac6-b669-4e9f-a09d-55b461f9c5c3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0d4876f0-8854-4aec-85de-813c5b395f31"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("12d0fe59-7bd8-4c28-9f78-02c9940fa95a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("24ebc8d4-2080-47dd-9e70-b74dcd810ff5"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3528ce1f-0cc4-454a-bbee-f765209d458a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("35e0a9ea-4c51-4374-951b-6a33e0e5aefe"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3f625883-1a0b-46bb-9e1a-e22e4a5e485d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3fa0e8f5-5ee8-405c-a24b-9af70bd3aa0d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("44e4c71b-7f06-4d27-bcea-368aebe11283"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("4595f0eb-90a5-411b-bf6d-952b0d5df842"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("4a153134-46f7-4021-a3c4-d1f78b244879"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6599d5d9-a3b1-405a-ac24-15be148c32b3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6b74651a-342f-4244-9858-c2be8da9b23c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("73347ad7-9f3d-4d44-af41-f2cf190e0219"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7f9e966c-d3f8-46f9-8380-e5dc8f3d5005"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8ca9c78e-09eb-4aa8-ae35-b8f7bae5cb80"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8f836e61-83cf-4a8e-92ef-79983b07ad15"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a4549331-ee20-4545-9263-d9a57b279dd3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a4c098af-c4d9-4ae4-ba9b-91a71c721dde"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ae3e89de-3f28-43de-90b3-7913f6a1d8d4"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b045f7c8-1cc5-4e50-a917-54c5d243efa0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b370f28f-fe81-4426-85df-44edb3afb16c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c0e85055-a76c-4641-87de-e38d1795f398"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c2111a82-4e32-4985-a499-9bd22d86799f"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("da4e3828-8950-42e3-9803-fefbae0ed8bf"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e70dcad6-4d46-47df-9451-eaf9de8c7a99"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ec555e4b-d89f-4cba-bd6d-a1046accb827"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("0a2f21ef-f9fb-49b1-a741-c68d27f9b3b4"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1c39f8e6-0580-4505-a681-16aa3bd21624"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("242b0a08-b76a-468b-a5de-ff47ba082853"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2819004b-3e86-4e7f-8423-c0f0fe356dcf"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3501dc70-aca3-45e0-81eb-fc3a6350ef1e"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3c15b6bc-23d1-4343-ad55-be9925a13358"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3c84cb2a-98f4-40df-9000-31cba4c0a95c"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3de742d0-5d35-404d-a117-1e5c338cb7b3"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("486235a9-0f42-4ece-8bdd-c9ff69dc463d"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5587c168-4dc1-46d2-905e-8d9c8e5022e7"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5d0d9e6b-843b-4b90-8a99-daa27bef8bd3"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("72a0be44-044c-4e6d-a005-bf01e551ba67"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7cc1ae7e-bb0e-4eba-a310-b83c41a75334"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("80346575-892f-48e7-a936-60ed416167c8"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("84e72f94-dbb2-433d-b0ef-71a92c7697b8"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("9020f39c-9d1a-4379-8ce5-3b6edfa7cee1"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("98599668-af7d-4a98-8f09-bc869fa9c076"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a80248ac-abab-45d4-a1ce-7fb4b4502802"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b8f7a20d-8f83-4072-ad9f-c5ff295770f0"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c07f5da0-f9a5-48ca-b66a-f03ec657c695"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c1c594ac-11af-4688-8f17-63c6fe001f4a"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c445d0bc-946e-43c9-8fce-bbcba7ba014a"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c5920d9a-6eb3-4542-821f-677bb2fab502"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c9726a91-cd94-4abd-859f-cb49113ea99e"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ceba186a-4926-4002-a198-7cd40c0fa513"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e4b12814-9fb1-46d3-a18b-29201d0e4c16"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e5cf9b29-282e-46d5-9da5-a22dec78bbdc"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("fe8df537-6d0e-4f4d-9937-ef6115d6ba9d"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.InsertData(
                table: "Medicines",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "ExpiryDate", "IsActive", "Manufacturer", "Name", "ReorderLevel", "StockQuantity", "UnitPrice", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("66666666-6666-6666-6666-666666666661"), "Antibiotic", new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5110), "Used to treat bacterial infections.", new DateOnly(2028, 1, 1), true, "PharmaCorp", "Amoxicillin 500mg", 100, 1000, 15.00m, new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5110) },
                    { new Guid("66666666-6666-6666-6666-666666666662"), "Painkiller", new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5120), "Nonsteroidal anti-inflammatory drug (NSAID).", new DateOnly(2027, 6, 30), true, "HealthLife", "Ibuprofen 400mg", 50, 500, 8.50m, new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5120) },
                    { new Guid("66666666-6666-6666-6666-666666666663"), "Gastrointestinal", new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5130), "Proton pump inhibitor for acid reflux.", new DateOnly(2026, 12, 15), true, "GastroMed", "Omeprazole 20mg", 30, 300, 22.00m, new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5130) },
                    { new Guid("66666666-6666-6666-6666-666666666664"), "Blood Thinner", new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5130), "Used to reduce the risk of heart attacks.", new DateOnly(2029, 3, 22), true, "CardioCare", "Aspirin 81mg", 80, 800, 5.00m, new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5130) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5060));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5070));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5070));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5090));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5100));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5010));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 9, 56, 1, 44, DateTimeKind.Utc).AddTicks(5020));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0a2f21ef-f9fb-49b1-a741-c68d27f9b3b4"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1c39f8e6-0580-4505-a681-16aa3bd21624"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("242b0a08-b76a-468b-a5de-ff47ba082853"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2819004b-3e86-4e7f-8423-c0f0fe356dcf"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3501dc70-aca3-45e0-81eb-fc3a6350ef1e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3c15b6bc-23d1-4343-ad55-be9925a13358"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3c84cb2a-98f4-40df-9000-31cba4c0a95c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3de742d0-5d35-404d-a117-1e5c338cb7b3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("486235a9-0f42-4ece-8bdd-c9ff69dc463d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5587c168-4dc1-46d2-905e-8d9c8e5022e7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5d0d9e6b-843b-4b90-8a99-daa27bef8bd3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("72a0be44-044c-4e6d-a005-bf01e551ba67"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7cc1ae7e-bb0e-4eba-a310-b83c41a75334"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("80346575-892f-48e7-a936-60ed416167c8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("84e72f94-dbb2-433d-b0ef-71a92c7697b8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("9020f39c-9d1a-4379-8ce5-3b6edfa7cee1"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("98599668-af7d-4a98-8f09-bc869fa9c076"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a80248ac-abab-45d4-a1ce-7fb4b4502802"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b8f7a20d-8f83-4072-ad9f-c5ff295770f0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c07f5da0-f9a5-48ca-b66a-f03ec657c695"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c1c594ac-11af-4688-8f17-63c6fe001f4a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c445d0bc-946e-43c9-8fce-bbcba7ba014a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c5920d9a-6eb3-4542-821f-677bb2fab502"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c9726a91-cd94-4abd-859f-cb49113ea99e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ceba186a-4926-4002-a198-7cd40c0fa513"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e4b12814-9fb1-46d3-a18b-29201d0e4c16"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e5cf9b29-282e-46d5-9da5-a22dec78bbdc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("fe8df537-6d0e-4f4d-9937-ef6115d6ba9d"));

            migrationBuilder.DeleteData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666661"));

            migrationBuilder.DeleteData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666662"));

            migrationBuilder.DeleteData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666663"));

            migrationBuilder.DeleteData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666664"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("0362aed7-94a2-4084-b6c4-3fa065793b48"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0a5e9ac6-b669-4e9f-a09d-55b461f9c5c3"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0d4876f0-8854-4aec-85de-813c5b395f31"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("12d0fe59-7bd8-4c28-9f78-02c9940fa95a"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("24ebc8d4-2080-47dd-9e70-b74dcd810ff5"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3528ce1f-0cc4-454a-bbee-f765209d458a"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("35e0a9ea-4c51-4374-951b-6a33e0e5aefe"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3f625883-1a0b-46bb-9e1a-e22e4a5e485d"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3fa0e8f5-5ee8-405c-a24b-9af70bd3aa0d"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("44e4c71b-7f06-4d27-bcea-368aebe11283"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("4595f0eb-90a5-411b-bf6d-952b0d5df842"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("4a153134-46f7-4021-a3c4-d1f78b244879"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6599d5d9-a3b1-405a-ac24-15be148c32b3"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6b74651a-342f-4244-9858-c2be8da9b23c"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("73347ad7-9f3d-4d44-af41-f2cf190e0219"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7f9e966c-d3f8-46f9-8380-e5dc8f3d5005"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8ca9c78e-09eb-4aa8-ae35-b8f7bae5cb80"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8f836e61-83cf-4a8e-92ef-79983b07ad15"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a4549331-ee20-4545-9263-d9a57b279dd3"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a4c098af-c4d9-4ae4-ba9b-91a71c721dde"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ae3e89de-3f28-43de-90b3-7913f6a1d8d4"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b045f7c8-1cc5-4e50-a917-54c5d243efa0"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b370f28f-fe81-4426-85df-44edb3afb16c"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c0e85055-a76c-4641-87de-e38d1795f398"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c2111a82-4e32-4985-a499-9bd22d86799f"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("da4e3828-8950-42e3-9803-fefbae0ed8bf"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e70dcad6-4d46-47df-9451-eaf9de8c7a99"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ec555e4b-d89f-4cba-bd6d-a1046accb827"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5490));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5490));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5500));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5500));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5510));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5440));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5450));
        }
    }
}
