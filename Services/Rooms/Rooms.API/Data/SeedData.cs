using Microsoft.EntityFrameworkCore;

using Rooms.API.Entities;

namespace Rooms.API.Data
{
    public static class SeedData
    {
        private const int RoomsPerBlock = 3;
        private const int TargetMaintenanceTicketCount = 420;
        private const int MaintenanceSeedDays = 365;

        // Static GUIDs for seeding user references
        private static readonly Guid ReporterUser1Id = new Guid("11111111-1111-1111-1111-111111111111");
        private static readonly Guid ReporterUser2Id = new Guid("22222222-2222-2222-2222-222222222222");
        private static readonly Guid MaintenanceStaffId = new Guid("33333333-3333-3333-3333-333333333333");
        private static readonly Guid TestStudentOneId = new Guid("44444444-4444-4444-4444-444444444444");
        private static readonly Guid TestStudentTwoId = new Guid("55555555-5555-5555-5555-555555555555");

        // Predefined GUIDs for buildings
        private static readonly Guid AlphaBuildingId = new Guid("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        private static readonly Guid BetaBuildingId = new Guid("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");

        // Predefined GUIDs for floors
        private static readonly Guid AlphaFloor1Id = new Guid("f1111111-1111-1111-1111-111111111111");
        private static readonly Guid AlphaFloor2Id = new Guid("f2222222-2222-2222-2222-222222222222");
        private static readonly Guid AlphaFloor3Id = new Guid("f3333333-3333-3333-3333-333333333333");
        private static readonly Guid AlphaFloor4Id = new Guid("f7777777-7777-7777-7777-777777777777");
        private static readonly Guid AlphaFloor5Id = new Guid("f8888888-8888-8888-8888-888888888888");
        private static readonly Guid AlphaFloor6Id = new Guid("f9999999-9999-9999-9999-999999999999");
        private static readonly Guid AlphaFloor7Id = new Guid("faaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid BetaFloor1Id = new Guid("f4444444-4444-4444-4444-444444444444");
        private static readonly Guid BetaFloor2Id = new Guid("f5555555-5555-5555-5555-555555555555");
        private static readonly Guid BetaFloor3Id = new Guid("f6666666-6666-6666-6666-666666666666");
        private static readonly Guid BetaFloor4Id = new Guid("fbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid BetaFloor5Id = new Guid("fccccccc-cccc-cccc-cccc-cccccccccccc");

        // Predefined GUIDs for specialized rooms (one per floor)
        private static readonly Guid StudyRoomId = new Guid("cccccccc-dddd-eeee-ffff-aaaaaaaaaaaa");
        private static readonly Guid LaundryRoomId = new Guid("eeeeeeee-ffff-aaaa-bbbb-cccccccccccc");
        private static readonly Guid CommonRoomId = new Guid("dddddddd-eeee-ffff-aaaa-bbbbbbbbbbbb");
        private static readonly Guid StudyRoom2Id = new Guid("cccccccc-dddd-eeee-ffff-bbbbbbbbbbbb");
        private static readonly Guid LaundryRoom2Id = new Guid("eeeeeeee-ffff-aaaa-bbbb-dddddddddddd");
        private static readonly Guid CommonRoom2Id = new Guid("dddddddd-eeee-ffff-aaaa-cccccccccccc");

        private static readonly Guid[] ReporterUserIds =
        [
            ReporterUser1Id,
            ReporterUser2Id,
            TestStudentOneId,
            TestStudentTwoId,
        ];

        private static readonly BuildingSeedSpec[] BuildingSpecs =
        [
            new BuildingSeedSpec(
                AlphaBuildingId,
                "Alpha Building",
                "123 Alpha Street",
                floorsCount: 7,
                blocksPerFloor: 6,
                yearBuilt: 2010,
                administratorContact: "admin@alpha.com",
                [
                    AlphaFloor1Id,
                    AlphaFloor2Id,
                    AlphaFloor3Id,
                    AlphaFloor4Id,
                    AlphaFloor5Id,
                    AlphaFloor6Id,
                    AlphaFloor7Id,
                ]),
            new BuildingSeedSpec(
                BetaBuildingId,
                "Beta Building",
                "456 Beta Avenue",
                floorsCount: 5,
                blocksPerFloor: 5,
                yearBuilt: 2015,
                administratorContact: "admin@beta.com",
                [
                    BetaFloor1Id,
                    BetaFloor2Id,
                    BetaFloor3Id,
                    BetaFloor4Id,
                    BetaFloor5Id,
                ]),
        ];

        private static readonly (string Title, string Description)[] MaintenanceTemplates =
        [
            ("Leaking faucet", "Bathroom faucet is leaking and needs plumbing service."),
            ("Heating issue", "Room temperature is below normal and the radiator needs inspection."),
            ("Broken outlet", "Electrical outlet is not working and should be checked by maintenance."),
            ("Window repair", "Window does not close properly and causes drafts in the room."),
            ("Door lock problem", "Door lock is hard to open and may need replacement."),
            ("Furniture repair", "Bed frame or desk is damaged and needs repair."),
            ("Wi-Fi signal problem", "Residents report unstable Wi-Fi signal in this room."),
            ("Ceiling leak", "Water stains appeared on the ceiling after rain."),
            ("Light fixture failure", "Main light fixture is not working."),
            ("Sanitation request", "Room requires maintenance support for sanitation-related issue."),
        ];

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
            foreach (var spec in BuildingSpecs)
            {
                for (var floorNumber = 1; floorNumber <= spec.FloorsCount; floorNumber++)
                {
                    var floorId = spec.FloorIds[floorNumber - 1];
                    var floor = await dbContext.Floors
                        .FirstOrDefaultAsync(
                            f => f.Id == floorId || (f.BuildingId == spec.BuildingId && f.Number == floorNumber),
                            cancellationToken);

                    if (floor is null)
                    {
                        dbContext.Floors.Add(new Floor
                        {
                            Id = floorId,
                            BuildingId = spec.BuildingId,
                            Number = floorNumber,
                            BlocksCount = spec.BlocksPerFloor,
                        });

                        continue;
                    }

                    floor.BlocksCount = spec.BlocksPerFloor;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedBlocksAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var seededBuildingIds = BuildingSpecs.Select(x => x.BuildingId).ToArray();
            var floors = await dbContext.Floors
                .Include(f => f.Blocks)
                .Where(f => seededBuildingIds.Contains(f.BuildingId))
                .ToListAsync(cancellationToken);

            foreach (var spec in BuildingSpecs)
            {
                foreach (var floor in floors.Where(f => f.BuildingId == spec.BuildingId && f.Number <= spec.FloorsCount))
                {
                    var existingLabels = floor.Blocks
                        .Select(b => b.Label)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    for (var blockNumber = 1; blockNumber <= spec.BlocksPerFloor; blockNumber++)
                    {
                        var label = $"Block {blockNumber}";
                        if (existingLabels.Contains(label))
                        {
                            continue;
                        }

                        dbContext.Blocks.Add(new Block
                        {
                            Id = Guid.NewGuid(),
                            FloorId = floor.Id,
                            Label = label,
                            GenderRule = blockNumber % 2 == 0 ? "Female" : "Male",
                        });
                    }
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedBuildingsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            foreach (var spec in BuildingSpecs)
            {
                var building = await dbContext.Buildings
                    .FirstOrDefaultAsync(b => b.Id == spec.BuildingId, cancellationToken);

                if (building is null)
                {
                    dbContext.Buildings.Add(new Building
                    {
                        Id = spec.BuildingId,
                        Name = spec.Name,
                        Address = spec.Address,
                        FloorsCount = spec.FloorsCount,
                        YearBuilt = spec.YearBuilt,
                        AdministratorContact = spec.AdministratorContact,
                        IsActive = true,
                    });

                    continue;
                }

                building.FloorsCount = spec.FloorsCount;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedPlacesAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var rooms = await dbContext.Rooms
                .Include(r => r.Places)
                .Where(r => r.RoomType == RoomType.Regular)
                .ToListAsync(cancellationToken);

            foreach (var room in rooms)
            {
                var existingIndexes = room.Places.Select(p => p.Index).ToHashSet();
                for (var index = 1; index <= room.Capacity; index++)
                {
                    if (existingIndexes.Contains(index))
                    {
                        continue;
                    }

                    dbContext.Places.Add(new Place
                    {
                        Id = Guid.NewGuid(),
                        RoomId = room.Id,
                        Index = index,
                    });
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedRoomsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var seededBuildingIds = BuildingSpecs.Select(x => x.BuildingId).ToArray();
            var blocks = (await dbContext.Blocks
                .Include(b => b.Floor)
                .Include(b => b.Rooms)
                .Where(b => seededBuildingIds.Contains(b.Floor.BuildingId))
                .ToListAsync(cancellationToken))
                .OrderBy(b => b.Floor.BuildingId)
                .ThenBy(b => b.Floor.Number)
                .ThenBy(b => GetBlockNumber(b.Label))
                .ToList();

            var blocksByFloor = blocks.GroupBy(b => new { b.Floor.BuildingId, b.Floor.Number });

            foreach (var floorGroup in blocksByFloor)
            {
                var nextRoomCounter = GetNextRoomCounter(
                    floorGroup.Key.Number,
                    floorGroup.SelectMany(b => b.Rooms).Where(r => r.RoomType == RoomType.Regular));

                foreach (var block in floorGroup)
                {
                    var regularRoomCount = block.Rooms.Count(r => r.RoomType == RoomType.Regular);
                    for (var i = regularRoomCount; i < RoomsPerBlock; i++)
                    {
                        dbContext.Rooms.Add(new Room
                        {
                            Id = Guid.NewGuid(),
                            BlockId = block.Id,
                            FloorId = block.FloorId,
                            BuildingId = block.Floor.BuildingId,
                            Label = ((floorGroup.Key.Number * 100) + nextRoomCounter).ToString(),
                            Capacity = 2,
                            Status = RoomStatus.Available,
                            RoomType = RoomType.Regular,
                            Amenities = new List<string> { "WiFi", "Desk" },
                        });

                        nextRoomCounter++;
                    }
                }
            }

            var existingSpecializedRoomIds = await dbContext.Rooms
                .Where(r => r.RoomType == RoomType.Specialized)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);
            var existingSpecializedRoomIdsSet = existingSpecializedRoomIds.ToHashSet();

            foreach (var room in GetSpecializedRoomSeeds())
            {
                if (existingSpecializedRoomIdsSet.Contains(room.Id))
                {
                    continue;
                }

                dbContext.Rooms.Add(room);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedMaintenanceTicketsAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var existingCount = await dbContext.MaintenanceTickets.CountAsync(cancellationToken);
            if (existingCount >= TargetMaintenanceTicketCount)
            {
                return;
            }

            var sourceRooms = await dbContext.Rooms
                .AsNoTracking()
                .Include(r => r.Block!)
                    .ThenInclude(b => b.Floor)
                .Where(r => r.RoomType == RoomType.Regular && r.BlockId.HasValue)
                .ToListAsync(cancellationToken);

            var rooms = sourceRooms
                .Where(r => r.Block?.Floor is not null)
                .Select(r =>
                {
                    var blockNumber = GetBlockNumber(r.Block!.Label);
                    var weight = GetMaintenanceWeight(r.Block.Floor.BuildingId, r.Block.Floor.Number, blockNumber);

                    return new MaintenanceRoomSeedInfo(
                        r.Id,
                        r.Label,
                        r.Block.Floor.Number,
                        blockNumber,
                        weight);
                })
                .ToList();

            if (rooms.Count == 0)
            {
                return;
            }

            var weightedRooms = rooms
                .SelectMany(room => Enumerable.Repeat(room, room.Weight))
                .ToList();
            var ticketsToCreate = TargetMaintenanceTicketCount - existingCount;
            var startDate = DateTime.UtcNow.Date.AddDays(-MaintenanceSeedDays);
            var now = DateTime.UtcNow;
            var tickets = new List<MaintenanceTicket>(ticketsToCreate);

            for (var i = 0; i < ticketsToCreate; i++)
            {
                var sequence = existingCount + i;
                var room = weightedRooms[(sequence * 37) % weightedRooms.Count];
                var template = MaintenanceTemplates[(sequence + room.BlockNumber + room.FloorNumber) % MaintenanceTemplates.Length];
                var createdAt = startDate
                    .AddDays((sequence * 17) % MaintenanceSeedDays)
                    .AddHours((sequence * 7) % 24)
                    .AddMinutes((sequence * 13) % 60);
                var status = GetMaintenanceStatus(sequence, createdAt, now);
                var priority = GetMaintenancePriority(sequence, room.Weight);
                var isResolved = status == MaintenanceStatus.Resolved;

                tickets.Add(new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.RoomId,
                    Title = template.Title,
                    Description = $"{template.Description} Seeded yearly ticket for floor {room.FloorNumber}, block {room.BlockNumber}, room {room.RoomLabel}.",
                    Status = status,
                    CreatedAt = createdAt,
                    ResolvedAt = isResolved ? GetResolvedAt(sequence, createdAt, now) : null,
                    IsResolved = isResolved,
                    ReporterById = ReporterUserIds[sequence % ReporterUserIds.Length],
                    AssignedToId = status == MaintenanceStatus.Open && sequence % 3 == 0 ? null : MaintenanceStaffId,
                    Priority = priority,
                });
            }

            dbContext.MaintenanceTickets.AddRange(tickets);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static List<Room> GetSpecializedRoomSeeds()
        {
            return
            [
                new Room
                {
                    Id = StudyRoomId,
                    BlockId = null,
                    FloorId = AlphaFloor1Id,
                    BuildingId = AlphaBuildingId,
                    Label = "Study Room",
                    Capacity = 10,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Study Room",
                    Amenities = new List<string> { "WiFi", "Comfortable Seating", "Whiteboards" },
                },
                new Room
                {
                    Id = LaundryRoomId,
                    BlockId = null,
                    FloorId = AlphaFloor2Id,
                    BuildingId = AlphaBuildingId,
                    Label = "Laundry Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Laundry Room",
                    Amenities = new List<string> { "WiFi", "Washing Machines", "Dryers" },
                },
                new Room
                {
                    Id = StudyRoom2Id,
                    BlockId = null,
                    FloorId = AlphaFloor3Id,
                    BuildingId = AlphaBuildingId,
                    Label = "Study Room",
                    Capacity = 10,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Study Room",
                    Amenities = new List<string> { "WiFi", "Comfortable Seating", "Whiteboards" },
                },
                new Room
                {
                    Id = CommonRoomId,
                    BlockId = null,
                    FloorId = BetaFloor1Id,
                    BuildingId = BetaBuildingId,
                    Label = "Common Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Common Room",
                    Amenities = new List<string> { "WiFi", "Comfortable Seating", "TV", "Games" },
                },
                new Room
                {
                    Id = LaundryRoom2Id,
                    BlockId = null,
                    FloorId = BetaFloor2Id,
                    BuildingId = BetaBuildingId,
                    Label = "Laundry Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Laundry Room",
                    Amenities = new List<string> { "WiFi", "Washing Machines", "Dryers" },
                },
                new Room
                {
                    Id = CommonRoom2Id,
                    BlockId = null,
                    FloorId = BetaFloor3Id,
                    BuildingId = BetaBuildingId,
                    Label = "Common Room",
                    Capacity = 15,
                    Status = RoomStatus.Available,
                    RoomType = RoomType.Specialized,
                    Purpose = "Common Room",
                    Amenities = new List<string> { "WiFi", "Comfortable Seating", "TV", "Games" },
                },
            ];
        }

        private static int GetNextRoomCounter(int floorNumber, IEnumerable<Room> rooms)
        {
            var maxCounter = rooms
                .Select(room => int.TryParse(room.Label, out var labelNumber) ? labelNumber - (floorNumber * 100) : 0)
                .Where(counter => counter > 0)
                .DefaultIfEmpty(0)
                .Max();

            return maxCounter + 1;
        }

        private static int GetBlockNumber(string label)
        {
            var numberText = label.Replace("Block", string.Empty, StringComparison.OrdinalIgnoreCase).Trim();
            return int.TryParse(numberText, out var number) ? number : 1;
        }

        private static int GetMaintenanceWeight(Guid buildingId, int floorNumber, int blockNumber)
        {
            var weight = 1 + ((floorNumber + blockNumber) % 3);

            if (buildingId == AlphaBuildingId && floorNumber == 2 && (blockNumber == 3 || blockNumber == 4))
            {
                weight += 5;
            }

            if (buildingId == AlphaBuildingId && floorNumber == 5 && blockNumber == 6)
            {
                weight += 4;
            }

            if (buildingId == BetaBuildingId && floorNumber == 1 && blockNumber == 1)
            {
                weight += 3;
            }

            if (buildingId == BetaBuildingId && floorNumber == 4 && blockNumber == 5)
            {
                weight += 5;
            }

            return weight;
        }

        private static MaintenanceStatus GetMaintenanceStatus(int sequence, DateTime createdAt, DateTime now)
        {
            var statusBucket = sequence % 20;
            var status = statusBucket < 9
                ? MaintenanceStatus.Resolved
                : statusBucket < 15
                    ? MaintenanceStatus.Open
                    : MaintenanceStatus.InProgress;

            return status == MaintenanceStatus.Resolved && createdAt.AddDays(14) > now
                ? MaintenanceStatus.InProgress
                : status;
        }

        private static MaintenancePriority GetMaintenancePriority(int sequence, int roomWeight)
        {
            if (roomWeight >= 6 && sequence % 4 == 0)
            {
                return MaintenancePriority.Critical;
            }

            return (sequence % 20) switch
            {
                < 2 => MaintenancePriority.Critical,
                < 7 => MaintenancePriority.High,
                < 15 => MaintenancePriority.Medium,
                _ => MaintenancePriority.Low,
            };
        }

        private static DateTime GetResolvedAt(int sequence, DateTime createdAt, DateTime now)
        {
            var resolvedAt = createdAt.AddDays(1 + (sequence % 14)).AddHours(sequence % 8);
            return resolvedAt > now ? now.AddHours(-1) : resolvedAt;
        }

        private sealed class BuildingSeedSpec
        {
            public BuildingSeedSpec(
                Guid buildingId,
                string name,
                string address,
                int floorsCount,
                int blocksPerFloor,
                int yearBuilt,
                string administratorContact,
                Guid[] floorIds)
            {
                BuildingId = buildingId;
                Name = name;
                Address = address;
                FloorsCount = floorsCount;
                BlocksPerFloor = blocksPerFloor;
                YearBuilt = yearBuilt;
                AdministratorContact = administratorContact;
                FloorIds = floorIds;
            }

            public Guid BuildingId { get; }

            public string Name { get; }

            public string Address { get; }

            public int FloorsCount { get; }

            public int BlocksPerFloor { get; }

            public int YearBuilt { get; }

            public string AdministratorContact { get; }

            public Guid[] FloorIds { get; }
        }

        private sealed class MaintenanceRoomSeedInfo
        {
            public MaintenanceRoomSeedInfo(Guid roomId, string roomLabel, int floorNumber, int blockNumber, int weight)
            {
                RoomId = roomId;
                RoomLabel = roomLabel;
                FloorNumber = floorNumber;
                BlockNumber = blockNumber;
                Weight = weight;
            }

            public Guid RoomId { get; }

            public string RoomLabel { get; }

            public int FloorNumber { get; }

            public int BlockNumber { get; }

            public int Weight { get; }
        }
    }
}
