using MediatR;
using Microsoft.EntityFrameworkCore;
using TelegramAgent.API.Data;
using TelegramAgent.API.Features.Commands.UnlinkAccount;

namespace TelegramAgent.API.Features.Handlers
{
    public class UnlinkAccountHandler : IRequestHandler<UnlinkAccountCommand, UnlinkAccountResult>
    {
        private readonly TelegramDbContext _dbContext;
        private readonly ILogger<UnlinkAccountHandler> _logger;

        public UnlinkAccountHandler(TelegramDbContext dbContext, ILogger<UnlinkAccountHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<UnlinkAccountResult> Handle(UnlinkAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingLink = await _dbContext.TelegramLinks
                    .FirstOrDefaultAsync(tl => tl.ChatId == request.ChatId, cancellationToken);

                if (existingLink == null)
                {
                    return new UnlinkAccountResult(false, "No account is currently linked to this chat.");
                }

                _dbContext.TelegramLinks.Remove(existingLink);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully unlinked user {UserId} from Telegram chat {ChatId}",
                    existingLink.UserId,
                    request.ChatId);

                return new UnlinkAccountResult(true, "Account unlinked successfully! You will no longer receive notifications in this chat.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking account for chat {ChatId}", request.ChatId);
                return new UnlinkAccountResult(false, "An error occurred while unlinking your account. Please try again.");
            }
        }
    }
}