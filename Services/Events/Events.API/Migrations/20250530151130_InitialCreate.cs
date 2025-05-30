using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    NumberOfAttendees = table.Column<int>(type: "INTEGER", nullable: true),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPublic = table.Column<bool>(type: "INTEGER", nullable: false),
                    BuildingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RoomId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventParticipants_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvitationTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationTokens_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "Id", "BuildingId", "Date", "Description", "IsPublic", "Location", "Name", "NumberOfAttendees", "OwnerId", "RoomId" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2025, 6, 6, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1576), "Welcome party for new dorm residents. Come meet your neighbors and enjoy some refreshments!", true, "Main Hall - Alpha Building", "Welcome Party", 50, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2025, 6, 2, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1590), "Weekly study group for Computer Science students. Bring your textbooks and questions!", true, "Study Room A - Alpha Building", "Study Group Session", 15, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa") },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2025, 6, 13, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1595), "Board games, video games, and snacks! A fun evening for all dorm residents.", true, "Common Room - Beta Building", "Game Night", 25, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb") },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2025, 6, 1, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1599), "Monthly floor meeting to discuss community guidelines and upcoming events.", false, "2nd Floor Common Area - Alpha Building", "Floor Meeting", 20, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2025, 6, 9, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1602), "Free pizza night sponsored by the dorm council. First come, first served!", true, "Main Dining Hall - Beta Building", "Pizza Night", 100, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2025, 6, 4, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1609), "Learn proper laundry techniques and machine usage. Perfect for first-year students!", true, "Laundry Room - Alpha Building", "Laundry Workshop", 12, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("eeeeeeee-ffff-aaaa-bbbb-cccccccccccc") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2025, 6, 7, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1612), "Advanced mathematics study session. Calculus and statistics focus.", true, "Study Room B - Alpha Building", "Study Session - Advanced Math", 8, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("cccccccc-dddd-eeee-ffff-bbbbbbbbbbbb") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2025, 6, 11, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1616), "Weekly movie night featuring classic films. Popcorn provided!", true, "Common Room 2 - Beta Building", "Movie Night", 30, new Guid("11111111-1111-1111-1111-111111111111"), new Guid("dddddddd-eeee-ffff-aaaa-cccccccccccc") }
                });

            migrationBuilder.InsertData(
                table: "EventParticipants",
                columns: new[] { "Id", "EventId", "JoinedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1670), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1674), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-cccc-cccc-cccc-cccccccccccc"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 5, 28, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1676), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-dddd-dddd-dddd-dddddddddddd"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1678), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("55555555-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 5, 27, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1680), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("66666666-ffff-ffff-ffff-ffffffffffff"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 5, 30, 9, 11, 29, 909, DateTimeKind.Utc).AddTicks(1683), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("77777777-aaaa-bbbb-cccc-dddddddddddd"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2025, 5, 30, 3, 11, 29, 909, DateTimeKind.Utc).AddTicks(1686), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("88888888-aaaa-bbbb-cccc-dddddddddddd"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2025, 5, 30, 7, 11, 29, 909, DateTimeKind.Utc).AddTicks(1688), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2025, 5, 30, 11, 11, 29, 909, DateTimeKind.Utc).AddTicks(1690), new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "InvitationTokens",
                columns: new[] { "Id", "CreatedAt", "EventId", "ExpiresAt", "IsActive", "Token" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), new DateTime(2025, 5, 25, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1726), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2025, 6, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1727), true, "WELCOME2024" },
                    { new Guid("20202020-2020-2020-2020-202020202020"), new DateTime(2025, 5, 27, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1731), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 6, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1731), true, "STUDY2024" },
                    { new Guid("30303030-3030-3030-3030-303030303030"), new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1734), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 6, 19, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1734), true, "GAME2024" },
                    { new Guid("40404040-4040-4040-4040-404040404040"), new DateTime(2025, 5, 29, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1736), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2025, 6, 9, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1737), true, "LAUNDRY2024" },
                    { new Guid("50505050-5050-5050-5050-505050505050"), new DateTime(2025, 5, 30, 9, 11, 29, 909, DateTimeKind.Utc).AddTicks(1739), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2025, 6, 11, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1739), true, "MATH2024" },
                    { new Guid("60606060-6060-6060-6060-606060606060"), new DateTime(2025, 5, 30, 13, 11, 29, 909, DateTimeKind.Utc).AddTicks(1742), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2025, 6, 14, 15, 11, 29, 909, DateTimeKind.Utc).AddTicks(1742), true, "MOVIE2024" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipants_EventId",
                table: "EventParticipants",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_EventId",
                table: "InvitationTokens",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_Token",
                table: "InvitationTokens",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventParticipants");

            migrationBuilder.DropTable(
                name: "InvitationTokens");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
