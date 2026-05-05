using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Events.API.Database
{
    public sealed class EventsDbContextFactory : IDesignTimeDbContextFactory<EventsDbContext>
    {
        public EventsDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__events-db")
                ?? "Host=localhost;Port=5432;Database=events-db;Username=postgres;Password=postgres";

            var optionsBuilder = new DbContextOptionsBuilder<EventsDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new EventsDbContext(optionsBuilder.Options);
        }
    }
}
