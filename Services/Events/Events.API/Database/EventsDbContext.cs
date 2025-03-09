using Events.API.Entities;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Database
{
    public sealed class EventsDbContext : DbContext
    {
        public EventsDbContext(DbContextOptions<EventsDbContext> options)
            : base(options)
        {
        }

        public DbSet<DormEvent> Events { get; set; } = null!;
    }
}