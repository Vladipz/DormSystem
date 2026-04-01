using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Contracts.Notifications;
using NotificationCore.API.Data;
using NotificationCore.API.Entities;
using NotificationCore.API.Hubs;

namespace NotificationCore.API.Services
{
    public interface IInAppNotificationDispatcher
    {
        Task DispatchAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken = default);
    }

    public sealed class InAppNotificationDispatcher : IInAppNotificationDispatcher
    {
        private const string NotificationReceivedMethod = "notificationReceived";

        private readonly ApplicationDbContext _db;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<InAppNotificationDispatcher> _logger;

        public InAppNotificationDispatcher(
            ApplicationDbContext db,
            IHubContext<NotificationHub> hubContext,
            ILogger<InAppNotificationDispatcher> logger)
        {
            _db = db;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task DispatchAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken = default)
        {
            if (notifications.Count == 0)
            {
                return;
            }

            var userIds = notifications
                .Select(notification => notification.UserId)
                .Distinct()
                .ToList();

            var enabledUserIds = await _db.UserChannels
                .Where(channel => userIds.Contains(channel.UserId))
                .Where(channel => channel.Channel == NotificationChannel.InApp && channel.Enabled)
                .Select(channel => channel.UserId)
                .ToListAsync(cancellationToken);

            if (enabledUserIds.Count == 0)
            {
                return;
            }

            var enabledUserIdSet = enabledUserIds.ToHashSet();

            foreach (var notification in notifications.Where(notification => enabledUserIdSet.Contains(notification.UserId)))
            {
                var payload = new InAppNotificationDto(
                    notification.Id,
                    notification.Title,
                    notification.Message,
                    notification.Type,
                    notification.CreatedAt,
                    notification.IsRead);

                await _hubContext.Clients.User(notification.UserId.ToString())
                    .SendAsync(NotificationReceivedMethod, payload, cancellationToken);

                _logger.LogInformation(
                    "Dispatched in-app notification {NotificationId} to user {UserId}",
                    notification.Id,
                    notification.UserId);
            }
        }
    }
}
