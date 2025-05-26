using MediatR;
using Telegram.Bot;
using TelegramAgent.API.Features.Commands.UnlinkAccount;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public class UnlinkCommand : ITelegramCommand
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<UnlinkCommand> _logger;

        public string CommandName => "/unlink";

        public UnlinkCommand(IMediator mediator, ITelegramBotClient botClient, ILogger<UnlinkCommand> logger)
        {
            _mediator = mediator;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing /unlink command for chat {ChatId}", chatId);

            var command = new UnlinkAccountCommand(chatId);
            var result = await _mediator.Send(command, cancellationToken);

            var emoji = result.Success ? "✅" : "❌";
            await _botClient.SendMessage(
                chatId: chatId,
                text: $"{emoji} {result.Message}",
                cancellationToken: cancellationToken);

            _logger.LogInformation("Processed /unlink command for chat {ChatId}, success: {Success}", chatId, result.Success);
        }
    }
}