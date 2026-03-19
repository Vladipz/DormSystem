using ErrorOr;

using Events.API.Database;
using Events.API.Entities;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class AddParticipantHandlerTests
{
    private static AddParticipant.Handler BuildHandler(EventsDbContext db) =>
        new AddParticipant.Handler(db, new AddParticipant.Validator());

    [Fact]
    public async Task OwnerAddsUser_ReturnsSuccess()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePublicEvent(ownerId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new AddParticipant.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            RequesterId = ownerId,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task NonOwner_ReturnsForbidden()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new AddParticipant.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(), // not the owner
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
        var command = new AddParticipant.Command
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RequesterId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task DuplicateParticipant_ReturnsConflict()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePublicEvent(ownerId);
        db.Events.Add(ev);
        db.EventParticipants.Add(new EventParticipant
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new AddParticipant.Command
        {
            EventId = ev.Id,
            UserId = userId,
            RequesterId = ownerId,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }

    [Fact]
    public async Task OwnerAddsUser_ParticipantPersistedToDatabase()
    {
        var db = EventsDbContextFactory.Create();
        var ownerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var ev = TestDataBuilder.CreatePublicEvent(ownerId);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new AddParticipant.Command
        {
            EventId = ev.Id,
            UserId = userId,
            RequesterId = ownerId,
        };

        await handler.Handle(command, CancellationToken.None);

        var participant = db.EventParticipants
            .FirstOrDefault(p => p.EventId == ev.Id && p.UserId == userId);
        Assert.NotNull(participant);
    }
}