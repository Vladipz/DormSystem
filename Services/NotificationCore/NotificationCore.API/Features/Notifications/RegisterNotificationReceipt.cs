using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Mappings;
using NotificationCore.API.Observability;

using Shared.TokenService.Services;

namespace NotificationCore.API.Features.Notifications
{
    public static class RegisterNotificationReceipt
    {
        private static readonly HashSet<string> AllowedModes =
        [
            "websocket",
            "polling_5s",
        ];

        public sealed record Request(Guid NotificationId, string Mode, DateTime ReceivedAtUtc);

        public sealed record Command(Guid UserId, Guid NotificationId, string Mode, DateTime ReceivedAtUtc)
            : IRequest<ErrorOr<Success>>;

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Success>>
        {
            private readonly ApplicationDbContext _db;
            private readonly ILogger<Handler> _logger;

            public Handler(ApplicationDbContext db, ILogger<Handler> logger)
            {
                _db = db;
                _logger = logger;
            }

            public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
            {
                if (!AllowedModes.Contains(request.Mode))
                {
                    return Error.Validation("Notifications.InvalidMode", "Unsupported notification delivery mode.");
                }

                var notification = await _db.Notifications
                    .FirstOrDefaultAsync(
                        x => x.Id == request.NotificationId && x.UserId == request.UserId,
                        cancellationToken);

                if (notification is null)
                {
                    return Error.NotFound("Notifications.NotFound", "Notification not found.");
                }

                var latencyMs = (request.ReceivedAtUtc - notification.CreatedAt).TotalMilliseconds;
                if (latencyMs >= 0)
                {
                    NotificationMetrics.DeliveryLatencyMs.Record(
                        latencyMs,
                        new KeyValuePair<string, object?>("mode", request.Mode));

                    _logger.LogInformation(
                        "Recorded notification delivery latency for {NotificationId} in mode {Mode}: {LatencyMs}ms",
                        notification.Id,
                        request.Mode,
                        latencyMs);
                }

                return Result.Success;
            }
        }
    }

    public sealed class RegisterNotificationReceiptEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/notifications/me/receipt",
                async (
                    [FromBody] RegisterNotificationReceipt.Request request,
                    HttpContext httpContext,
                    ISender sender,
                    [FromServices] ITokenService tokenService) =>
                {
                    var userId = tokenService.GetUserId(httpContext);
                    if (userId.IsError)
                    {
                        return Results.Unauthorized();
                    }

                    var result = await sender.Send(new RegisterNotificationReceipt.Command(
                        userId.Value,
                        request.NotificationId,
                        request.Mode,
                        request.ReceivedAtUtc));

                    return result.Match(
                        _ => Results.NoContent(),
                        error => error.ToResponse());
                })
                .Produces(204)
                .Produces<Error>(400)
                .Produces(401)
                .Produces(404)
                .IncludeInOpenApi()
                .WithName("RegisterNotificationReceipt")
                .WithTags("Notifications")
                .RequireAuthorization();
        }
    }
}
