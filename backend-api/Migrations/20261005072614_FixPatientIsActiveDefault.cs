using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class FixPatientIsActiveDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"PatientProfiles\" SET \"IsActive\" = true;");

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("01815333-6b77-404c-88cd-0f1ca64b2c3d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0402fa5d-c92c-42d4-890c-6d0fc70679ef"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("063fd00b-fdd2-47dc-9ffe-0e367856e93d"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0d3b32bc-1740-43af-bd93-e006c33670c2"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2d5457d1-c19b-4dd2-ab48-17464b20e97a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2e9b9a0b-7162-4292-9a10-4b687d74b4a9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("41542728-14da-4217-8cd3-78cf3f7c2044"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("4de033c2-f096-414b-9c6c-b46351b0ea22"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6e83c3ba-5818-482d-8215-5773d748974b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7aedf560-8c05-48e5-b25d-95da7f68d620"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7ed2569d-f8dc-4c6e-ad96-2fca526e209e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("7f27358c-d756-4598-898c-fb078a2fff70"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("8d5090ec-62af-418b-b216-c3313a32a92c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ad133017-b858-4356-a40f-c6418d75c9af"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b146b909-57a7-4629-bfbd-7f898513622b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b51723c4-2f7f-419b-b4eb-d03bd4ba017e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b7aa0c54-9e59-4cb4-ac04-9218781b44fc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ba5bc95a-db9b-4c43-b38f-820f29673789"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c2d44e23-45e3-45a7-9616-5f43b7c834a0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("cb070f4d-ae0c-47b1-9c2a-e1ce285abdd7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d59ed0f8-2331-422e-90c4-9156a8967dbc"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d5da6267-33d7-4ff2-bd66-da42e3fca0d3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("dbf44ce1-ae90-4415-ab37-ff04c21bc620"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("df3bfae7-62be-4284-8c79-9050e68ba3c0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e64ec965-ce69-4fbd-b6cb-9898190afdb8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ea67c15b-9603-4c16-89a3-76ec7430800e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f9115aca-e86a-413b-8eb1-5820026a654a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f913bf36-0a53-4c9c-866a-4c1899305402"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("01c320c4-f45c-4bef-99cb-189cb0aa09cb"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("048468aa-a1bf-4a7a-b5d8-5fc779a49ed3"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0a12b148-f8bf-4954-949c-5613516fa125"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0ce604ca-af1e-4450-96cd-f080d1f18163"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0ced5306-f445-4284-9d0f-9ce45dcdd6e2"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("15f983f0-ff62-41f7-9e0f-8dc484706516"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1787a272-4b08-4795-952f-49aee1724b79"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2c624300-19b6-4c44-ad07-4651faf069df"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("32ed715a-5808-442b-99b1-4b5752d57f67"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("33249204-565a-43c0-b4f1-7a42878ad5c9"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5064ba46-f3b0-4ff7-8ad7-6091e4bb4874"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("52c6bdd6-fd1f-42f2-8cc1-3dd54593b0f4"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("571407d7-1464-49e0-bb3a-bc33fd38a33e"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5a514277-3d6f-4745-9719-5cc36e9f74f2"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6865ff03-096c-4a66-96f6-19da1b1229b8"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6cb02329-6ad8-4135-895e-1283ce175dd6"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("9751b624-ce25-4ce3-b61c-6b132ae2e5f0"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("9dd77ef0-7db3-4804-a19a-245928dbafdd"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a222ca5b-f734-4a41-84de-74e6cc368efa"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ab585e6d-9910-4eaa-88b0-f27364edc332"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b477fc2f-fa42-4828-af5a-dbdfc9dd3578"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("becd0746-48e9-48fb-86fa-fc3202ab444a"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c1140d52-ca79-4e4e-aaf5-7a08a6492420"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("cff331dd-21b5-4cca-9daa-f66c91874471"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d0bc037a-2dc1-4ccc-afc1-b56bdc258b34"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d90223f4-60c7-40a0-a231-d0d2dd736a5e"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e6088ec4-5774-4337-a634-1f7f4c76d1c3"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ff0a36a2-5af6-4a42-aaa7-03d4dad4e309"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8790));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8800));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8810));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8740));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 26, 14, 657, DateTimeKind.Utc).AddTicks(8750));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("01c320c4-f45c-4bef-99cb-189cb0aa09cb"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("048468aa-a1bf-4a7a-b5d8-5fc779a49ed3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0a12b148-f8bf-4954-949c-5613516fa125"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0ce604ca-af1e-4450-96cd-f080d1f18163"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("0ced5306-f445-4284-9d0f-9ce45dcdd6e2"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("15f983f0-ff62-41f7-9e0f-8dc484706516"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1787a272-4b08-4795-952f-49aee1724b79"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2c624300-19b6-4c44-ad07-4651faf069df"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("32ed715a-5808-442b-99b1-4b5752d57f67"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("33249204-565a-43c0-b4f1-7a42878ad5c9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5064ba46-f3b0-4ff7-8ad7-6091e4bb4874"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("52c6bdd6-fd1f-42f2-8cc1-3dd54593b0f4"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("571407d7-1464-49e0-bb3a-bc33fd38a33e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5a514277-3d6f-4745-9719-5cc36e9f74f2"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6865ff03-096c-4a66-96f6-19da1b1229b8"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("6cb02329-6ad8-4135-895e-1283ce175dd6"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("9751b624-ce25-4ce3-b61c-6b132ae2e5f0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("9dd77ef0-7db3-4804-a19a-245928dbafdd"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a222ca5b-f734-4a41-84de-74e6cc368efa"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ab585e6d-9910-4eaa-88b0-f27364edc332"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b477fc2f-fa42-4828-af5a-dbdfc9dd3578"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("becd0746-48e9-48fb-86fa-fc3202ab444a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c1140d52-ca79-4e4e-aaf5-7a08a6492420"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("cff331dd-21b5-4cca-9daa-f66c91874471"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d0bc037a-2dc1-4ccc-afc1-b56bdc258b34"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("d90223f4-60c7-40a0-a231-d0d2dd736a5e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e6088ec4-5774-4337-a634-1f7f4c76d1c3"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ff0a36a2-5af6-4a42-aaa7-03d4dad4e309"));

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("01815333-6b77-404c-88cd-0f1ca64b2c3d"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0402fa5d-c92c-42d4-890c-6d0fc70679ef"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("063fd00b-fdd2-47dc-9ffe-0e367856e93d"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("0d3b32bc-1740-43af-bd93-e006c33670c2"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2d5457d1-c19b-4dd2-ab48-17464b20e97a"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2e9b9a0b-7162-4292-9a10-4b687d74b4a9"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("41542728-14da-4217-8cd3-78cf3f7c2044"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("4de033c2-f096-414b-9c6c-b46351b0ea22"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("6e83c3ba-5818-482d-8215-5773d748974b"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7aedf560-8c05-48e5-b25d-95da7f68d620"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7ed2569d-f8dc-4c6e-ad96-2fca526e209e"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("7f27358c-d756-4598-898c-fb078a2fff70"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("8d5090ec-62af-418b-b216-c3313a32a92c"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ad133017-b858-4356-a40f-c6418d75c9af"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b146b909-57a7-4629-bfbd-7f898513622b"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b51723c4-2f7f-419b-b4eb-d03bd4ba017e"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b7aa0c54-9e59-4cb4-ac04-9218781b44fc"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ba5bc95a-db9b-4c43-b38f-820f29673789"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c2d44e23-45e3-45a7-9616-5f43b7c834a0"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("cb070f4d-ae0c-47b1-9c2a-e1ce285abdd7"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d59ed0f8-2331-422e-90c4-9156a8967dbc"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("d5da6267-33d7-4ff2-bd66-da42e3fca0d3"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("dbf44ce1-ae90-4415-ab37-ff04c21bc620"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("df3bfae7-62be-4284-8c79-9050e68ba3c0"), new DateOnly(2026, 10, 11), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e64ec965-ce69-4fbd-b6cb-9898190afdb8"), new DateOnly(2026, 10, 12), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ea67c15b-9603-4c16-89a3-76ec7430800e"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f9115aca-e86a-413b-8eb1-5820026a654a"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f913bf36-0a53-4c9c-866a-4c1899305402"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9820));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9840));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9840));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9760));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9770));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9780));
        }
    }
}
