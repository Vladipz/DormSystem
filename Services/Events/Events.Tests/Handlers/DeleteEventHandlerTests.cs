using ErrorOr;

using Events.API.Database;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class DeleteEventHandlerTests
{
    private static DeleteEvent.Handler BuildHandler(EventsDbContext db) =>
        new DeleteEvent.Handler(db);

    [Fact]
    public async Task ExistingEvent_ReturnsTrue()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new DeleteEvent.Command { Id = ev.Id };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ExistingEvent_RemovedFromDatabase()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new DeleteEvent.Command { Id = ev.Id };

        await handler.Handle(command, CancellationToken.None);

        var deleted = await db.Events.FindAsync(ev.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task EventNotFound_ReturnsNotFound()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new DeleteEvent.Command { Id = Guid.NewGuid() };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }
}