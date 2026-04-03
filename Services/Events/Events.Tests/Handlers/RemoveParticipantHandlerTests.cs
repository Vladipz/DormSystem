using ErrorOr;

using Events.API.Database;
using Events.API.Entities;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class RemoveParticipantHandlerTests
{
    private static RemoveParticipant.Handler BuildHandler(EventsDbContext db) =>
        new RemoveParticipant.Handler(db, new RemoveParticipant.Validator());

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_ParticipantExistsAndIsRemoved()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant = TestDataBuilder.CreateEventParticipant(eventId, userId);

        db.Events.Add(eventEntity);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = userId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Handle_Should_RemoveParticipantFromDatabase_When_ParticipantExists()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant = TestDataBuilder.CreateEventParticipant(eventId, userId);

        db.Events.Add(eventEntity);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        // Verify participant exists before removal
        var participantBeforeRemoval = await db.EventParticipants
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);
        Assert.NotNull(participantBeforeRemoval);

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = userId,
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var participantAfterRemoval = await db.EventParticipants
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);
        Assert.Null(participantAfterRemoval);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundError_When_ParticipantDoesNotExist()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var nonExistentUserId = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant = TestDataBuilder.CreateEventParticipant(eventId, userId);

        db.Events.Add(eventEntity);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = nonExistentUserId, // Different user ID that doesn't exist as participant
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Participant.NotFound", result.FirstError.Code);
        Assert.Equal("The specified participant was not found in this event.", result.FirstError.Description);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var nonExistentEventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = nonExistentEventId,
            UserId = userId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Participant.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundError_When_ParticipantExistsForDifferentEvent()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId1 = Guid.NewGuid();
        var eventId2 = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var event1 = TestDataBuilder.CreatePublicEvent();
        event1.Id = eventId1;
        var event2 = TestDataBuilder.CreatePublicEvent();
        event2.Id = eventId2;
        var participant = TestDataBuilder.CreateEventParticipant(eventId1, userId); // Participant for event1

        db.Events.AddRange(event1, event2);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId2, // Trying to remove from event2 where user is not a participant
            UserId = userId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Participant.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_EventIdIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = Guid.Empty, // Invalid EventId
            UserId = Guid.NewGuid(),
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("EventId", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_UserIdIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = Guid.NewGuid(),
            UserId = Guid.Empty, // Invalid UserId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("UserId", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_BothIdsAreEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = Guid.Empty, // Invalid EventId
            UserId = Guid.Empty,  // Invalid UserId
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);

        // Should have validation errors for both properties
        Assert.True(result.Errors.Count >= 2);
        Assert.Contains(result.Errors, e => e.Code == "EventId");
        Assert.Contains(result.Errors, e => e.Code == "UserId");
    }

    [Fact]
    public async Task Handle_Should_OnlyRemoveSpecifiedParticipant_When_MultipleParticipantsExist()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant1 = TestDataBuilder.CreateEventParticipant(eventId, userId1);
        var participant2 = TestDataBuilder.CreateEventParticipant(eventId, userId2);
        var participant3 = TestDataBuilder.CreateEventParticipant(eventId, userId3);

        db.Events.Add(eventEntity);
        db.EventParticipants.AddRange(participant1, participant2, participant3);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = userId2, // Remove only userId2
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);

        // Verify only the specified participant was removed
        var remainingParticipants = await db.EventParticipants
            .Where(p => p.EventId == eventId)
            .ToListAsync();

        Assert.Equal(2, remainingParticipants.Count);
        Assert.Contains(remainingParticipants, p => p.UserId == userId1);
        Assert.Contains(remainingParticipants, p => p.UserId == userId3);
        Assert.DoesNotContain(remainingParticipants, p => p.UserId == userId2);
    }

    [Fact]
    public async Task Handle_Should_HandleConcurrentRequests_When_MultipleRemoveOperations()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant = TestDataBuilder.CreateEventParticipant(eventId, userId);

        db.Events.Add(eventEntity);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = userId,
        };

        // Act - First removal should succeed
        var result1 = await handler.Handle(command, CancellationToken.None);

        // Act - Second removal should fail (participant already removed)
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result1.IsError);
        Assert.True(result1.Value);

        Assert.True(result2.IsError);
        Assert.Equal(ErrorType.NotFound, result2.FirstError.Type);
        Assert.Equal("Participant.NotFound", result2.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_SupportCancellation_When_CancellationTokenIsTriggered()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var eventEntity = TestDataBuilder.CreatePublicEvent();
        eventEntity.Id = eventId;
        var participant = TestDataBuilder.CreateEventParticipant(eventId, userId);

        db.Events.Add(eventEntity);
        db.EventParticipants.Add(participant);
        await db.SaveChangesAsync();

        var handler = BuildHandler(db);
        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = userId,
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Cancel immediately

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            async () => await handler.Handle(command, cts.Token));
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty as string
    [InlineData("")]
    public async Task Handle_Should_ReturnValidationError_When_EventIdIsInvalid(string invalidEventId)
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);

        // For empty string, we'll use Guid.Empty, for Guid.Empty string we parse it
        var eventId = string.IsNullOrEmpty(invalidEventId) ? Guid.Empty : Guid.Parse(invalidEventId);

        var command = new RemoveParticipant.Command
        {
            EventId = eventId,
            UserId = Guid.NewGuid(),
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("EventId", result.FirstError.Code);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")] // Guid.Empty as string
    public async Task Handle_Should_ReturnValidationError_When_UserIdIsInvalid(string invalidUserId)
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);

        var userId = Guid.Parse(invalidUserId);

        var command = new RemoveParticipant.Command
        {
            EventId = Guid.NewGuid(),
            UserId = userId,
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.Validation, result.FirstError.Type);
        Assert.Equal("UserId", result.FirstError.Code);
    }
}