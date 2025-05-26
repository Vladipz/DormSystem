using MediatR;
using Telegram.Bot;
using TelegramAgent.API.Features.Commands.LinkAccount;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public class AuthCommand : ITelegramCommand
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<AuthCommand> _logger;

        public string CommandName => "/auth";

        public AuthCommand(IMediator mediator, ITelegramBotClient botClient, ILogger<AuthCommand> logger)
        {
            _mediator = mediator;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing /auth command for chat {ChatId}", chatId);

            if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Please provide a 6-digit code. Usage: /auth 123456",
                    cancellationToken: cancellationToken);
                return;
            }

            var code = args[0].Trim();

            if (code.Length != 6 || !code.All(char.IsDigit))
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ Invalid code format. Please provide a 6-digit code like: /auth 123456",
                    cancellationToken: cancellationToken);
                return;
            }

            var command = new LinkAccountCommand(chatId, code);
            var result = await _mediator.Send(command, cancellationToken);

            var emoji = result.Success ? "✅" : "❌";
            await _botClient.SendMessage(
                chatId: chatId,
                text: $"{emoji} {result.Message}",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Processed /auth command for chat {ChatId}, success: {Success}", chatId, result.Success);
        }
    }
}