using MediatR;

namespace TelegramAgent.API.Features.Queries.GetLinkStatus
{
    public record GetLinkStatusQuery(long ChatId) : IRequest<LinkStatusResult>;

    public record LinkStatusResult(bool IsLinked, string Message, DateTime? LinkedAt = null);
}