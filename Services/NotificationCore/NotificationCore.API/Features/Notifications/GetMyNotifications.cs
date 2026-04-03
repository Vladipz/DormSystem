using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Entities;
using NotificationCore.API.Mappings;

using Shared.TokenService.Services;

namespace NotificationCore.API.Features.Notifications
{
    public static class GetMyNotifications
    {
        public sealed record Query(Guid UserId, int Limit) : IRequest<ErrorOr<Response>>;

        public sealed record Response(Guid UserId, int UnreadCount, List<NotificationDto> Notifications);

        public sealed record NotificationDto(
            Guid Id,
            string Title,
            string Message,
            NotificationType Type,
            DateTime CreatedAt,
            bool IsRead);

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<Response>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var limit = request.Limit <= 0 ? 10 : Math.Min(request.Limit, 50);

                var unreadCount = await _db.Notifications
                    .Where(notification => notification.UserId == request.UserId && !notification.IsRead)
                    .CountAsync(cancellationToken);

                var notifications = await _db.Notifications
                    .Where(notification => notification.UserId == request.UserId)
                    .OrderByDescending(notification => notification.CreatedAt)
                    .Take(limit)
                    .Select(notification => new NotificationDto(
                        notification.Id,
                        notification.Title,
                        notification.Message,
                        notification.Type,
                        notification.CreatedAt,
                        notification.IsRead))
                    .ToListAsync(cancellationToken);

                return new Response(request.UserId, unreadCount, notifications);
            }
        }
    }

    public sealed class GetMyNotificationsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/notifications/me",
                async (
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

                    var result = await sender.Send(new GetMyNotifications.Query(userId.Value, limit ?? 10));

                    return result.Match(
                        data => Results.Ok(data),
                        error => error.ToResponse());
                })
                .Produces<GetMyNotifications.Response>(200)
                .Produces<Error>(400)
                .Produces(401)
                .IncludeInOpenApi()
                .WithName("GetMyNotifications")
                .WithTags("Notifications")
                .RequireAuthorization();
        }
    }
}
