using NotificationCore.API.Entities;

namespace NotificationCore.API.Contracts.Notifications
{
    public sealed record InAppNotificationDto(
        Guid Id,
        string Title,
        string Message,
        NotificationType Type,
        DateTime CreatedAt,
        bool IsRead);
}
