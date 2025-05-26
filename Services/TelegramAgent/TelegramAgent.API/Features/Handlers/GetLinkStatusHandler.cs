using MediatR;
using Microsoft.EntityFrameworkCore;
using TelegramAgent.API.Data;
using TelegramAgent.API.Features.Queries.GetLinkStatus;

namespace TelegramAgent.API.Features.Handlers
{
    public class GetLinkStatusHandler : IRequestHandler<GetLinkStatusQuery, LinkStatusResult>
    {
        private readonly TelegramDbContext _dbContext;
        private readonly ILogger<GetLinkStatusHandler> _logger;

        public GetLinkStatusHandler(TelegramDbContext dbContext, ILogger<GetLinkStatusHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<LinkStatusResult> Handle(GetLinkStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var link = await _dbContext.TelegramLinks
                    .FirstOrDefaultAsync(tl => tl.ChatId == request.ChatId, cancellationToken);

                if (link == null)
                {
                    return new LinkStatusResult(
                        false,
                        "❌ No account is linked to this chat.\n\nUse /auth <code> to link your account.");
                }

                var timeAgo = DateTime.UtcNow - link.LinkedAt;
                var timeAgoText = timeAgo.TotalDays >= 1
                    ? $"{(int)timeAgo.TotalDays} day(s) ago"
                    : timeAgo.TotalHours >= 1
                        ? $"{(int)timeAgo.TotalHours} hour(s) ago"
                        : $"{(int)timeAgo.TotalMinutes} minute(s) ago";

                return new LinkStatusResult(
                    true,
                    $"✅ Account is linked to this chat.\n📅 Linked: {timeAgoText}\n\nYou will receive notifications here.",
                    link.LinkedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting link status for chat {ChatId}", request.ChatId);
                return new LinkStatusResult(false, "❌ An error occurred while checking link status. Please try again.");
            }
        }
    }
}