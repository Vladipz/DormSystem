using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TelegramAgent.API.Data;
using TelegramAgent.API.Entities;
using TelegramAgent.API.Features.Commands.LinkAccount;

namespace TelegramAgent.API.Features.Handlers
{
    public class LinkAccountHandler : IRequestHandler<LinkAccountCommand, LinkAccountResult>
    {
        private readonly TelegramDbContext _dbContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LinkAccountHandler> _logger;

        public LinkAccountHandler(
            TelegramDbContext dbContext,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<LinkAccountHandler> logger)
        {
            _dbContext = dbContext;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<LinkAccountResult> Handle(LinkAccountCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validate the code with Auth service
                var userId = await ValidateCodeWithAuthService(request.Code, cancellationToken);
                if (userId == null)
                {
                    return new LinkAccountResult(false, "Invalid or expired code. Please generate a new one.");
                }

                // Handle existing links
                await RemoveExistingLinks(userId.Value, request.ChatId, cancellationToken);

                // Check if already linked to same chat
                if (await IsAlreadyLinked(userId.Value, request.ChatId, cancellationToken))
                {
                    return new LinkAccountResult(true, "Your account is already linked to this chat.");
                }

                // Create new link
                await CreateNewLink(userId.Value, request.ChatId, cancellationToken);

                _logger.LogInformation(
                    "Successfully linked user {UserId} to Telegram chat {ChatId}",
                    userId.Value,
                    request.ChatId);

                return new LinkAccountResult(true, "Account linked successfully! You will now receive notifications in this chat.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking account for chat {ChatId} with code {Code}", request.ChatId, request.Code);
                return new LinkAccountResult(false, "An error occurred while linking your account. Please try again.");
            }
        }

        private async Task<Guid?> ValidateCodeWithAuthService(string code, CancellationToken cancellationToken)
        {
            var authServiceUrl = _configuration["Services:AuthService:BaseUrl"] ?? "http://localhost:5001";
            using var httpClient = _httpClientFactory.CreateClient();

            var validateRequest = new { code };
            var json = JsonSerializer.Serialize(validateRequest);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"{authServiceUrl}/api/link-codes/validate", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to validate code {Code}. Status: {StatusCode}",
                    code,
                    response.StatusCode);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var validateResponse = JsonSerializer.Deserialize<ValidateLinkCodeResponse>(responseContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

            return validateResponse?.UserId;
        }

        private async Task<bool> IsAlreadyLinked(Guid userId, long chatId, CancellationToken cancellationToken)
        {
            return await _dbContext.TelegramLinks
                .AnyAsync(tl => tl.UserId == userId && tl.ChatId == chatId, cancellationToken);
        }

        private async Task RemoveExistingLinks(Guid userId, long chatId, CancellationToken cancellationToken)
        {
            // Remove any existing user link (user linked to different chat)
            var existingUserLink = await _dbContext.TelegramLinks
                .FirstOrDefaultAsync(tl => tl.UserId == userId, cancellationToken);

            // Remove any existing chat link (different user linked to this chat)
            var existingChatLink = await _dbContext.TelegramLinks
                .FirstOrDefaultAsync(tl => tl.ChatId == chatId, cancellationToken);

            if (existingUserLink != null)
            {
                _dbContext.TelegramLinks.Remove(existingUserLink);
            }

            if (existingChatLink != null)
            {
                _dbContext.TelegramLinks.Remove(existingChatLink);
            }
        }

        private async Task CreateNewLink(Guid userId, long chatId, CancellationToken cancellationToken)
        {
            var newLink = new TelegramLink
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ChatId = chatId,
                LinkedAt = DateTime.UtcNow,
            };

            _dbContext.TelegramLinks.Add(newLink);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private record ValidateLinkCodeResponse(Guid UserId);
    }
}