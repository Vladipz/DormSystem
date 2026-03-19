using ErrorOr;

using Events.API.Database;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class GenerateEventInvitationHandlerTests
{
    private static GenerateEventInvitation.Handler BuildHandler(EventsDbContext db) =>
        new GenerateEventInvitation.Handler(db, new GenerateEventInvitation.Validator());

    [Fact]
    public async Task PublicEvent_ReturnsEmptyToken()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = ev.Id,
            OwnerId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(string.Empty, result.Value.Token);
    }

    [Fact]
    public async Task PrivateEvent_Owner_ReturnsNonEmptyToken()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePrivateEvent(ownerId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = ev.Id,
            OwnerId = ownerId,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotEmpty(result.Value.Token);
    }

    [Fact]
    public async Task PrivateEvent_Owner_TokenPersistedToDatabase()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePrivateEvent(ownerId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = ev.Id,
            OwnerId = ownerId,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        var saved = db.InvitationTokens.FirstOrDefault(t => t.Token == result.Value.Token);
        Assert.NotNull(saved);
        Assert.Equal(ev.Id, saved.EventId);
    }

    [Fact]
    public async Task PrivateEvent_Owner_TokenExpiresInThirtyDays()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePrivateEvent(ownerId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var before = DateTime.UtcNow;
        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = ev.Id,
            OwnerId = ownerId,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        var expectedExpiry = before.AddDays(30);
        Assert.True(result.Value.ExpiresAt >= expectedExpiry.AddSeconds(-5));
        Assert.True(result.Value.ExpiresAt <= expectedExpiry.AddSeconds(5));
    }

    [Fact]
    public async Task PrivateEvent_NonOwner_ReturnsForbidden()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePrivateEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = ev.Id,
            OwnerId = Guid.NewGuid(), // not the owner
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Forbidden, result.FirstError.Type);
    }

    [Fact]
    public async Task EventNotFound_ReturnsNotFound()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new GenerateEventInvitation.Command
        {
            EventId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }
}