using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Features.Events;
using Events.API.Services;
using Events.Tests.Helpers;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Shared.UserServiceClient;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class GetEventHandlerTests
{
    private static GetEvent.Handler BuildHandler(
        EventsDbContext db,
        ParticipantEnricher? participantEnricher = null)
    {
        participantEnricher ??= CreateDefaultParticipantEnricher();

        return new GetEvent.Handler(db, participantEnricher);
    }

    private static ParticipantEnricher CreateDefaultParticipantEnricher()
    {
        var authServiceMock = new Mock<IAuthServiceClient>();
        var loggerMock = new Mock<ILogger<ParticipantEnricher>>();
        return new ParticipantEnricher(authServiceMock.Object, loggerMock.Object);
    }

    private static GetEvent.Query CreateQuery(Guid eventId) => new ()
    {
        Id = eventId,
    };

    [Fact]
    public async Task Handle_Should_ReturnEventDetails_When_EventExists()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        testEvent.Name = "Test Event Name";
        testEvent.Description = "Test Event Description";
        testEvent.Location = "Test Location";
        testEvent.NumberOfAttendees = 50;
        testEvent.BuildingId = Guid.NewGuid();
        testEvent.RoomId = Guid.NewGuid();

        await db.Events.AddAsync(testEvent);
        await db.SaveChangesAsync();

        // Create a ParticipantEnricher with mocked auth service that returns empty results
        var authServiceMock = new Mock<IAuthServiceClient>();
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, UserDto>());

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        var eventDetails = result.Value;

        Assert.Equal(testEvent.Id, eventDetails.Id);
        Assert.Equal(testEvent.OwnerId, eventDetails.OwnerId);
        Assert.Equal(testEvent.Name, eventDetails.Name);
        Assert.Equal(testEvent.Date, eventDetails.Date);
        Assert.Equal(testEvent.Location, eventDetails.Location);
        Assert.Equal(testEvent.Description, eventDetails.Description);
        Assert.Equal(testEvent.NumberOfAttendees, eventDetails.NumberOfAttendees);
        Assert.Equal(testEvent.IsPublic, eventDetails.IsPublic);
        Assert.Equal(testEvent.BuildingId, eventDetails.BuildingId);
        Assert.Equal(testEvent.RoomId, eventDetails.RoomId);
        Assert.Equal(0, eventDetails.CurrentParticipantsCount);
        Assert.Empty(eventDetails.Participants);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var nonExistentEventId = Guid.NewGuid();
        var query = CreateQuery(nonExistentEventId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Event.NotFound", result.FirstError.Code);
        Assert.Equal("The specified event was not found.", result.FirstError.Description);
    }

    [Fact]
    public async Task Handle_Should_CalculateCorrectParticipantCount_When_EventHasParticipants()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        // Add 3 participants
        var participant1 = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        var participant2 = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        var participant3 = TestDataBuilder.CreateEventParticipant(testEvent.Id);

        await db.EventParticipants.AddRangeAsync(participant1, participant2, participant3);
        await db.SaveChangesAsync();

        // Mock auth service to return user data for all participants
        var authServiceMock = new Mock<IAuthServiceClient>();
        var userDict = new Dictionary<Guid, UserDto>
        {
            { participant1.UserId, new UserDto { Id = participant1.UserId, FirstName = "User1", LastName = "Last1", Email = "user1@test.com" } },
            { participant2.UserId, new UserDto { Id = participant2.UserId, FirstName = "User2", LastName = "Last2", Email = "user2@test.com" } },
            { participant3.UserId, new UserDto { Id = participant3.UserId, FirstName = "User3", LastName = "Last3", Email = "user3@test.com" } },
        };
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(userDict);

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.CurrentParticipantsCount);
        Assert.Equal(3, result.Value.Participants.Count);
    }

    [Fact]
    public async Task Handle_Should_CallParticipantEnricher_When_EventHasParticipants()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        var participant1 = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        var participant2 = TestDataBuilder.CreateEventParticipant(testEvent.Id);

        await db.EventParticipants.AddRangeAsync(participant1, participant2);
        await db.SaveChangesAsync();

        // Mock auth service to return user data
        var authServiceMock = new Mock<IAuthServiceClient>();
        var userDict = new Dictionary<Guid, UserDto>
        {
            { participant1.UserId, new UserDto { Id = participant1.UserId, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" } },
            { participant2.UserId, new UserDto { Id = participant2.UserId, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com" } },
        };
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(userDict);

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        // Verify that auth service was called with correct user IDs
        authServiceMock.Verify(
            x => x.GetUsersByIdsAsync(It.Is<IEnumerable<Guid>>(userIds =>
                userIds.Contains(participant1.UserId) && userIds.Contains(participant2.UserId))),
            Times.Once);

        // Verify that enriched participants are returned with correct data
        Assert.Equal(2, result.Value.Participants.Count);
        var enrichedParticipants = result.Value.Participants.ToList();

        var johnParticipant = enrichedParticipants.Single(p => p.UserId == participant1.UserId);
        Assert.Equal("John", johnParticipant.FirstName);
        Assert.Equal("Doe", johnParticipant.LastName);
        Assert.Equal("john.doe@example.com", johnParticipant.Email);

        var janeParticipant = enrichedParticipants.Single(p => p.UserId == participant2.UserId);
        Assert.Equal("Jane", janeParticipant.FirstName);
        Assert.Equal("Smith", janeParticipant.LastName);
        Assert.Equal("jane.smith@example.com", janeParticipant.Email);
    }

    [Fact]
    public async Task Handle_Should_OrderParticipantsByJoinedAtDescending_When_EventHasParticipants()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        var firstParticipant = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        firstParticipant.JoinedAt = DateTime.UtcNow.AddDays(-2);

        var secondParticipant = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        secondParticipant.JoinedAt = DateTime.UtcNow.AddDays(-1);

        var thirdParticipant = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        thirdParticipant.JoinedAt = DateTime.UtcNow;

        await db.EventParticipants.AddRangeAsync(firstParticipant, secondParticipant, thirdParticipant);
        await db.SaveChangesAsync();

        // Mock auth service to return user data
        var authServiceMock = new Mock<IAuthServiceClient>();
        var userDict = new Dictionary<Guid, UserDto>
        {
            { firstParticipant.UserId, new UserDto { Id = firstParticipant.UserId, FirstName = "First", LastName = "User", Email = "first@test.com" } },
            { secondParticipant.UserId, new UserDto { Id = secondParticipant.UserId, FirstName = "Second", LastName = "User", Email = "second@test.com" } },
            { thirdParticipant.UserId, new UserDto { Id = thirdParticipant.UserId, FirstName = "Third", LastName = "User", Email = "third@test.com" } },
        };
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(userDict);

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        var participants = result.Value.Participants.ToList();
        Assert.Equal(3, participants.Count);

        // Verify participants are ordered by JoinedAt descending (most recent first)
        Assert.Equal(thirdParticipant.UserId, participants[0].UserId);
        Assert.Equal(secondParticipant.UserId, participants[1].UserId);
        Assert.Equal(firstParticipant.UserId, participants[2].UserId);
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyParticipantsList_When_EventHasNoParticipants()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, UserDto>());

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(0, result.Value.CurrentParticipantsCount);
        Assert.Empty(result.Value.Participants);

        // Auth service should NOT be called when there are no participants
        // The ParticipantEnricher returns empty list immediately if no participants
        authServiceMock.Verify(
            x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ReturnPrivateEventDetails_When_EventIsPrivate()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var privateEvent = TestDataBuilder.CreatePrivateEvent();
        await db.Events.AddAsync(privateEvent);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, UserDto>());

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(privateEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsPublic);
        Assert.Equal(privateEvent.Id, result.Value.Id);
        Assert.Equal(privateEvent.OwnerId, result.Value.OwnerId);
    }

    [Fact]
    public async Task Handle_Should_HandleNullOptionalFields_When_EventHasNullValues()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        testEvent.NumberOfAttendees = null;
        testEvent.BuildingId = null;
        testEvent.RoomId = null;

        await db.Events.AddAsync(testEvent);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(new Dictionary<Guid, UserDto>());

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        var eventDetails = result.Value;

        Assert.Null(eventDetails.NumberOfAttendees);
        Assert.Null(eventDetails.BuildingId);
        Assert.Null(eventDetails.RoomId);
    }

    [Fact]
    public async Task Handle_Should_ReturnEventWithEnrichedParticipantDetails_When_ParticipantEnricherSucceeds()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        var participant = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        await db.EventParticipants.AddAsync(participant);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        var userDict = new Dictionary<Guid, UserDto>
        {
            { participant.UserId, new UserDto { Id = participant.UserId, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com" } },
        };
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(userDict);

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        var eventDetails = result.Value;

        Assert.Single(eventDetails.Participants);
        var returnedParticipant = eventDetails.Participants.First();

        Assert.Equal(participant.UserId, returnedParticipant.UserId);
        Assert.Equal(participant.JoinedAt, returnedParticipant.JoinedAt);
        Assert.Equal("John", returnedParticipant.FirstName);
        Assert.Equal("Doe", returnedParticipant.LastName);
        Assert.Equal("john.doe@example.com", returnedParticipant.Email);
    }

    [Fact]
    public async Task Handle_Should_HandleEmptyGuid_When_EventIdIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var query = CreateQuery(Guid.Empty);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Event.NotFound", result.FirstError.Code);
    }

    [Fact]
    public async Task Handle_Should_PassCorrectParticipantDataToEnricher_When_EventHasParticipants()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        var specificUserId = Guid.NewGuid();
        var specificJoinedAt = DateTime.UtcNow.AddMinutes(-30);

        var participant = TestDataBuilder.CreateEventParticipant(testEvent.Id, specificUserId);
        participant.JoinedAt = specificJoinedAt;

        await db.EventParticipants.AddAsync(participant);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        var userDict = new Dictionary<Guid, UserDto>
        {
            { specificUserId, new UserDto { Id = specificUserId, FirstName = "Test", LastName = "User", Email = "test@example.com" } },
        };

        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(userDict);

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Single(result.Value.Participants);

        var returnedParticipant = result.Value.Participants.First();
        Assert.Equal(specificUserId, returnedParticipant.UserId);
        Assert.Equal(specificJoinedAt, returnedParticipant.JoinedAt);

        // Verify auth service was called with correct user ID
        authServiceMock.Verify(
            x => x.GetUsersByIdsAsync(It.Is<IEnumerable<Guid>>(userIds => userIds.Contains(specificUserId))),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnParticipantsWithoutUserData_When_AuthServiceFails()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var testEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(testEvent);

        var participant = TestDataBuilder.CreateEventParticipant(testEvent.Id);
        await db.EventParticipants.AddAsync(participant);
        await db.SaveChangesAsync();

        var authServiceMock = new Mock<IAuthServiceClient>();
        authServiceMock
            .Setup(x => x.GetUsersByIdsAsync(It.IsAny<IEnumerable<Guid>>()))
            .ReturnsAsync(Error.Failure("Auth.ServiceUnavailable", "Auth service is currently unavailable"));

        var participantEnricher = new ParticipantEnricher(authServiceMock.Object, new NullLogger<ParticipantEnricher>());
        var handler = BuildHandler(db, participantEnricher);
        var query = CreateQuery(testEvent.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(1, result.Value.CurrentParticipantsCount);

        // Should return empty participants list when enrichment fails
        Assert.Empty(result.Value.Participants);
    }
}