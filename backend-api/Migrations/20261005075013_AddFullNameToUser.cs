using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("001e50bd-a5c7-45ac-bf90-9ac1759c9fa6"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1642dda9-90fc-45fd-9612-f83ff6d9893a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1c6e957d-8b11-46c0-9363-8e75fa64cf19"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("29cffa2e-fd73-4b90-a6f5-6261548c0cb8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2dbab48c-fcf9-4bf1-9b67-ed70c2048956"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3a42b95e-207b-43b9-ac02-3abd30f78e71"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3cdc4392-fca1-4cf8-af38-5b2cb6606943"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("408bfec6-c02b-4df6-9874-35a82ad75872"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5896e5c5-f49a-427a-963a-155ef9aa0ec9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5cf58a49-4b5a-46a9-9e72-2d795b7f3965"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6d8e0450-1cfb-473a-9cab-bb48d844f1aa"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("732bffc3-44ab-401d-bd45-85cb781e0e89"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("76b985e1-64a9-4d79-8707-a34392b8e29f"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7744539e-7606-43ab-8cb6-312a8d789917"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7b99d311-9af6-4a1a-b684-29e9036595f7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7fb4e69a-87a5-4d05-8509-d0401b5822c3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8308b390-7dcb-4795-a8c2-4f135eec6ea4"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("92ad9d34-92bb-4c8d-96a1-bfa0893f0ebb"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a19dbd1d-912a-47fb-a4a7-fedff96dfe59"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("aafa9621-ea85-4ad7-8b35-3d4c1d5b42a6"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("bcc92ece-75c4-454e-89cf-6766aa078753"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d10ec423-cdca-4eee-b09c-2d977e82b0bc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d4e71a46-1747-4baa-aed2-fd9ae11d9160"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("dbe6a1d8-cd0e-487f-814f-0b36c2918ca6"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ddd431b8-8738-48d0-8cea-55c49eaa8b4c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e11b3d4c-814f-44e9-849f-39216a77bfc0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e845e0c0-3aaf-46d9-a9ff-feff94640f26"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f80cefdc-ad3c-4c60-91de-9e46fdd7a1a9"));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Users",
                type: "text",
                nullable: false,
                defaultValue: "");

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
                columns: new[] { "CreatedAt", "FullName" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5440), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "FullName" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5440), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "FullName" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 50, 13, 240, DateTimeKind.Utc).AddTicks(5450), "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Users");

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("001e50bd-a5c7-45ac-bf90-9ac1759c9fa6"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1642dda9-90fc-45fd-9612-f83ff6d9893a"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1c6e957d-8b11-46c0-9363-8e75fa64cf19"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("29cffa2e-fd73-4b90-a6f5-6261548c0cb8"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2dbab48c-fcf9-4bf1-9b67-ed70c2048956"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3a42b95e-207b-43b9-ac02-3abd30f78e71"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3cdc4392-fca1-4cf8-af38-5b2cb6606943"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("408bfec6-c02b-4df6-9874-35a82ad75872"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5896e5c5-f49a-427a-963a-155ef9aa0ec9"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5cf58a49-4b5a-46a9-9e72-2d795b7f3965"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6d8e0450-1cfb-473a-9cab-bb48d844f1aa"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("732bffc3-44ab-401d-bd45-85cb781e0e89"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("76b985e1-64a9-4d79-8707-a34392b8e29f"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7744539e-7606-43ab-8cb6-312a8d789917"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7b99d311-9af6-4a1a-b684-29e9036595f7"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7fb4e69a-87a5-4d05-8509-d0401b5822c3"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8308b390-7dcb-4795-a8c2-4f135eec6ea4"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("92ad9d34-92bb-4c8d-96a1-bfa0893f0ebb"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a19dbd1d-912a-47fb-a4a7-fedff96dfe59"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("aafa9621-ea85-4ad7-8b35-3d4c1d5b42a6"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("bcc92ece-75c4-454e-89cf-6766aa078753"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d10ec423-cdca-4eee-b09c-2d977e82b0bc"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d4e71a46-1747-4baa-aed2-fd9ae11d9160"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("dbe6a1d8-cd0e-487f-814f-0b36c2918ca6"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ddd431b8-8738-48d0-8cea-55c49eaa8b4c"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e11b3d4c-814f-44e9-849f-39216a77bfc0"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e845e0c0-3aaf-46d9-a9ff-feff94640f26"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f80cefdc-ad3c-4c60-91de-9e46fdd7a1a9"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4920));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4930));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4930));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4940));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4940));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4860));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4870));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 38, 27, 886, DateTimeKind.Utc).AddTicks(4880));
        }
    }
}
