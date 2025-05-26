using MassTransit;

using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Entities;

using Shared.Data;

namespace NotificationCore.API.Events.Events
{
    public class EventCreatedConsumer : IConsumer<EventCreated>
    {
        private static readonly Action<ILogger, Guid, string, Exception?> _logOwnerNotificationCreated =
            LoggerMessage.Define<Guid, string>(
                LogLevel.Information,
                new EventId(1, nameof(EventCreatedConsumer)),
                "Created notification for event owner {OwnerId} for event {EventName}");

        private static readonly Action<ILogger, int, string, int, Exception?> _logNotificationsCreated =
            LoggerMessage.Define<int, string, int>(
                LogLevel.Information,
                new EventId(2, nameof(EventCreatedConsumer)),
                "Created {NotificationCount} notifications for event {EventName}: 1 for owner + {UserCount} for users with event notifications enabled");

        private static readonly Action<ILogger, int, Guid, Exception?> _logNotificationsSaved =
            LoggerMessage.Define<int, Guid>(
                LogLevel.Information,
                new EventId(3, nameof(EventCreatedConsumer)),
                "Successfully saved {NotificationCount} notifications for event {EventId}");

        private readonly ApplicationDbContext _db;
        private readonly ILogger<EventCreatedConsumer> _logger;

        public EventCreatedConsumer(ApplicationDbContext db, ILogger<EventCreatedConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EventCreated> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var eventMessage = context.Message;
            var notifications = new List<Notification>();

            // 1️⃣ Create notification for the event owner
            var ownerNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = eventMessage.OwnerId,
                Title = "Event Scheduled Successfully",
                Message = $"Your event \"{eventMessage.Name}\" has been scheduled for {eventMessage.Date:MMMM dd, yyyy 'at' HH:mm}.",
                Type = NotificationType.Events,
                CreatedAt = DateTime.UtcNow,
            };

            notifications.Add(ownerNotification);
            _logOwnerNotificationCreated(_logger, eventMessage.OwnerId, eventMessage.Name, null);

            // 2️⃣ Get all users who have enabled Events notifications
            var usersWithEventNotifications = await _db.UserNotificationSettings
                .Where(setting => setting.NotificationType == NotificationType.Events && setting.Enabled)
                .Where(setting => setting.UserId != eventMessage.OwnerId) // Exclude owner to avoid duplicate notification
                .Select(setting => setting.UserId)
                .Distinct()
                .ToListAsync(context.CancellationToken);

            // 3️⃣ Create notifications for all users who have enabled event notifications
            foreach (var userId in usersWithEventNotifications)
            {
                var userNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = "New Event Available",
                    Message = eventMessage.IsPublic
                        ? $"A new public event \"{eventMessage.Name}\" has been scheduled for {eventMessage.Date:MMMM dd, yyyy 'at' HH:mm}. Check it out!"
                        : $"A new event \"{eventMessage.Name}\" has been scheduled for {eventMessage.Date:MMMM dd, yyyy 'at' HH:mm}.",
                    Type = NotificationType.Events,
                    CreatedAt = DateTime.UtcNow,
                };

                notifications.Add(userNotification);
            }

            _logNotificationsCreated(_logger, notifications.Count, eventMessage.Name, usersWithEventNotifications.Count, null);

            // 4️⃣ Save all notifications to database
            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync(context.CancellationToken);

            // 5️⃣ Publish integration events for each notification
            foreach (var notification in notifications)
            {
                await context.Publish(
                    new NotificationCreatedIntegrationEvent(
                    notification.Id,
                    notification.UserId,
                    notification.Title,
                    notification.Message,
                    notification.CreatedAt),
                    context.CancellationToken);
            }

            _logNotificationsSaved(_logger, notifications.Count, eventMessage.EventId, null);
        }
    }
}