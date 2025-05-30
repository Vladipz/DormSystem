using Events.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Database
{
    public static class SeedData
    {
        // Static GUIDs matching the ones in Auth.BLL.Services.DbSeederService
        private static readonly Guid AdminUserId = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid RegularUserId = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid MaintenanceStaffId = new Guid("33333333-3333-3333-3333-333333333333");

        // Static GUIDs matching the ones in Rooms.API.Data.SeedData
        private static readonly Guid AlphaBuildingId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        private static readonly Guid BetaBuildingId = new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
        private static readonly Guid StudyRoomId = new Guid("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa");
        private static readonly Guid LaundryRoomId = new Guid("eeeeeeee-ffff-aaaa-bbbb-cccccccccccc");
        private static readonly Guid CommonRoomId = new Guid("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb");
        private static readonly Guid StudyRoom2Id = new Guid("cccccccc-dddd-eeee-ffff-bbbbbbbbbbbb");
        private static readonly Guid CommonRoom2Id = new Guid("dddddddd-eeee-ffff-aaaa-cccccccccccc");

        public static void SeedEvents(ModelBuilder modelBuilder)
        {
            var events = new List<DormEvent>
            {
                new DormEvent
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Welcome Party",
                    Date = DateTime.UtcNow.AddDays(7),
                    Location = "Main Hall - Alpha Building",
                    Description = "Welcome party for new dorm residents. Come meet your neighbors and enjoy some refreshments!",
                    NumberOfAttendees = 50,
                    OwnerId = AdminUserId,
                    IsPublic = true,
                    BuildingId = AlphaBuildingId,
                    RoomId = null,
                },
                new DormEvent
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Study Group Session",
                    Date = DateTime.UtcNow.AddDays(3),
                    Location = "Study Room A - Alpha Building",
                    Description = "Weekly study group for Computer Science students. Bring your textbooks and questions!",
                    NumberOfAttendees = 15,
                    OwnerId = RegularUserId,
                    IsPublic = true,
                    BuildingId = AlphaBuildingId,
                    RoomId = StudyRoomId,
                },
                new DormEvent
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    Name = "Game Night",
                    Date = DateTime.UtcNow.AddDays(14),
                    Location = "Common Room - Beta Building",
                    Description = "Board games, video games, and snacks! A fun evening for all dorm residents.",
                    NumberOfAttendees = 25,
                    OwnerId = RegularUserId,
                    IsPublic = true,
                    BuildingId = BetaBuildingId,
                    RoomId = CommonRoomId,
                },
                new DormEvent
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    Name = "Floor Meeting",
                    Date = DateTime.UtcNow.AddDays(2),
                    Location = "2nd Floor Common Area - Alpha Building",
                    Description = "Monthly floor meeting to discuss community guidelines and upcoming events.",
                    NumberOfAttendees = 20,
                    OwnerId = AdminUserId,
                    IsPublic = false,
                    BuildingId = AlphaBuildingId,
                    RoomId = null,
                },
                new DormEvent
                {
                    Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                    Name = "Pizza Night",
                    Date = DateTime.UtcNow.AddDays(10),
                    Location = "Main Dining Hall - Beta Building",
                    Description = "Free pizza night sponsored by the dorm council. First come, first served!",
                    NumberOfAttendees = 100,
                    OwnerId = AdminUserId,
                    IsPublic = true,
                    BuildingId = BetaBuildingId,
                    RoomId = null,
                },
                new DormEvent
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Name = "Laundry Workshop",
                    Date = DateTime.UtcNow.AddDays(5),
                    Location = "Laundry Room - Alpha Building",
                    Description = "Learn proper laundry techniques and machine usage. Perfect for first-year students!",
                    NumberOfAttendees = 12,
                    OwnerId = MaintenanceStaffId,
                    IsPublic = true,
                    BuildingId = AlphaBuildingId,
                    RoomId = LaundryRoomId,
                },
                new DormEvent
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Name = "Study Session - Advanced Math",
                    Date = DateTime.UtcNow.AddDays(8),
                    Location = "Study Room B - Alpha Building",
                    Description = "Advanced mathematics study session. Calculus and statistics focus.",
                    NumberOfAttendees = 8,
                    OwnerId = RegularUserId,
                    IsPublic = true,
                    BuildingId = AlphaBuildingId,
                    RoomId = StudyRoom2Id,
                },
                new DormEvent
                {
                    Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    Name = "Movie Night",
                    Date = DateTime.UtcNow.AddDays(12),
                    Location = "Common Room 2 - Beta Building",
                    Description = "Weekly movie night featuring classic films. Popcorn provided!",
                    NumberOfAttendees = 30,
                    OwnerId = AdminUserId,
                    IsPublic = true,
                    BuildingId = BetaBuildingId,
                    RoomId = CommonRoom2Id,
                },
            };

            modelBuilder.Entity<DormEvent>().HasData(events);

            // Seed event participants using real user IDs from Auth service
            var participants = new List<EventParticipant>
            {
                // Welcome Party participants
                new EventParticipant
                {
                    Id = Guid.Parse("11111111-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    EventId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    UserId = RegularUserId,
                    JoinedAt = DateTime.UtcNow.AddDays(-1),
                },
                new EventParticipant
                {
                    Id = Guid.Parse("22222222-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    EventId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    UserId = MaintenanceStaffId,
                    JoinedAt = DateTime.UtcNow.AddDays(-1),
                },
                // Study Group participants
                new EventParticipant
                {
                    Id = Guid.Parse("33333333-cccc-cccc-cccc-cccccccccccc"),
                    EventId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    UserId = AdminUserId,
                    JoinedAt = DateTime.UtcNow.AddDays(-2),
                },
                new EventParticipant
                {
                    Id = Guid.Parse("44444444-dddd-dddd-dddd-dddddddddddd"),
                    EventId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    UserId = MaintenanceStaffId,
                    JoinedAt = DateTime.UtcNow.AddDays(-1),
                },
                // Game Night participants
                new EventParticipant
                {
                    Id = Guid.Parse("55555555-eeee-eeee-eeee-eeeeeeeeeeee"),
                    EventId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    UserId = AdminUserId,
                    JoinedAt = DateTime.UtcNow.AddDays(-3),
                },
                // Laundry Workshop participants
                new EventParticipant
                {
                    Id = Guid.Parse("66666666-ffff-ffff-ffff-ffffffffffff"),
                    EventId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    UserId = RegularUserId,
                    JoinedAt = DateTime.UtcNow.AddHours(-6),
                },
                // Advanced Math Study participants
                new EventParticipant
                {
                    Id = Guid.Parse("77777777-aaaa-bbbb-cccc-dddddddddddd"),
                    EventId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    UserId = AdminUserId,
                    JoinedAt = DateTime.UtcNow.AddHours(-12),
                },
                // Movie Night participants
                new EventParticipant
                {
                    Id = Guid.Parse("88888888-aaaa-bbbb-cccc-dddddddddddd"),
                    EventId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    UserId = RegularUserId,
                    JoinedAt = DateTime.UtcNow.AddHours(-8),
                },
                new EventParticipant
                {
                    Id = Guid.Parse("99999999-aaaa-bbbb-cccc-dddddddddddd"),
                    EventId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    UserId = MaintenanceStaffId,
                    JoinedAt = DateTime.UtcNow.AddHours(-4),
                },
            };

            modelBuilder.Entity<EventParticipant>().HasData(participants);

            // Seed invitation tokens
            var invitationTokens = new List<InvitationToken>
            {
                new InvitationToken
                {
                    Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                    Token = "WELCOME2024",
                    EventId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true,
                },
                new InvitationToken
                {
                    Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                    Token = "STUDY2024",
                    EventId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsActive = true,
                },
                new InvitationToken
                {
                    Id = Guid.Parse("30303030-3030-3030-3030-303030303030"),
                    Token = "GAME2024",
                    EventId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(20),
                    IsActive = true,
                },
                new InvitationToken
                {
                    Id = Guid.Parse("40404040-4040-4040-4040-404040404040"),
                    Token = "LAUNDRY2024",
                    EventId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ExpiresAt = DateTime.UtcNow.AddDays(10),
                    IsActive = true,
                },
                new InvitationToken
                {
                    Id = Guid.Parse("50505050-5050-5050-5050-505050505050"),
                    Token = "MATH2024",
                    EventId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    CreatedAt = DateTime.UtcNow.AddHours(-6),
                    ExpiresAt = DateTime.UtcNow.AddDays(12),
                    IsActive = true,
                },
                new InvitationToken
                {
                    Id = Guid.Parse("60606060-6060-6060-6060-606060606060"),
                    Token = "MOVIE2024",
                    EventId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                    CreatedAt = DateTime.UtcNow.AddHours(-2),
                    ExpiresAt = DateTime.UtcNow.AddDays(15),
                    IsActive = true,
                },
            };

            modelBuilder.Entity<InvitationToken>().HasData(invitationTokens);
        }
    }
}