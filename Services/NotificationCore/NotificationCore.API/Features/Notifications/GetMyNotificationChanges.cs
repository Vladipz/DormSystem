using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Mappings;

using Shared.TokenService.Services;

namespace NotificationCore.API.Features.Notifications
{
    public static class GetMyNotificationChanges
    {
        public sealed record Query(Guid UserId, DateTime? AfterCreatedAtUtc, int Limit)
            : IRequest<ErrorOr<Response>>;

        public sealed record Response(List<GetMyNotifications.NotificationDto> Notifications);

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<Response>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var limit = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 100);

                var query = _db.Notifications
                    .Where(notification => notification.UserId == request.UserId);

                if (request.AfterCreatedAtUtc.HasValue)
                {
                    query = query.Where(notification => notification.CreatedAt > request.AfterCreatedAtUtc.Value);
                }

                var notifications = await query
                    .OrderBy(notification => notification.CreatedAt)
                    .Take(limit)
                    .Select(notification => new GetMyNotifications.NotificationDto(
                        notification.Id,
                        notification.Title,
                        notification.Message,
                        notification.Type,
                        notification.CreatedAt,
                        notification.IsRead))
                    .ToListAsync(cancellationToken);

                return new Response(notifications);
            }
        }
    }

    public sealed class GetMyNotificationChangesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/notifications/me/changes",
                async (
                    [FromQuery] DateTime? afterCreatedAt,
                    [FromQuery] int? limit,
                    HttpContext httpContext,
                    ISender sender,
                    [FromServices] ITokenService tokenService) =>
                {
                    var userId = tokenService.GetUserId(httpContext);
                    if (userId.IsError)
                    {
                        return Results.Unauthorized();
                    }

                    var result = await sender.Send(new GetMyNotificationChanges.Query(
                        userId.Value,
                        afterCreatedAt,
                        limit ?? 50));

                    return result.Match(
                        data => Results.Ok(data),
                        error => error.ToResponse());
                })
                .Produces<GetMyNotificationChanges.Response>(200)
                .Produces<Error>(400)
                .Produces(401)
                .IncludeInOpenApi()
                .WithName("GetMyNotificationChanges")
                .WithTags("Notifications")
                .RequireAuthorization();
        }
    }
}
