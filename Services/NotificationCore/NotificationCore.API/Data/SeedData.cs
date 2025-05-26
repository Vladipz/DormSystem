using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Entities;

namespace NotificationCore.API.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken = default)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);

            if (!await dbContext.UserNotificationSettings.AnyAsync(cancellationToken))
            {
                var adminId = new Guid("00000000-0000-0000-0000-000000000001");
                var studentId = new Guid("22222222-2222-2222-2222-222222222222");

                dbContext.UserNotificationSettings.AddRange(
                [
                    // Admin settings
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminId,
                        NotificationType = NotificationType.InspectionResults,
                        Enabled = true,
                    },
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminId,
                        NotificationType = NotificationType.Events,
                        Enabled = false,
                    },
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminId,
                        NotificationType = NotificationType.RoomBookings,
                        Enabled = true,
                    },
                    // Student settings
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentId,
                        NotificationType = NotificationType.InspectionResults,
                        Enabled = true,
                    },
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentId,
                        NotificationType = NotificationType.Events,
                        Enabled = true,
                    },
                    new UserNotificationSetting
                    {
                        Id = Guid.NewGuid(),
                        UserId = studentId,
                        NotificationType = NotificationType.RoomBookings,
                        Enabled = true,
                    },
                ]);
            }

            if (!await dbContext.UserChannels.AnyAsync(cancellationToken))
            {
                dbContext.UserChannels.AddRange(
                [
                    new UserChannel
                    {
                        Id = Guid.NewGuid(),
                        UserId = new Guid("00000000-0000-0000-0000-000000000001"),
                        Channel = NotificationChannel.Telegram,
                        Enabled = true,
                        ExternalReference = "123456789", // e.g. chat_id
                    },
                    new UserChannel
                    {
                        Id = Guid.NewGuid(),
                        UserId = new Guid("00000000-0000-0000-0000-000000000001"),
                        Channel = NotificationChannel.Email,
                        Enabled = false,
                        ExternalReference = "user@example.com",
                    },
                    // Student channels
                    new UserChannel
                    {
                        Id = Guid.NewGuid(),
                        UserId = new Guid("22222222-2222-2222-2222-222222222222"),
                        Channel = NotificationChannel.Email,
                        Enabled = true,
                        ExternalReference = "student@dorm.com",
                    },
                ]);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}