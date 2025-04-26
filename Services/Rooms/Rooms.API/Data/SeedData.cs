using Microsoft.EntityFrameworkCore;

using Rooms.API.Entities;

namespace Rooms.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dbContext);

            // Apply any pending migrations
            await dbContext.Database.MigrateAsync(cancellationToken);

            if (!await dbContext.Rooms.AnyAsync(cancellationToken))
            {
                var rooms = new List<Room>
                {
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Label = "Room A101",
                        Capacity = 2,
                        Status = RoomStatus.Available,
                        RoomType = RoomType.Regular,
                        Amenities =
                            ["WiFi", "Desk", "Closet"],
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Label = "Study Room 1",
                        Capacity = 8,
                        Status = RoomStatus.Available,
                        RoomType = RoomType.Specialized,
                        Purpose = "Group Study",
                        Amenities =
                            ["Whiteboard", "Projector"],
                    },
                    new Room
                    {
                        Id = Guid.NewGuid(),
                        Label = "Laundry Room",
                        Capacity = 1,
                        Status = RoomStatus.Maintenance,
                        RoomType = RoomType.Specialized,
                        Purpose = "Laundry Machines",
                        Amenities =
                            ["Washing Machine", "Dryer"],
                    },
                };

                dbContext.Rooms.AddRange(rooms);

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}