using MediatR;

namespace TelegramAgent.API.Features.Queries.GetHelp
{
    public record GetHelpQuery : IRequest<HelpResult>;

    public record HelpResult(string Message);
}