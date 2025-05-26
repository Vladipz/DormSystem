using MassTransit;

using Microsoft.EntityFrameworkCore;

using Shared.Data;

using Telegram.Bot;

using TelegramAgent.API.Data;

namespace TelegramAgent.API.Features
{
    public static class NotificationDelivery
    {
        public class NotificationCreatedConsumer : IConsumer<NotificationCreatedIntegrationEvent>
        {
            private readonly TelegramDbContext _dbContext;
            private readonly ITelegramBotClient _botClient;
            private readonly ILogger<NotificationCreatedConsumer> _logger;

            public NotificationCreatedConsumer(TelegramDbContext dbContext, ITelegramBotClient botClient, ILogger<NotificationCreatedConsumer> logger)
            {
                _dbContext = dbContext;
                _botClient = botClient;
                _logger = logger;
            }

            public async Task Consume(ConsumeContext<NotificationCreatedIntegrationEvent> context)
            {
                var notification = context.Message;

                _logger.LogInformation(
                    "Processing notification {NotificationId} for user {UserId}",
                    notification.NotificationId, notification.UserId);

                // Look up the Telegram chat for this user
                var telegramLink = await _dbContext.TelegramLinks
                    .FirstOrDefaultAsync(tl => tl.UserId == notification.UserId, context.CancellationToken);

                if (telegramLink == null)
                {
                    _logger.LogInformation(
                        "No Telegram link found for user {UserId}, skipping notification {NotificationId}",
                        notification.UserId, notification.NotificationId);
                    return;
                }

                try
                {
                    var message = $"{notification.Title}\n\n{notification.Message}";

                    await _botClient.SendMessage(
                        chatId: telegramLink.ChatId,
                        text: message,
                        cancellationToken: context.CancellationToken);

                    _logger.LogInformation(
                        "Successfully sent notification {NotificationId} to Telegram chat {ChatId} for user {UserId}",
                        notification.NotificationId, telegramLink.ChatId, notification.UserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification {NotificationId} to Telegram chat {ChatId} for user {UserId}",
                        notification.NotificationId, telegramLink.ChatId, notification.UserId);

                    // Don't rethrow - let MassTransit handle retries based on default policy
                    // The consumer should not break due to Telegram API errors
                }
            }
        }
    }
}