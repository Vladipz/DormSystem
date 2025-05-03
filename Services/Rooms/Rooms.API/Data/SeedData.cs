using Microsoft.EntityFrameworkCore;

using Rooms.API.Entities;

namespace Rooms.API.Data
{
    public static class SeedData
    {
        // Static GUIDs for seeding user references
        private static readonly Guid ReporterUser1Id = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ReporterUser2Id = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid MaintenanceStaffId = new Guid("33333333-3333-3333-3333-333333333333");

        public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            await dbContext.Database.MigrateAsync(cancellationToken);

            await SeedBuildingsAsync(dbContext, cancellationToken);
            await SeedFloorsAsync(dbContext, cancellationToken);
            await SeedBlocksAsync(dbContext, cancellationToken);
            await SeedRoomsAsync(dbContext, cancellationToken);
            await SeedPlacesAsync(dbContext, cancellationToken);
            await SeedMaintenanceTicketsAsync(dbContext, cancellationToken);
        }

        private static async Task SeedFloorsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Floors.AnyAsync(cancellationToken))
            {
                return;
            }

            var buildingIds = await dbContext.Buildings
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            var floors = new List<Floor>();

            foreach (var buildingId in buildingIds)
            {
                for (int i = 1; i <= 3; i++) // 3 floors for each building
                {
                    floors.Add(new Floor
                    {
                        Id = Guid.NewGuid(),
                        BuildingId = buildingId,
                        Number = i,
                        BlocksCount = 2,
                    });
                }
            }

            dbContext.Floors.AddRange(floors);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedBlocksAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Blocks.AnyAsync(cancellationToken))
            {
                return;
            }

            var floorIds = await dbContext.Floors
                .Select(f => f.Id)
                .ToListAsync(cancellationToken);

            var blocks = new List<Block>();

            foreach (var floorId in floorIds)
            {
                for (int i = 1; i <= 2; i++) // 2 blocks per floor
                {
                    blocks.Add(new Block
                    {
                        Id = Guid.NewGuid(),
                        FloorId = floorId,
                        Label = $"Block {i}",
                        GenderRule = i % 2 == 0 ? "Female" : "Male",
                    });
                }
            }

            dbContext.Blocks.AddRange(blocks);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedBuildingsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Buildings.AnyAsync(cancellationToken))
            {
                return;
            }

            var buildings = new List<Building>
            {
                new Building
                {
                    Id = Guid.NewGuid(),
                    Name = "Alpha Building",
                    Address = "123 Alpha Street",
                    FloorsCount = 5,
                    YearBuilt = 2010,
                    AdministratorContact = "admin@alpha.com",
                    IsActive = true,
                },
                new Building
                {
                    Id = Guid.NewGuid(),
                    Name = "Beta Building",
                    Address = "456 Beta Avenue",
                    FloorsCount = 3,
                    YearBuilt = 2015,
                    AdministratorContact = "admin@beta.com",
                    IsActive = true,
                },
            };

            dbContext.Buildings.AddRange(buildings);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedPlacesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Places.AnyAsync(cancellationToken))
            {
                return;
            }

            var rooms = await dbContext.Rooms
                .Select(r => new { r.Id, r.Capacity })
                .ToListAsync(cancellationToken);

            var places = new List<Place>();

            foreach (var room in rooms)
            {
                for (int i = 1; i <= room.Capacity; i++)
                {
                    places.Add(new Place
                    {
                        Id = Guid.NewGuid(),
                        RoomId = room.Id,
                        Index = i,
                    });
                }
            }

            dbContext.Places.AddRange(places);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedRoomsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Rooms.AnyAsync(cancellationToken))
            {
                return;
            }

            var blockIds = await dbContext.Blocks
                .Select(b => b.Id)
                .ToListAsync(cancellationToken);

            var rooms = new List<Room>();

            foreach (var blockId in blockIds)
            {
                for (int i = 1; i <= 3; i++) // 3 rooms per block
                {
                    rooms.Add(new Room
                    {
                        Id = Guid.NewGuid(),
                        BlockId = blockId,
                        Label = $"Room {i}",
                        Capacity = 2,
                        Status = RoomStatus.Available,
                        RoomType = RoomType.Regular,
                        Amenities = ["WiFi", "Desk"],
                    });
                }
            }

            dbContext.Rooms.AddRange(rooms);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedMaintenanceTicketsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.MaintenanceTickets.AnyAsync(cancellationToken))
            {
                return;
            }

            var firstRoom = await dbContext.Rooms.FirstOrDefaultAsync(cancellationToken);
            if (firstRoom is null)
            {
                return; // No rooms to assign maintenance tickets
            }

            var tickets = new List<MaintenanceTicket>
            {
                new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = firstRoom.Id,
                    Title = "Fix broken window",
                    Description = "Window in room is broken and needs replacement.",
                    Status = MaintenanceStatus.Open,
                    CreatedAt = DateTime.UtcNow,
                    ReporterById = ReporterUser1Id,
                    AssignedToId = null, // Not assigned yet
                    Priority = MaintenancePriority.High,
                },
                new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = firstRoom.Id,
                    Title = "Fix lights",
                    Description = "Light bulbs are not working.",
                    Status = MaintenanceStatus.InProgress,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    ReporterById = ReporterUser2Id,
                    AssignedToId = MaintenanceStaffId,
                    Priority = MaintenancePriority.Medium,
                },
                new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = firstRoom.Id,
                    Title = "Leaking faucet",
                    Description = "The bathroom faucet is leaking and needs repair.",
                    Status = MaintenanceStatus.Open,
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    ReporterById = ReporterUser1Id,
                    AssignedToId = null,
                    Priority = MaintenancePriority.Low,
                },
                new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = firstRoom.Id,
                    Title = "No hot water",
                    Description = "Urgent: No hot water in the shower. Need immediate assistance.",
                    Status = MaintenanceStatus.Open,
                    CreatedAt = DateTime.UtcNow.AddHours(-12),
                    ReporterById = ReporterUser2Id,
                    AssignedToId = MaintenanceStaffId,
                    Priority = MaintenancePriority.Critical,
                },
            };

            dbContext.MaintenanceTickets.AddRange(tickets);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}