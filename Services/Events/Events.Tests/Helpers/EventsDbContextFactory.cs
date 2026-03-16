using Events.API.Database;

using Microsoft.EntityFrameworkCore;

namespace Events.Tests.Helpers;

internal static class EventsDbContextFactory
{
    // Unique DB name per call = no shared state between tests
    public static EventsDbContext Create()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EventsDbContext(options);
    }
}
