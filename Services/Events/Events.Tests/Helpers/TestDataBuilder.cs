using Events.API.Entities;

namespace Events.Tests.Helpers;

internal static class TestDataBuilder
{
    public static DormEvent CreatePublicEvent(Guid? ownerId = null) =>
        new DormEvent
        {
            Id = Guid.NewGuid(),
            Name = "Test Public Event",
            Date = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Description = "Test description",
            IsPublic = true,
            OwnerId = ownerId ?? Guid.NewGuid(),
        };

    public static DormEvent CreatePrivateEvent(Guid? ownerId = null) =>
        new DormEvent
        {
            Id = Guid.NewGuid(),
            Name = "Test Private Event",
            Date = DateTime.UtcNow.AddDays(7),
            Location = "Test Location",
            Description = "Test description",
            IsPublic = false,
            OwnerId = ownerId ?? Guid.NewGuid(),
        };

    public static InvitationToken CreateActiveToken(Guid eventId) =>
        new InvitationToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString("N"),
            EventId = eventId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsActive = true,
        };

    public static InvitationToken CreateExpiredToken(Guid eventId) =>
        new InvitationToken
        {
            Id = Guid.NewGuid(),
            Token = Guid.NewGuid().ToString("N"),
            EventId = eventId,
            CreatedAt = DateTime.UtcNow.AddDays(-31),
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
        };
}
