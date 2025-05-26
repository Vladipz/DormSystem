using MediatR;

namespace TelegramAgent.API.Features.Commands.LinkAccount
{
    public record LinkAccountCommand(long ChatId, string Code) : IRequest<LinkAccountResult>;

    public record LinkAccountResult(bool Success, string Message);
}