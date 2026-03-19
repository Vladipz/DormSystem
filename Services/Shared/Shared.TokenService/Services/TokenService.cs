using System.Security.Claims;

using ErrorOr;

using Microsoft.AspNetCore.Http;

namespace Shared.TokenService.Services
{
    public class TokenService : ITokenService
    {
        public ErrorOr<Guid> GetUserId(HttpContext context)
        {
            // Support both long URI claim names (legacy) and short names (current)
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? context.User.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Error.Unauthorized(description: "Invalid or missing user ID in token");
            }

            return parsedUserId;
        }

        public bool HasToken(HttpContext context)
        {
            return context.Request.Headers.ContainsKey("Authorization");
        }
    }
}