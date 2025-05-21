using System.Globalization;

using Inspections.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace Inspections.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            await dbContext.Database.MigrateAsync(cancellationToken);

            if (await dbContext.Inspections.AnyAsync(cancellationToken))
            {
                return;
            }

            // Seed 1: Scheduled Safety Inspection
            var inspection1 = new Inspection
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Monthly Safety Inspection",
                Type = "Safety",
                StartDate = new DateTime(2025, 5, 10, 9, 0, 0, DateTimeKind.Utc),
                Status = InspectionStatus.Scheduled,
                Rooms = new List<RoomInspection>
                {
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("00c7e3e0-fa58-4699-94ab-ac73518e37bb"),
                        RoomNumber = "Common Room",
                        Floor = "3",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("02480b7a-035b-439f-8a92-564c6871abab"),
                        RoomNumber = "Room 2",
                        Floor = "3",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                },
            };

            // Seed 2: Active Maintenance Check
            var inspection2 = new Inspection
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Quarterly Maintenance Check",
                Type = "Maintenance",
                StartDate = new DateTime(2025, 5, 15, 10, 30, 0, DateTimeKind.Utc),
                Status = InspectionStatus.Active,
                Rooms = new List<RoomInspection>
                {
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("2666f98b-3817-4b36-9bae-3593c02e1803"),
                        RoomNumber = "Room 2",
                        Floor = "2",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Confirmed,
                    },
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("2a8ecdd1-ecae-4136-96e5-85f632dae4b2"),
                        RoomNumber = "Room 2",
                        Floor = "1",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                },
            };

            // Seed 3: Scheduled Fire Safety
            var inspection3 = new Inspection
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Annual Fire Safety Inspection",
                Type = "Fire Safety",
                StartDate = new DateTime(2025, 4, 20, 14, 0, 0, DateTimeKind.Utc),
                Status = InspectionStatus.Scheduled,
                Rooms = new List<RoomInspection>
                {
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("3fb78d9f-11ff-4436-8396-b89b35bcb917"),
                        RoomNumber = "Room 3",
                        Floor = "2",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("56d3a436-a09b-4e2a-bc2e-585118186838"),
                        RoomNumber = "Laundry Room",
                        Floor = "1",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                },
            };

            // Seed 4: Scheduled Health Check
            var inspection4 = new Inspection
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Name = "Bi-Annual Health Inspection",
                Type = "Health",
                StartDate = new DateTime(2025, 6, 1, 8, 0, 0, DateTimeKind.Utc),
                Status = InspectionStatus.Scheduled,
                Rooms = new List<RoomInspection>
                {
                    new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = Guid.Parse("68747b34-f07f-4c72-92ba-af956c93ebdb"),
                        RoomNumber = "Room 1",
                        Floor = "1",
                        Building = "Alpha Building",
                        Status = RoomInspectionStatus.Pending,
                    },
                },
            };

            // Seed 5: Completed General Inspection with 40+ rooms
            var inspection5 = new Inspection
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Name = "End of Semester Inspection",
                Type = "General",
                StartDate = new DateTime(2025, 3, 5, 9, 0, 0, DateTimeKind.Utc),
                Status = InspectionStatus.Completed,
                Rooms = new List<RoomInspection>(),
            };

            // Generate 40 fake rooms for completed inspection
            var rnd = new Random();

            // Predefine comment phrase lists
            var confirmedPhrases = new[]
            {
                "All good",
                "No issues detected",
                "Clean and tidy",
                "Ready for occupancy",
                "Inspection passed",
                "No deficiencies found",
                "Room in perfect condition",
                "Everything meets standards",
                "No corrective actions needed",
                "Condition acceptable",
            };
            var notConfirmedPhrases = new[]
            {
                "Needs follow-up",
                "Issues found, re-inspection required",
                "Broken window observed",
                "Water leak detected",
                "Furniture damaged",
                "Electrical fault noted",
                "Paint peeling off walls",
                "Door handle loose",
                "Ventilation problem noticed",
                "Heating system malfunctioning",
            };
            var noAccessPhrases = new[]
            {
                "No answer at door",
                "Room locked, could not enter",
                "Access code invalid",
                "Resident refused inspection",
                "Door chain engaged",
                "Security alarm active",
                "Resident unavailable",
                "Key missing",
                "Access blocked",
                "Area restricted",
            };

            for (int i = 1; i <= 40; i++)
            {
                // Randomize room details
                var roomNumber = (i % 3 == 0) ? "Common Room" : $"Room {i}";
                var floor = (((i - 1) % 5) + 1).ToString(CultureInfo.InvariantCulture);
                var building = "Alpha Building";

                // Random status assignments
                var statuses = new[] { RoomInspectionStatus.Confirmed, RoomInspectionStatus.NotConfirmed, RoomInspectionStatus.NoAccess };
                var status = statuses[rnd.Next(statuses.Length)];

                // Select a random comment based on status
                string? comment = status switch
                {
                    RoomInspectionStatus.Confirmed => confirmedPhrases[rnd.Next(confirmedPhrases.Length)],
                    RoomInspectionStatus.NotConfirmed => notConfirmedPhrases[rnd.Next(notConfirmedPhrases.Length)],
                    RoomInspectionStatus.NoAccess => noAccessPhrases[rnd.Next(noAccessPhrases.Length)],
                    _ => null
                };

                inspection5.Rooms.Add(new RoomInspection
                {
                    Id = Guid.NewGuid(),
                    RoomId = Guid.NewGuid(), // fake room ID
                    RoomNumber = roomNumber,
                    Floor = floor,
                    Building = building,
                    Status = status,
                    Comment = comment,
                });
            }

            // Add all inspections
            dbContext.Inspections.AddRange(inspection1, inspection2, inspection3, inspection4, inspection5);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
