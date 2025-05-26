using MediatR;

namespace TelegramAgent.API.Features.Commands.UnlinkAccount
{
    public record UnlinkAccountCommand(long ChatId) : IRequest<UnlinkAccountResult>;

    public record UnlinkAccountResult(bool Success, string Message);
}