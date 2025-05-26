using MediatR;
using Telegram.Bot;
using TelegramAgent.API.Features.Queries.GetLinkStatus;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public class StatusCommand : ITelegramCommand
    {
        private readonly IMediator _mediator;
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger<StatusCommand> _logger;

        public string CommandName => "/status";

        public StatusCommand(IMediator mediator, ITelegramBotClient botClient, ILogger<StatusCommand> logger)
        {
            _mediator = mediator;
            _botClient = botClient;
            _logger = logger;
        }

        public async Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Processing /status command for chat {ChatId}", chatId);

            var query = new GetLinkStatusQuery(chatId);
            var result = await _mediator.Send(query, cancellationToken);

            await _botClient.SendMessage(
                chatId: chatId,
                text: result.Message,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Processed /status command for chat {ChatId}, linked: {IsLinked}", chatId, result.IsLinked);
        }
    }
}