using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class FixSeedDataWithStaticDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("33333333-cccc-cccc-cccc-cccccccccccc"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("44444444-dddd-dddd-dddd-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("55555555-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("66666666-ffff-ffff-ffff-ffffffffffff"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 18, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("77777777-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 12, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 16, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2023, 12, 31, 20, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Date",
                value: new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Date",
                value: new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Date",
                value: new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Date",
                value: new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Date",
                value: new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Date",
                value: new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Date",
                value: new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "Date",
                value: new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("50505050-5050-5050-5050-505050505050"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 31, 18, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("60606060-6060-6060-6060-606060606060"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2023, 12, 31, 22, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1670));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1674));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("33333333-cccc-cccc-cccc-cccccccccccc"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 28, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1676));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("44444444-dddd-dddd-dddd-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1678));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("55555555-eeee-eeee-eeee-eeeeeeeeeeee"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 27, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1680));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("66666666-ffff-ffff-ffff-ffffffffffff"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 30, 9, 11, 29, 909, DateTimeKind.Utc).AddTicks(1683));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("77777777-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 30, 3, 11, 29, 909, DateTimeKind.Utc).AddTicks(1686));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 30, 7, 11, 29, 909, DateTimeKind.Utc).AddTicks(1688));

            migrationBuilder.UpdateData(
                table: "EventParticipants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"),
                column: "JoinedAt",
                value: new DateTime(2025, 5, 30, 11, 11, 29, 909, DateTimeKind.Utc).AddTicks(1690));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "Date",
                value: new DateTime(2025, 6, 6, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1576));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "Date",
                value: new DateTime(2025, 6, 2, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                column: "Date",
                value: new DateTime(2025, 6, 13, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1595));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                column: "Date",
                value: new DateTime(2025, 6, 1, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1599));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                column: "Date",
                value: new DateTime(2025, 6, 9, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1602));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                column: "Date",
                value: new DateTime(2025, 6, 4, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1609));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                column: "Date",
                value: new DateTime(2025, 6, 7, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1612));

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                column: "Date",
                value: new DateTime(2025, 6, 11, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1616));

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("10101010-1010-1010-1010-101010101010"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 25, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1726), new DateTime(2025, 6, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1727) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("20202020-2020-2020-2020-202020202020"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 27, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1731), new DateTime(2025, 6, 6, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1731) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("30303030-3030-3030-3030-303030303030"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1734), new DateTime(2025, 6, 19, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1734) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("40404040-4040-4040-4040-404040404040"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1736), new DateTime(2025, 6, 9, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1737) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("50505050-5050-5050-5050-505050505050"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 30, 9, 11, 29, 909, DateTimeKind.Utc).AddTicks(1739), new DateTime(2025, 6, 11, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1739) });

            migrationBuilder.UpdateData(
                table: "InvitationTokens",
                keyColumn: "Id",
                keyValue: new Guid("60606060-6060-6060-6060-606060606060"),
                columns: new[] { "CreatedAt", "ExpiresAt" },
                values: new object[] { new DateTime(2025, 5, 30, 13, 11, 29, 909, DateTimeKind.Utc).AddTicks(1742), new DateTime(2025, 6, 14, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1742) });
        }
    }
}
