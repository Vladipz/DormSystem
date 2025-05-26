using MediatR;
using Telegram.Bot;
using TelegramAgent.API.Features.Queries.GetHelp;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public class StartCommand : ITelegramCommand
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<StartCommand> _logger;

        public string CommandName => "/start";

        public StartCommand(IMediator mediator, ITelegramBotClient botClient, ILogger<StartCommand> logger)
        {
            _mediator = mediator;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing /start command for chat {ChatId}", chatId);

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