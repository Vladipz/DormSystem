using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    NumberOfAttendees = table.Column<int>(type: "integer", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
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
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), "Welcome party for new dorm residents. Come meet your neighbors and enjoy some refreshments!", true, "Main Hall - Alpha Building", "Welcome Party", 50, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Utc), "Weekly study group for Computer Science students. Bring your textbooks and questions!", true, "Study Room A - Alpha Building", "Study Group Session", 15, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa") },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2024, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Board games, video games, and snacks! A fun evening for all dorm residents.", true, "Common Room - Beta Building", "Game Night", 25, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb") },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Utc), "Monthly floor meeting to discuss community guidelines and upcoming events.", false, "2nd Floor Common Area - Alpha Building", "Floor Meeting", 20, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), "Free pizza night sponsored by the dorm council. First come, first served!", true, "Main Dining Hall - Beta Building", "Pizza Night", 100, new Guid("11111111-1111-1111-1111-111111111111"), null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Learn proper laundry techniques and machine usage. Perfect for first-year students!", true, "Laundry Room - Alpha Building", "Laundry Workshop", 12, new Guid("33333333-3333-3333-3333-333333333333"), new Guid("eeeeeeee-ffff-aaaa-bbbb-cccccccccccc") },
                    { new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"), new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Utc), "Advanced mathematics study session. Calculus and statistics focus.", true, "Study Room B - Alpha Building", "Study Session - Advanced Math", 8, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("cccccccc-dddd-eeee-ffff-bbbbbbbbbbbb") },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"), new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), "Weekly movie night featuring classic films. Popcorn provided!", true, "Common Room 2 - Beta Building", "Movie Night", 30, new Guid("11111111-1111-1111-1111-111111111111"), new Guid("dddddddd-eeee-ffff-aaaa-cccccccccccc") }
                });

            migrationBuilder.InsertData(
                table: "EventParticipants",
                columns: new[] { "Id", "EventId", "JoinedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("33333333-cccc-cccc-cccc-cccccccccccc"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2023, 12, 30, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-dddd-dddd-dddd-dddddddddddd"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("55555555-eeee-eeee-eeee-eeeeeeeeeeee"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("66666666-ffff-ffff-ffff-ffffffffffff"), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2023, 12, 31, 18, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("77777777-aaaa-bbbb-cccc-dddddddddddd"), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2023, 12, 31, 12, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("88888888-aaaa-bbbb-cccc-dddddddddddd"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2023, 12, 31, 16, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("99999999-aaaa-bbbb-cccc-dddddddddddd"), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2023, 12, 31, 20, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333") }
                });

            migrationBuilder.InsertData(
                table: "InvitationTokens",
                columns: new[] { "Id", "CreatedAt", "EventId", "ExpiresAt", "IsActive", "Token" },
                values: new object[,]
                {
                    { new Guid("10101010-1010-1010-1010-101010101010"), new DateTime(2023, 12, 27, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, "WELCOME2024" },
                    { new Guid("20202020-2020-2020-2020-202020202020"), new DateTime(2023, 12, 29, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), true, "STUDY2024" },
                    { new Guid("30303030-3030-3030-3030-303030303030"), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2024, 1, 21, 0, 0, 0, 0, DateTimeKind.Utc), true, "GAME2024" },
                    { new Guid("40404040-4040-4040-4040-404040404040"), new DateTime(2023, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc), true, "LAUNDRY2024" },
                    { new Guid("50505050-5050-5050-5050-505050505050"), new DateTime(2023, 12, 31, 18, 0, 0, 0, DateTimeKind.Utc), new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), new DateTime(2024, 1, 13, 0, 0, 0, 0, DateTimeKind.Utc), true, "MATH2024" },
                    { new Guid("60606060-6060-6060-6060-606060606060"), new DateTime(2023, 12, 31, 22, 0, 0, 0, DateTimeKind.Utc), new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), true, "MOVIE2024" }
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
