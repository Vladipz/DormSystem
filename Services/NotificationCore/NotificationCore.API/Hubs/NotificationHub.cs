using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationCore.API.Hubs
{
    [Authorize]
    public sealed class NotificationHub : Hub
    {
        public Task Ping()
        {
            return Task.CompletedTask;
        }
    }
}
