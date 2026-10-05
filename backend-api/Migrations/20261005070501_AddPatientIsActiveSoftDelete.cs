using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareFlowAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientIsActiveSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("07505f9c-d499-4cb7-9469-2d46c3f48424"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1152ed57-2e7a-4bf4-a166-33b87664498e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("1370d3ee-ddea-4f70-a866-ba70fa8ec30a"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("2f50558e-11bd-43c1-9c9f-a3f45dc6872e"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("34bae3d2-086e-43e1-b2e0-636e8cefdfe9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("35252d60-3d06-480f-9c6f-e28aa80aa178"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3b8a37b1-7ace-4949-9f7e-efc3090ff138"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("3c78335c-ef85-440b-ae60-12d8182311fe"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("5288125b-6e75-4d58-b515-ba071e40f686"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("71df104d-bdcd-423c-9efd-3fd14c2a5736"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("72325d0d-7a52-40fb-a007-08051ed234bf"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("94dc0fe6-0f1f-4b01-961a-4c8275439ee7"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("a99dfe3b-b2ff-45da-9f2f-dd9bde756174"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b4410bfb-4011-4c34-8eca-bde300ee1b3b"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b4a0a20f-e22e-4a44-932a-d31d9883ee57"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b5dca36b-27dd-4db3-9e30-e54dacd18558"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("b996946b-1d50-49e7-9c67-3a1ede5255d0"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c2b9c2b9-ed3e-471e-b4e0-8b47c1d44a24"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("c53b1a55-0920-48fb-ac34-bfd52275d749"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("dd07bdb3-98ab-4acc-bae7-36c8073bf0a5"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e0fea91b-397d-46ca-859e-8f32fcfc0af9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e839ef9c-ed70-410f-b75f-b59b0952ec3c"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("e909facf-55fc-461b-b1e9-004cc1b58819"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("ef4592f9-cb36-4f20-b34a-466f71904bc9"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f3497b2d-e5fd-4233-b976-a505d9eb2a89"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("f66a7c7e-9ead-40ab-9b5f-5b5480bd6567"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("fab12c65-1d9a-4f17-9bac-0eb37fc1f746"));

            migrationBuilder.DeleteData(
                table: "DoctorAvailabilities",
                keyColumn: "Id",
                keyValue: new Guid("fb2fcac3-f744-449f-9e9d-4fc9afd3071c"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "PatientProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

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
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9820), true });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9830), true });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9830), true });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9840), true });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "IsActive" },
                values: new object[] { new DateTime(2026, 10, 5, 7, 5, 0, 991, DateTimeKind.Utc).AddTicks(9840), true });

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PatientProfiles");

            migrationBuilder.InsertData(
                table: "DoctorAvailabilities",
                columns: new[] { "Id", "Date", "DoctorId", "EndTime", "StartTime" },
                values: new object[,]
                {
                    { new Guid("07505f9c-d499-4cb7-9469-2d46c3f48424"), new DateOnly(2026, 10, 4), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1152ed57-2e7a-4bf4-a166-33b87664498e"), new DateOnly(2026, 10, 5), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("1370d3ee-ddea-4f70-a866-ba70fa8ec30a"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("2f50558e-11bd-43c1-9c9f-a3f45dc6872e"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("34bae3d2-086e-43e1-b2e0-636e8cefdfe9"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("35252d60-3d06-480f-9c6f-e28aa80aa178"), new DateOnly(2026, 10, 4), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3b8a37b1-7ace-4949-9f7e-efc3090ff138"), new DateOnly(2026, 10, 4), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("3c78335c-ef85-440b-ae60-12d8182311fe"), new DateOnly(2026, 10, 5), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("5288125b-6e75-4d58-b515-ba071e40f686"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("71df104d-bdcd-423c-9efd-3fd14c2a5736"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("72325d0d-7a52-40fb-a007-08051ed234bf"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("94dc0fe6-0f1f-4b01-961a-4c8275439ee7"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("a99dfe3b-b2ff-45da-9f2f-dd9bde756174"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b4410bfb-4011-4c34-8eca-bde300ee1b3b"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b4a0a20f-e22e-4a44-932a-d31d9883ee57"), new DateOnly(2026, 10, 4), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b5dca36b-27dd-4db3-9e30-e54dacd18558"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("b996946b-1d50-49e7-9c67-3a1ede5255d0"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c2b9c2b9-ed3e-471e-b4e0-8b47c1d44a24"), new DateOnly(2026, 10, 5), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("c53b1a55-0920-48fb-ac34-bfd52275d749"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("dd07bdb3-98ab-4acc-bae7-36c8073bf0a5"), new DateOnly(2026, 10, 6), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e0fea91b-397d-46ca-859e-8f32fcfc0af9"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e839ef9c-ed70-410f-b75f-b59b0952ec3c"), new DateOnly(2026, 10, 5), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("e909facf-55fc-461b-b1e9-004cc1b58819"), new DateOnly(2026, 10, 10), new Guid("22222222-2222-2222-2222-222222222220"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("ef4592f9-cb36-4f20-b34a-466f71904bc9"), new DateOnly(2026, 10, 9), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f3497b2d-e5fd-4233-b976-a505d9eb2a89"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("f66a7c7e-9ead-40ab-9b5f-5b5480bd6567"), new DateOnly(2026, 10, 8), new Guid("22222222-2222-2222-2222-222222222223"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("fab12c65-1d9a-4f17-9bac-0eb37fc1f746"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222221"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) },
                    { new Guid("fb2fcac3-f744-449f-9e9d-4fc9afd3071c"), new DateOnly(2026, 10, 7), new Guid("22222222-2222-2222-2222-222222222224"), new TimeOnly(10, 30, 0), new TimeOnly(10, 0, 0) }
                });

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555551"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9820));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555552"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9830));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555553"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9840));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555554"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.UpdateData(
                table: "PatientProfiles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9860));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9720));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9730));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "CreatedAt",
                value: new DateTime(2026, 10, 3, 10, 29, 40, 185, DateTimeKind.Utc).AddTicks(9740));
        }
    }
}
