using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceSchedulingModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("01b987f2-74c8-41a3-8847-260be27d2607"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("16bff826-e518-4fad-9215-d40b128efe4f"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("210b0a1d-3f73-4dd3-ad0c-cc6a363153c0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("269f87ff-57ca-4ef0-a1dd-581f5566b306"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("36b59332-ca20-4508-aca1-e1e137dedb46"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3b06256a-d942-4f3b-ad7e-4ba6f9eed48d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("44fb9971-7d4e-40b4-9d47-7c2b36338ae2"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5197fbe8-f54a-4d87-bd7b-82b93d36e111"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("587cd0b2-95c4-433f-92dc-b9188ec5db80"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6224f692-78e7-48f6-a55b-eb8bcc9b9ae7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("682de52a-65a8-4f98-b9ee-6fe3ac81b76b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7260440d-6353-4c1e-a5f0-49326f42e993"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7ac29be8-eee8-4203-be5e-652a6cdb6dc7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7cb01284-60b0-4880-809b-004b8527706f"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("853a3600-cdb2-4b80-bfc6-7bed651c80a3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("872cb9ab-0875-4181-af32-75194d3bfa63"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8d88328d-01d4-4de3-9080-db2e4007c2bb"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("99215c53-cc5d-4a9b-b71e-d33f4cc6111d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("9c9f8f80-c8b7-4640-8af7-eab067e76a18"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a7eb8897-8c4f-4afd-a156-73ce9caad686"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a8cb586b-a2d5-4d19-978b-ffee0fe5d7af"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b1744bb9-a343-4325-be78-40b7667e844d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("bb106138-c2dd-4e96-8aeb-0f574300e117"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c3352b19-8c12-4a32-88fa-2fa16bb79c99"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d7ca6639-46e4-47c3-b1c3-7ceb1e4a780b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e45e97fd-d61c-4370-85b6-aead3f9e9085"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e4dfd2e5-b8df-4615-a78c-e8383d4f67d1"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f4e656d7-65d9-4ae9-beaa-897f57ba779f"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("0a414029-8870-4082-ada4-3d2b32dbf7ee"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0d4b805e-7d44-4e76-887b-de6346192f3a"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0f8baff3-42e4-4506-888f-38081fc21574"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("199603c4-8ce2-4f14-878b-b8f63e8863e1"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1f8e5a54-a798-4e37-b8a4-e403abda7560"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1ffda6b2-0826-4451-b907-9b8208cf7c01"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("237cfba2-d5ab-482a-977e-9fe517d2edf4"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("47f8096e-5bff-41b0-980f-4b0cef8296ce"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("485714ea-4975-4693-b36f-2de26ee545f0"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("51a0672d-9912-41f2-91c7-eb2cf21ed29e"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("53b062a9-6d42-4fda-b672-1e3f6c3726f8"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5cf25091-099b-4349-bfc1-916bed825e30"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6502b1b2-3fe0-415a-b0b1-57fc0e1a240a"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6dd3f5ac-5a04-4398-a7e0-a0f00795f154"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("742d29dd-10e7-4a22-926e-fca8e437e589"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7c8a6b04-f3bd-4002-becc-e5e911f40329"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("81e40f76-d1f3-4498-8fcc-d53de2191121"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8b10638d-82c4-4840-86ef-8bb437ab2a33"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a97bc2d1-ac23-4a6b-b911-48ebd8086f73"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b041b4ee-979b-404b-8db4-ad1a276dc191"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b1c0ddb3-a0fd-4219-8883-43ee4399b9fc"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d0da0c71-19b4-47fa-82b3-9ed8dcd9619b"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d14e8d6c-ac24-4f2c-a737-ed5226e4bfbd"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e2489c30-da1b-4d3f-a00c-bf59711e0353"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ee796a78-739f-454f-a002-55b849bdf3dc"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f6926a3d-0e57-45d4-b468-61176b4b7592"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f9246a79-42cd-4dac-9fe8-26d8c8b0e014"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f9640190-ba8b-48ef-93df-4221bc0fc7a3"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666661"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1940), new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1940) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666662"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1960), new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1960) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666663"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1960), new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1960) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666664"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1970), new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1970) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666665"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1980), new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1980) });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1890));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1900));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1900));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1910));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1920));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1790));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1800));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 13, 49, 56, 103, DateTimeKind.Utc).AddTicks(1810));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0a414029-8870-4082-ada4-3d2b32dbf7ee"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0d4b805e-7d44-4e76-887b-de6346192f3a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0f8baff3-42e4-4506-888f-38081fc21574"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("199603c4-8ce2-4f14-878b-b8f63e8863e1"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1f8e5a54-a798-4e37-b8a4-e403abda7560"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1ffda6b2-0826-4451-b907-9b8208cf7c01"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("237cfba2-d5ab-482a-977e-9fe517d2edf4"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("47f8096e-5bff-41b0-980f-4b0cef8296ce"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("485714ea-4975-4693-b36f-2de26ee545f0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("51a0672d-9912-41f2-91c7-eb2cf21ed29e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("53b062a9-6d42-4fda-b672-1e3f6c3726f8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5cf25091-099b-4349-bfc1-916bed825e30"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6502b1b2-3fe0-415a-b0b1-57fc0e1a240a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6dd3f5ac-5a04-4398-a7e0-a0f00795f154"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("742d29dd-10e7-4a22-926e-fca8e437e589"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7c8a6b04-f3bd-4002-becc-e5e911f40329"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("81e40f76-d1f3-4498-8fcc-d53de2191121"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8b10638d-82c4-4840-86ef-8bb437ab2a33"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a97bc2d1-ac23-4a6b-b911-48ebd8086f73"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b041b4ee-979b-404b-8db4-ad1a276dc191"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b1c0ddb3-a0fd-4219-8883-43ee4399b9fc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d0da0c71-19b4-47fa-82b3-9ed8dcd9619b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d14e8d6c-ac24-4f2c-a737-ed5226e4bfbd"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e2489c30-da1b-4d3f-a00c-bf59711e0353"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ee796a78-739f-454f-a002-55b849bdf3dc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f6926a3d-0e57-45d4-b468-61176b4b7592"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f9246a79-42cd-4dac-9fe8-26d8c8b0e014"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f9640190-ba8b-48ef-93df-4221bc0fc7a3"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("01b987f2-74c8-41a3-8847-260be27d2607"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("16bff826-e518-4fad-9215-d40b128efe4f"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("210b0a1d-3f73-4dd3-ad0c-cc6a363153c0"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("269f87ff-57ca-4ef0-a1dd-581f5566b306"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("36b59332-ca20-4508-aca1-e1e137dedb46"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3b06256a-d942-4f3b-ad7e-4ba6f9eed48d"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("44fb9971-7d4e-40b4-9d47-7c2b36338ae2"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5197fbe8-f54a-4d87-bd7b-82b93d36e111"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("587cd0b2-95c4-433f-92dc-b9188ec5db80"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6224f692-78e7-48f6-a55b-eb8bcc9b9ae7"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("682de52a-65a8-4f98-b9ee-6fe3ac81b76b"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7260440d-6353-4c1e-a5f0-49326f42e993"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7ac29be8-eee8-4203-be5e-652a6cdb6dc7"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7cb01284-60b0-4880-809b-004b8527706f"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("853a3600-cdb2-4b80-bfc6-7bed651c80a3"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("872cb9ab-0875-4181-af32-75194d3bfa63"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8d88328d-01d4-4de3-9080-db2e4007c2bb"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("99215c53-cc5d-4a9b-b71e-d33f4cc6111d"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("9c9f8f80-c8b7-4640-8af7-eab067e76a18"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a7eb8897-8c4f-4afd-a156-73ce9caad686"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a8cb586b-a2d5-4d19-978b-ffee0fe5d7af"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b1744bb9-a343-4325-be78-40b7667e844d"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("bb106138-c2dd-4e96-8aeb-0f574300e117"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c3352b19-8c12-4a32-88fa-2fa16bb79c99"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d7ca6639-46e4-47c3-b1c3-7ceb1e4a780b"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e45e97fd-d61c-4370-85b6-aead3f9e9085"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e4dfd2e5-b8df-4615-a78c-e8383d4f67d1"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f4e656d7-65d9-4ae9-beaa-897f57ba779f"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666661"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3320), new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3320) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666662"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3330), new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3330) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666663"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3330), new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3330) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666664"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3340), new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3340) });

            migrationBuilder.UpdateData(
                table: "Medicines",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666665"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3340), new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3340) });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3270));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3280));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3300));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3300));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3300));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3220));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 10, 6, 45, 264, DateTimeKind.Utc).AddTicks(3230));
        }
    }
}
