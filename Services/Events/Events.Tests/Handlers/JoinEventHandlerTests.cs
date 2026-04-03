using ErrorOr;

using Events.API.Database;
using Events.API.Entities;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class JoinEventHandlerTests
{
    private static JoinEvent.Handler BuildHandler(EventsDbContext db) =>
        new JoinEvent.Handler(db, new JoinEvent.Validator());

    [Fact]
    public async Task PublicEvent_ValidUser_ReturnsSuccess()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task PublicEvent_AddsParticipantToDatabase()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();
        var userId = Guid.NewGuid();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command { EventId = ev.Id, UserId = userId };

        await handler.Handle(command, CancellationToken.None);

        var participant = db.EventParticipants
            .FirstOrDefault(p => p.EventId == ev.Id && p.UserId == userId);
        Assert.NotNull(participant);
    }

    [Fact]
    public async Task PrivateEvent_ValidActiveToken_ReturnsSuccess()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePrivateEvent();
        db.Events.Add(ev);
        var token = TestDataBuilder.CreateActiveToken(ev.Id);
        db.InvitationTokens.Add(token);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            Token = token.Token,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
    }

    [Fact]
    public async Task PrivateEvent_ExpiredToken_ReturnsNotFound()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePrivateEvent();
        db.Events.Add(ev);
        var token = TestDataBuilder.CreateExpiredToken(ev.Id);
        db.InvitationTokens.Add(token);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            Token = token.Token,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task PrivateEvent_InactiveToken_ReturnsNotFound()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePrivateEvent();
        db.Events.Add(ev);
        var token = TestDataBuilder.CreateActiveToken(ev.Id);
        token.IsActive = false;
        db.InvitationTokens.Add(token);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            Token = token.Token,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task PrivateEvent_NoToken_ReturnsValidationError()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePrivateEvent();
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            Token = null,
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
    }

    [Fact]
    public async Task EventNotFound_ReturnsNotFound()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
    }

    [Fact]
    public async Task AlreadyJoined_ReturnsConflict()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        db.Events.Add(ev);
        var userId = Guid.NewGuid();
        db.EventParticipants.Add(new EventParticipant
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command { EventId = ev.Id, UserId = userId };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Conflict, result.FirstError.Type);
    }

    [Fact]
    public async Task EventFull_ReturnsFailure()
    {
        var db = EventsDbContextFactory.Create();
        var ev = TestDataBuilder.CreatePublicEvent();
        ev.NumberOfAttendees = 1;
        db.Events.Add(ev);
        db.EventParticipants.Add(new EventParticipant
        {
            Id = Guid.NewGuid(),
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
            JoinedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new JoinEvent.Command
        {
            EventId = ev.Id,
            UserId = Guid.NewGuid(),
        };

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Failure, result.FirstError.Type);
    }
}