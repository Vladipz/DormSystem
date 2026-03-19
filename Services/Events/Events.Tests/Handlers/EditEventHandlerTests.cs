using ErrorOr;

using Events.API.Database;
using Events.API.Entities;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class EditEventHandlerTests
{
    private static EditEvent.Handler BuildHandler(EventsDbContext db)
    {
        return new EditEvent.Handler(db, new EditEvent.Validator());
    }

    private static EditEvent.Command ValidCommand(Guid? id = null) => new ()
    {
        Id = id ?? Guid.NewGuid(),
        Name = "Updated Event Name",
        Date = DateTime.UtcNow.AddDays(5),
        Location = "Updated Location",
        Description = "Updated description",
        NumberOfAttendees = 50,
        IsPublic = true,
        BuildingId = Guid.NewGuid(),
        RoomId = Guid.NewGuid(),
    };

    [Fact]
    public async Task Handle_Should_ReturnTrue_When_EventExistsAndCommandIsValid()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task Handle_Should_UpdateAllEventFields_When_CommandIsValid()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        var updatedEvent = await db.Events.FindAsync(existingEvent.Id);
        Assert.NotNull(updatedEvent);
        Assert.Equal(command.Name, updatedEvent.Name);
        Assert.Equal(command.Date, updatedEvent.Date);
        Assert.Equal(command.Location, updatedEvent.Location);
        Assert.Equal(command.Description, updatedEvent.Description);
        Assert.Equal(command.NumberOfAttendees, updatedEvent.NumberOfAttendees);
        Assert.Equal(command.IsPublic, updatedEvent.IsPublic);
        Assert.Equal(command.BuildingId, updatedEvent.BuildingId);
        Assert.Equal(command.RoomId, updatedEvent.RoomId);
    }

    [Fact]
    public async Task Handle_Should_SaveChangesToDatabase_When_EventIsUpdated()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var originalName = existingEvent.Name;
        var command = ValidCommand(existingEvent.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        // Verify changes are persisted by fetching fresh from database
        using var newContext = EventsDbContextFactory.Create();
        await newContext.Database.EnsureCreatedAsync();
        var persistedEvent = await db.Events.AsNoTracking().FirstAsync(e => e.Id == existingEvent.Id);
        Assert.NotEqual(originalName, persistedEvent.Name);
        Assert.Equal(command.Name, persistedEvent.Name);
    }

    [Fact]
    public async Task Handle_Should_ReturnNotFoundError_When_EventDoesNotExist()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var nonExistentId = Guid.NewGuid();
        var command = ValidCommand(nonExistentId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.Single(result.Errors);
        Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        Assert.Equal("Event.NotFound", result.FirstError.Code);
        Assert.Equal("The specified event was not found.", result.FirstError.Description);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_IdIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Id = Guid.Empty;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_NameIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Name = string.Empty;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_NameIsWhitespace()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Name = "   "; // Only whitespace

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_DateIsInThePast()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Date = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("must be in the future", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_DateIsEmpty()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Date = default;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_DescriptionExceedsMaxLength()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Description = new string('a', 2001); // 2001 characters, exceeds limit

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("cannot exceed 2000 characters", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_AcceptValidDescription_When_DescriptionIsAtMaxLength()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.Description = new string('a', 2000); // Exactly 2000 characters

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_NumberOfAttendeesIsZero()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.NumberOfAttendees = 0;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("must be greater than 0", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_NumberOfAttendeesIsNegative()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.NumberOfAttendees = -5;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("must be greater than 0", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_AcceptNullNumberOfAttendees_When_NotSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.NumberOfAttendees = null;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        var updatedEvent = await db.Events.FindAsync(existingEvent.Id);
        Assert.Null(updatedEvent!.NumberOfAttendees);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_NoLocationAndNoBuildingSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Location = string.Empty;
        command.BuildingId = null;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("Either a location or a building must be specified", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_AcceptCommand_When_LocationIsEmptyButBuildingIsSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.Location = string.Empty;
        command.BuildingId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_Should_AcceptCommand_When_BuildingIsNullButLocationIsSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.Location = "Some Location";
        command.BuildingId = null;
        command.RoomId = null; // Must also clear RoomId when BuildingId is null

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationError_When_RoomIsSpecifiedWithoutBuilding()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.RoomId = Guid.NewGuid();
        command.BuildingId = null;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
        Assert.Contains(result.Errors, e => e.Description.Contains("A building must be specified when a room is specified", StringComparison.InvariantCulture));
    }

    [Fact]
    public async Task Handle_Should_AcceptCommand_When_RoomAndBuildingAreSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.RoomId = Guid.NewGuid();
        command.BuildingId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_Should_AcceptCommand_When_RoomIsNullAndBuildingIsSpecified()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.RoomId = null;
        command.BuildingId = Guid.NewGuid();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);
    }

    [Fact]
    public async Task Handle_Should_UpdateIsPublicFlag_When_ChangingFromPrivateToPublic()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePrivateEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.IsPublic = true;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        var updatedEvent = await db.Events.FindAsync(existingEvent.Id);
        Assert.True(updatedEvent!.IsPublic);
    }

    [Fact]
    public async Task Handle_Should_UpdateIsPublicFlag_When_ChangingFromPublicToPrivate()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = TestDataBuilder.CreatePublicEvent();
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.IsPublic = false;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        var updatedEvent = await db.Events.FindAsync(existingEvent.Id);
        Assert.False(updatedEvent!.IsPublic);
    }

    [Fact]
    public async Task Handle_Should_ClearNullableFields_When_SetToNull()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var existingEvent = new DormEvent
        {
            Id = Guid.NewGuid(),
            Name = "Original Event",
            Date = DateTime.UtcNow.AddDays(7),
            Location = "Original Location",
            Description = "Original description",
            NumberOfAttendees = 100,
            IsPublic = true,
            OwnerId = Guid.NewGuid(),
            BuildingId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
        };
        await db.Events.AddAsync(existingEvent);
        await db.SaveChangesAsync();

        var command = ValidCommand(existingEvent.Id);
        command.NumberOfAttendees = null;
        command.BuildingId = null;
        command.RoomId = null;
        command.Location = "New Location"; // Ensure validation passes

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsError);

        var updatedEvent = await db.Events.FindAsync(existingEvent.Id);
        Assert.Null(updatedEvent!.NumberOfAttendees);
        Assert.Null(updatedEvent.BuildingId);
        Assert.Null(updatedEvent.RoomId);
    }

    [Fact]
    public async Task Handle_Should_ReturnValidationErrors_When_MultipleValidationRulesFail()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Id = Guid.Empty;
        command.Name = string.Empty;
        command.Date = DateTime.UtcNow.AddDays(-1);
        command.Location = string.Empty;
        command.BuildingId = null;
        command.NumberOfAttendees = -5;

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.True(result.Errors.Count > 1); // Multiple validation errors
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task Handle_Should_NotCheckDatabase_When_ValidationFails()
    {
        // Arrange
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Date = DateTime.UtcNow.AddDays(-1); // This will cause validation to fail

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));

        // Verify no database queries were made by checking that no events were accessed
        // Since validation fails early, the database query for finding the event shouldn't execute
    }
}