using Telegram.Bot.Types;

namespace TelegramAgent.API.Features.Bot.Commands
{
    public interface ITelegramCommand
    {
        string CommandName { get; }
        Task HandleAsync(long chatId, string[] args, CancellationToken cancellationToken);
    }
}