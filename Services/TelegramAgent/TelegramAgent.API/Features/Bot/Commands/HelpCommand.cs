using MediatR;

using Telegram.Bot;

using TelegramAgent.API.Features.Queries.GetHelp;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public class HelpCommand(IMediator mediator, ITelegramBotClient botClient, ILogger<HelpCommand> logger) : ITelegramCommand
    {
        private readonly IMediator _mediator = mediator;
        private readonly ITelegramBotClient _botClient = botClient;
        private readonly ILogger<HelpCommand> _logger = logger;

        public string CommandName => "/help";

        public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing /help command for chat {ChatId}", chatId);

            var query = new GetHelpQuery();
            var result = await _mediator.Send(query, cancellationToken);

            await _botClient.SendMessage(
                chatId: chatId,
                text: result.Message,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Successfully sent help message to chat {ChatId}", chatId);
        }
    }
}