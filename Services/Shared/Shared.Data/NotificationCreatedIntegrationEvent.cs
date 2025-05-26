namespace Shared.Data
{
    public record NotificationCreatedIntegrationEvent(
        Guid NotificationId,
        Guid UserId,
        string Title,
        string Message,
        DateTime CreatedAt);
} 