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
    public static class MarkMyNotificationsAsRead
    {
        public sealed record Request(List<Guid> Ids);

        public sealed record Command(Guid UserId, IReadOnlyCollection<Guid> Ids) : IRequest<ErrorOr<Response>>;

        public sealed record Response(int MarkedCount, List<Guid> ReadIds);

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Response>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ErrorOr<Response>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ids = request.Ids
                    .Distinct()
                    .ToList();

                if (ids.Count == 0)
                {
                    return new Response(0, []);
                }

                var notifications = await _db.Notifications
                    .Where(notification => notification.UserId == request.UserId)
                    .Where(notification => !notification.IsRead)
                    .Where(notification => ids.Contains(notification.Id))
                    .ToListAsync(cancellationToken);

                foreach (var notification in notifications)
                {
                    notification.IsRead = true;
                }

                await _db.SaveChangesAsync(cancellationToken);

                var readIds = notifications
                    .Select(notification => notification.Id)
                    .ToList();

                return new Response(readIds.Count, readIds);
            }
        }
    }

    public sealed class MarkMyNotificationsAsReadEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch(
                "/notifications/me/read",
                async (
                    [FromBody] MarkMyNotificationsAsRead.Request request,
                    HttpContext httpContext,
                    ISender sender,
                    [FromServices] ITokenService tokenService) =>
                {
                    var userId = tokenService.GetUserId(httpContext);
                    if (userId.IsError)
                    {
                        return Results.Unauthorized();
                    }

                    var result = await sender.Send(new MarkMyNotificationsAsRead.Command(
                        userId.Value,
                        request.Ids ?? []));

                    return result.Match(
                        data => Results.Ok(data),
                        error => error.ToResponse());
                })
                .Produces<MarkMyNotificationsAsRead.Response>(200)
                .Produces<Error>(400)
                .Produces(401)
                .IncludeInOpenApi()
                .WithName("MarkMyNotificationsAsRead")
                .WithTags("Notifications")
                .RequireAuthorization();
        }
    }
}
