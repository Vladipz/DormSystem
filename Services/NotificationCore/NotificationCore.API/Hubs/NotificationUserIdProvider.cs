using System.Security.Claims;

using Microsoft.AspNetCore.SignalR;

namespace NotificationCore.API.Hubs
{
    public sealed class NotificationUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? connection.User?.FindFirstValue("sub");
        }
    }
}
