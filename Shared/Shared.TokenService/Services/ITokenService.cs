using ErrorOr;
using Microsoft.AspNetCore.Http;

namespace Shared.TokenService.Services
{
    public interface ITokenService
    {
        ErrorOr<Guid> GetUserId(HttpContext context);

        bool HasToken(HttpContext context);
    }
}
