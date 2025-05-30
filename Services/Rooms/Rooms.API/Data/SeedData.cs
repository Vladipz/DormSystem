using System.Collections.ObjectModel;

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

        // Predefined GUIDs for buildings
        private static readonly Guid AlphaBuildingId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        private static readonly Guid BetaBuildingId = new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");

        // Predefined GUIDs for floors
        private static readonly Guid AlphaFloor1Id = new Guid("f1111111-1111-1111-1111-111111111111");
        private static readonly Guid AlphaFloor2Id = new Guid("f2222222-2222-2222-2222-222222222222");
        private static readonly Guid AlphaFloor3Id = new Guid("f3333333-3333-3333-3333-333333333333");
        private static readonly Guid BetaFloor1Id = new Guid("f4444444-4444-4444-4444-444444444444");
        private static readonly Guid BetaFloor2Id = new Guid("f5555555-5555-5555-5555-555555555555");
        private static readonly Guid BetaFloor3Id = new Guid("f6666666-6666-6666-6666-666666666666");

        // Predefined GUIDs for specialized rooms (one per floor)
        private static readonly Guid StudyRoomId = new Guid("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa");
        private static readonly Guid LaundryRoomId = new Guid("eeeeeeee-ffff-aaaa-bbbb-cccccccccccc");
        private static readonly Guid CommonRoomId = new Guid("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb");
        private static readonly Guid StudyRoom2Id = new Guid("cccccccc-dddd-eeee-ffff-bbbbbbbbbbbb");
        private static readonly Guid LaundryRoom2Id = new Guid("eeeeeeee-ffff-aaaa-bbbb-dddddddddddd");
        private static readonly Guid CommonRoom2Id = new Guid("dddddddd-eeee-ffff-aaaa-cccccccccccc");

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

            var floors = new List<Floor>
            {
                // Alpha Building floors
                new Floor
                {
                    Id = AlphaFloor1Id,
                    BuildingId = AlphaBuildingId,
                    Number = 1,
                    BlocksCount = 2,
                },
                new Floor
                {
                    Id = AlphaFloor2Id,
                    BuildingId = AlphaBuildingId,
                    Number = 2,
                    BlocksCount = 2,
                },
                new Floor
                {
                    Id = AlphaFloor3Id,
                    BuildingId = AlphaBuildingId,
                    Number = 3,
                    BlocksCount = 2,
                },
                // Beta Building floors
                new Floor
                {
                    Id = BetaFloor1Id,
                    BuildingId = BetaBuildingId,
                    Number = 1,
                    BlocksCount = 2,
                },
                new Floor
                {
                    Id = BetaFloor2Id,
                    BuildingId = BetaBuildingId,
                    Number = 2,
                    BlocksCount = 2,
                },
                new Floor
                {
                    Id = BetaFloor3Id,
                    BuildingId = BetaBuildingId,
                    Number = 3,
                    BlocksCount = 2,
                },
            };

            dbContext.Floors.AddRange(floors);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedBlocksAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            if (await dbContext.Blocks.AnyAsync(cancellationToken))
            {
                return;
            }

            var floors = new[] { AlphaFloor1Id, AlphaFloor2Id, AlphaFloor3Id, BetaFloor1Id, BetaFloor2Id, BetaFloor3Id };
            var blocks = new List<Block>();

            foreach (var floorId in floors)
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
                    Id = AlphaBuildingId,
                    Name = "Alpha Building",
                    Address = "123 Alpha Street",
                    FloorsCount = 3,
                    YearBuilt = 2010,
                    AdministratorContact = "admin@alpha.com",
                    IsActive = true,
                },
                new Building
                {
                    Id = BetaBuildingId,
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
                .Where(r => r.RoomType == RoomType.Regular) // Only create places for regular rooms
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

            var blocks = await dbContext.Blocks
                .Include(b => b.Floor)
                .ToListAsync(cancellationToken);

            var rooms = new List<Room>();

            // Group blocks by floor for proper room numbering
            var blocksByFloor = blocks.GroupBy(b => new { b.Floor.BuildingId, b.Floor.Number });

            foreach (var floorGroup in blocksByFloor)
            {
                var floorNumber = floorGroup.Key.Number;
                var blocksOnFloor = floorGroup.ToList();

                int roomCounter = 1;

                // Rooms inside blocks with proper numbering (floor + room number)
                foreach (var block in blocksOnFloor)
                {
                    for (int i = 1; i <= 3; i++) // 3 rooms per block
                    {
                        var roomNumber = (floorNumber * 100) + roomCounter;

                        rooms.Add(new Room
                        {
                            Id = Guid.NewGuid(),
                            BlockId = block.Id,
                            Label = roomNumber.ToString(),
                            Capacity = 2,
                            Status = RoomStatus.Available,
                            RoomType = RoomType.Regular,
                            Amenities = new Collection<string> { "WiFi", "Desk" },
                        });

                        roomCounter++;
                    }
                }
            }

            // Specialized rooms with predefined IDs
            var specializedRooms = new List<Room>
            {
                // Alpha Building specialized rooms
                new Room
                {
                    Id = StudyRoomId,
                    BlockId = null,
                    FloorId = AlphaFloor1Id,
                    Label = "Study Room",
                    Capacity = 10,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Study Room",
                    Amenities = new Collection<string> { "WiFi", "Comfortable Seating", "Whiteboards" },
                },
                new Room
                {
                    Id = LaundryRoomId,
                    BlockId = null,
                    FloorId = AlphaFloor2Id,
                    Label = "Laundry Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Laundry Room",
                    Amenities = new Collection<string> { "WiFi", "Washing Machines", "Dryers" },
                },
                new Room
                {
                    Id = StudyRoom2Id,
                    BlockId = null,
                    FloorId = AlphaFloor3Id,
                    Label = "Study Room",
                    Capacity = 10,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Study Room",
                    Amenities = new Collection<string> { "WiFi", "Comfortable Seating", "Whiteboards" },
                },
                // Beta Building specialized rooms
                new Room
                {
                    Id = CommonRoomId,
                    BlockId = null,
                    FloorId = BetaFloor1Id,
                    Label = "Common Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Common Room",
                    Amenities = new Collection<string> { "WiFi", "Comfortable Seating", "TV", "Games" },
                },
                new Room
                {
                    Id = LaundryRoom2Id,
                    BlockId = null,
                    FloorId = BetaFloor2Id,
                    Label = "Laundry Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Laundry Room",
                    Amenities = new Collection<string> { "WiFi", "Washing Machines", "Dryers" },
                },
                new Room
                {
                    Id = CommonRoom2Id,
                    BlockId = null,
                    FloorId = BetaFloor3Id,
                    Label = "Common Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Common Room",
                    Amenities = new Collection<string> { "WiFi", "Comfortable Seating", "TV", "Games" },
                },
            };

            rooms.AddRange(specializedRooms);
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