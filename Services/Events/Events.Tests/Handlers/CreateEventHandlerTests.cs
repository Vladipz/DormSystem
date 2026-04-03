using ErrorOr;

using Events.API.Database;
using Events.API.Features.Events;
using Events.Tests.Helpers;

using MassTransit;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using Shared.Data;

using Xunit;

namespace Events.Tests.Handlers;

public sealed class CreateEventHandlerTests
{
    private static CreateEvent.Handler BuildHandler(
        EventsDbContext db,
        Mock<IBus>? busMock = null)
    {
        busMock ??= new Mock<IBus>();
        busMock.Setup(b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        return new CreateEvent.Handler(
            db,
            new CreateEvent.Validator(),
            NullLogger<CreateEvent.Handler>.Instance,
            busMock.Object);
    }

    private static CreateEvent.Command ValidCommand(Guid? ownerId = null) => new ()
    {
        Name = "Test Event",
        Date = DateTime.UtcNow.AddDays(1),
        Location = "Main Hall",
        Description = "A test event.",
        OwnerId = ownerId ?? Guid.NewGuid(),
        IsPublic = true,
    };

    [Fact]
    public async Task ValidCommand_ReturnsNonEmptyGuid()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);

        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task ValidCommand_PersistsEventToDatabase()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        var saved = await db.Events.FindAsync(result.Value);
        Assert.NotNull(saved);
        Assert.Equal(command.Name, saved.Name);
    }

    [Fact]
    public async Task ValidCommand_PublishesEventCreatedMessage()
    {
        var db = EventsDbContextFactory.Create();
        var busMock = new Mock<IBus>();
        busMock.Setup(b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        var handler = BuildHandler(db, busMock);

        await handler.Handle(ValidCommand(), CancellationToken.None);

        busMock.Verify(
            b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ValidCommand_PublishedMessage_HasCorrectOwnerId()
    {
        var db = EventsDbContextFactory.Create();
        var busMock = new Mock<IBus>();
        var ownerId = Guid.NewGuid();
        var handler = BuildHandler(db, busMock);

        // Override after BuildHandler so callback is the active setup
        EventCreated? published = null;
        busMock.Setup(b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()))
               .Callback<EventCreated, CancellationToken>((e, _) => published = e)
               .Returns(Task.CompletedTask);

        await handler.Handle(ValidCommand(ownerId), CancellationToken.None);

        Assert.NotNull(published);
        Assert.Equal(ownerId, published.OwnerId);
    }

    [Fact]
    public async Task EmptyName_ReturnsValidationError()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Name = string.Empty;

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task PastDate_ReturnsValidationError()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Date = DateTime.UtcNow.AddDays(-1);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task NoLocationAndNoBuilding_ReturnsValidationError()
    {
        var db = EventsDbContextFactory.Create();
        var handler = BuildHandler(db);
        var command = ValidCommand();
        command.Location = string.Empty;
        command.BuildingId = null;

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.All(result.Errors, e => Assert.Equal(ErrorType.Validation, e.Type));
    }

    [Fact]
    public async Task ValidationFailure_DoesNotPublish()
    {
        var db = EventsDbContextFactory.Create();
        var busMock = new Mock<IBus>();
        busMock.Setup(b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        var handler = BuildHandler(db, busMock);
        var command = ValidCommand();
        command.Name = string.Empty;

        await handler.Handle(command, CancellationToken.None);

        busMock.Verify(
            b => b.Publish(It.IsAny<EventCreated>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}