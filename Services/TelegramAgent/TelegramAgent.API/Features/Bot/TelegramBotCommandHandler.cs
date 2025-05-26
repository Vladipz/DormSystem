using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramAgent.API.Features.Bot
{
    public interface ITelegramBotCommandHandler
    {
        Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    }

    public class TelegramBotCommandHandler : ITelegramBotCommandHandler
    {
        private readonly ITelegramCommandRegistry _commandRegistry;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<TelegramBotCommandHandler> _logger;

        public TelegramBotCommandHandler(
            ITelegramCommandRegistry commandRegistry,
            ITelegramBotClient botClient,
            ILogger<TelegramBotCommandHandler> logger)
        {
            _commandRegistry = commandRegistry;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.Text == null || update.Message.Chat.Id == 0)
                return;

            var chatId = update.Message.Chat.Id;
            var messageText = update.Message.Text.Trim();

            _logger.LogInformation("Received message from chat {ChatId}: {Message}", chatId, messageText);

            try
            {
                var (commandName, args) = ParseCommand(messageText);

                var command = _commandRegistry.GetCommand(commandName);
                if (command != null)
                {
                    await command.HandleAsync(chatId, args, cancellationToken);
                }
                else
                {
                    await HandleUnknownCommand(chatId, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling update for chat {ChatId}", chatId);
                await SendErrorMessage(chatId, cancellationToken);
            }
        }

        private static (string command, string[] args) ParseCommand(string messageText)
        {
            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var command = parts.Length > 0 ? parts[0] : string.Empty;
            var args = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
            return (command, args);
        }



        private async Task HandleUnknownCommand(long chatId, CancellationToken cancellationToken)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "❓ Unknown command. Use /start for help, /auth <code> to link, /unlink to unlink, or /status to check status.",
                cancellationToken: cancellationToken);
        }

        private async Task SendErrorMessage(long chatId, CancellationToken cancellationToken)
        {
            try
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ An error occurred while processing your request. Please try again.",
                    cancellationToken: cancellationToken);
            }
            catch (Exception sendEx)
            {
                _logger.LogError(sendEx, "Failed to send error message to chat {ChatId}", chatId);
            }
        }
    }
}