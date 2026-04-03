using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Entities;

namespace NotificationCore.API.Data
{
    public static class SeedData
    {
        private static readonly Guid AdminId = new("11111111-1111-1111-1111-111111111111");
        private static readonly Guid StudentId = new("22222222-2222-2222-2222-222222222222");
        private static readonly Guid TestStudentOneId = new("44444444-4444-4444-4444-444444444444");
        private static readonly Guid TestStudentTwoId = new("55555555-5555-5555-5555-555555555555");

        public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);

            await EnsureUserNotificationSettingsAsync(dbContext, AdminId, new Dictionary<NotificationType, bool>
            {
                [NotificationType.InspectionResults] = true,
                [NotificationType.Events] = false,
                [NotificationType.RoomBookings] = true,
            }, cancellationToken);

            await EnsureUserNotificationSettingsAsync(dbContext, StudentId, new Dictionary<NotificationType, bool>
            {
                [NotificationType.InspectionResults] = true,
                [NotificationType.Events] = true,
                [NotificationType.RoomBookings] = true,
            }, cancellationToken);

            await EnsureUserNotificationSettingsAsync(dbContext, TestStudentOneId, new Dictionary<NotificationType, bool>
            {
                [NotificationType.Events] = true,
                [NotificationType.InspectionResults] = true,
            }, cancellationToken);

            await EnsureUserNotificationSettingsAsync(dbContext, TestStudentTwoId, new Dictionary<NotificationType, bool>
            {
                [NotificationType.Events] = true,
                [NotificationType.InspectionResults] = true,
            }, cancellationToken);

            await EnsureUserChannelAsync(dbContext, AdminId, NotificationChannel.Telegram, true, "123456789", cancellationToken);
            await EnsureUserChannelAsync(dbContext, AdminId, NotificationChannel.Email, false, "admin@dorm.com", cancellationToken);
            await EnsureUserChannelAsync(dbContext, AdminId, NotificationChannel.InApp, true, null, cancellationToken);

            await EnsureUserChannelAsync(dbContext, StudentId, NotificationChannel.Email, true, "student@dorm.com", cancellationToken);
            await EnsureUserChannelAsync(dbContext, StudentId, NotificationChannel.InApp, true, null, cancellationToken);

            await EnsureUserChannelAsync(dbContext, TestStudentOneId, NotificationChannel.InApp, true, null, cancellationToken);
            await EnsureUserChannelAsync(dbContext, TestStudentTwoId, NotificationChannel.InApp, true, null, cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureUserNotificationSettingsAsync(
            ApplicationDbContext dbContext,
            Guid userId,
            IReadOnlyDictionary<NotificationType, bool> settings,
            CancellationToken cancellationToken)
        {
            var existingSettings = await dbContext.UserNotificationSettings
                .Where(setting => setting.UserId == userId)
                .ToListAsync(cancellationToken);

            foreach (var (notificationType, enabled) in settings)
            {
                var existingSetting = existingSettings.FirstOrDefault(setting => setting.NotificationType == notificationType);
                if (existingSetting is null)
                {
                    dbContext.UserNotificationSettings.Add(new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        NotificationType = notificationType,
                        Enabled = enabled,
                    });
                }
                else
                {
                    existingSetting.Enabled = enabled;
                }
            }
        }

        private static async Task EnsureUserChannelAsync(
            ApplicationDbContext dbContext,
            Guid userId,
            NotificationChannel channel,
            bool enabled,
            string? externalReference,
            CancellationToken cancellationToken)
        {
            var existingChannel = await dbContext.UserChannels
                .FirstOrDefaultAsync(
                    item => item.UserId == userId && item.Channel == channel,
                    cancellationToken);

            if (existingChannel is null)
            {
                dbContext.UserChannels.Add(new UserChannel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Channel = channel,
                    Enabled = enabled,
                    ExternalReference = externalReference,
                });
            }
            else
            {
                existingChannel.Enabled = enabled;
                existingChannel.ExternalReference = externalReference;
            }
        }
    }
}
