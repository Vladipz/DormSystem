using System.Globalization;

using MassTransit;

using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Entities;

using Shared.Data;
using Shared.RoomServiceClient;

namespace NotificationCore.API.Events.Events
{
    public class RoomInspectionStatusUpdatedConsumer : IConsumer<RoomInspectionStatusUpdatedEvent>
    {
        private static readonly Action<ILogger, Guid, string, int, Exception?> _logNotificationsCreated =
            LoggerMessage.Define<Guid, string, int>(
                LogLevel.Information,
                new EventId(1, nameof(RoomInspectionStatusUpdatedConsumer)),
                "Created {NotificationCount} notifications for room inspection {RoomInspectionId} status update to {NewStatus}");

        private static readonly Action<ILogger, Guid, Exception?> _logNoOccupantsFound =
            LoggerMessage.Define<Guid>(
                LogLevel.Information,
                new EventId(2, nameof(RoomInspectionStatusUpdatedConsumer)),
                "No occupants found for room {RoomId}, skipping notification creation");

        private static readonly Action<ILogger, Guid, Exception?> _logRoomServiceError =
            LoggerMessage.Define<Guid>(
                LogLevel.Warning,
                new EventId(3, nameof(RoomInspectionStatusUpdatedConsumer)),
                "Failed to get room occupants for room {RoomId}");

        private readonly ApplicationDbContext _db;
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomInspectionStatusUpdatedConsumer> _logger;

        public RoomInspectionStatusUpdatedConsumer(
            ApplicationDbContext db,
            IRoomService roomService,
            ILogger<RoomInspectionStatusUpdatedConsumer> logger)
        {
            _db = db;
            _roomService = roomService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<RoomInspectionStatusUpdatedEvent> context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var inspectionEvent = context.Message;

            // Get occupied places (users) in the room
            var placesResult = await _roomService.GetOccupiedPlacesByRoomIdAsync(
                inspectionEvent.RoomId,
                context.CancellationToken);

            if (placesResult.IsError)
            {
                _logRoomServiceError(_logger, inspectionEvent.RoomId, null);

                // Don't fail the whole operation if we can't get room occupants
                return;
            }

            var occupiedPlaces = placesResult.Value;
            if (occupiedPlaces.Count == 0)
            {
                _logNoOccupantsFound(_logger, inspectionEvent.RoomId, null);
                return;
            }

            // Extract all user IDs at once
            var userIds = occupiedPlaces
                .Where(p => p.OccupiedByUserId.HasValue)
                .Select(p => p.OccupiedByUserId!.Value)
                .ToList();

            if (userIds.Count == 0)
            {
                return;
            }

            // Get all users with inspection notifications enabled in one query
            var usersWithInspectionNotifications = await _db.UserNotificationSettings
                .Where(setting =>
                    userIds.Contains(setting.UserId) &&
                    setting.NotificationType == NotificationType.InspectionResults &&
                    setting.Enabled)
                .Select(setting => setting.UserId)
                .ToListAsync(context.CancellationToken);

            _logger.LogInformation("Users with inspection notifications: {Users}", usersWithInspectionNotifications);

            if (usersWithInspectionNotifications.Count == 0)
            {
                return;
            }

            var notifications = new List<Notification>();

            // Create notifications only for users who have enabled inspection notifications
            foreach (var userId in usersWithInspectionNotifications)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Title = GetNotificationTitle(inspectionEvent.NewStatus, inspectionEvent.InspectionType),
                    Message = GetNotificationMessage(inspectionEvent),
                    Type = NotificationType.InspectionResults,
                    CreatedAt = DateTime.UtcNow,
                };

                notifications.Add(notification);
            }

            if (notifications.Count != 0)
            {
                // Save all notifications to database
                _db.Notifications.AddRange(notifications);
                await _db.SaveChangesAsync(context.CancellationToken);

                // Publish integration events for each notification
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

                _logNotificationsCreated(_logger, inspectionEvent.RoomInspectionId, inspectionEvent.NewStatus, notifications.Count, null);
            }
        }

        private static string GetNotificationTitle(string status, string inspectionType) => status.ToLower(CultureInfo.CurrentCulture) switch
        {
            "confirmed" => "🎉 Great News! Inspection Passed",
            "notconfirmed" => "⚠️ Issues Found During Inspection",
            "noaccess" => "🚫 Access Issue Occurred",
            _ => "📋 Room Inspection Update"
        };

        private static string GetNotificationMessage(RoomInspectionStatusUpdatedEvent inspectionEvent)
        {
            var roomInfo = $"🏠 Room {inspectionEvent.RoomNumber}, {inspectionEvent.Building}";
            var inspectionInfo = $"📝 Inspection: '{inspectionEvent.InspectionName}' ({inspectionEvent.InspectionType})";

            return inspectionEvent.NewStatus.ToLower(CultureInfo.CurrentCulture) switch
            {
                "confirmed" => FormatConfirmedMessage(roomInfo, inspectionInfo),
                "notconfirmed" => FormatNotConfirmedMessage(roomInfo, inspectionInfo, inspectionEvent.Comment),
                "noaccess" => FormatNoAccessMessage(roomInfo, inspectionInfo, inspectionEvent.Comment),
                _ => FormatGenericMessage(roomInfo, inspectionInfo, inspectionEvent.NewStatus, inspectionEvent.Comment)
            };
        }

        private static string FormatConfirmedMessage(string roomInfo, string inspectionInfo)
        {
            return $"✨ Congratulations! Your room has successfully passed the inspection!\n\n" +
                   $"{roomInfo}\n" +
                   $"{inspectionInfo}\n\n" +
                   $"🎯 Result: Everything looks perfect! No issues found.\n" +
                   $"💫 Thank you for maintaining excellent standards!";
        }

        private static string FormatNotConfirmedMessage(string roomInfo, string inspectionInfo, string? comment)
        {
            var message = $"🔍 Inspection Results for Your Room\n\n" +
                         $"{roomInfo}\n" +
                         $"{inspectionInfo}\n\n" +
                         $"📌 Status: Issues identified that require attention";

            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $"\n\n💬 Inspector's Notes:\n\"{comment}\"";
            }

            message += "\n\n🔧 Please address the mentioned issues and contact administration for a follow-up inspection.";

            return message;
        }

        private static string FormatNoAccessMessage(string roomInfo, string inspectionInfo, string? comment)
        {
            var message = $"🚪 Unable to Complete Inspection\n\n" +
                         $"{roomInfo}\n" +
                         $"{inspectionInfo}\n\n" +
                         $"❌ The inspector was unable to access your room.";

            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $"\n\n💬 Note:\n\"{comment}\"";
            }

            message += "\n\n📞 Please contact administration to schedule a new inspection date.";

            return message;
        }

        private static string FormatGenericMessage(string roomInfo, string inspectionInfo, string status, string? comment)
        {
            var message = $"📄 Inspection Status Update\n\n" +
                         $"{roomInfo}\n" +
                         $"{inspectionInfo}\n\n" +
                         $"🔄 New Status: {status}";

            if (!string.IsNullOrWhiteSpace(comment))
            {
                message += $"\n\n💬 Comment:\n\"{comment}\"";
            }

            return message;
        }
    }
}