using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using NotificationCore.API.Data;
using NotificationCore.API.Entities;
using NotificationCore.API.Mappings;
using NotificationCore.API.Services;

namespace NotificationCore.API.Features.Notifications
{
    public static class CreateTestNotification
    {
        public sealed record Request(List<Guid> UserIds, string Title, string Message, int Count = 1);

        public sealed record Command(IReadOnlyCollection<Guid> UserIds, string Title, string Message, int Count)
            : IRequest<ErrorOr<Response>>;

        public sealed record Response(int CreatedCount, List<Guid> NotificationIds);

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Response>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IInAppNotificationDispatcher _dispatcher;

            public Handler(ApplicationDbContext db, IInAppNotificationDispatcher dispatcher)
            {
                _db = db;
                _dispatcher = dispatcher;
            }

            public async Task<ErrorOr<Response>> Handle(Command request, CancellationToken cancellationToken)
            {
                var userIds = request.UserIds
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList();

                if (userIds.Count == 0)
                {
                    return Error.Validation("Notifications.Test.UserIds", "At least one userId is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return Error.Validation("Notifications.Test.Title", "Title is required.");
                }

                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Error.Validation("Notifications.Test.Message", "Message is required.");
                }

                var count = request.Count <= 0 ? 1 : Math.Min(request.Count, 100);
                var notifications = new List<Notification>();

                for (var iteration = 0; iteration < count; iteration++)
                {
                    foreach (var userId in userIds)
                    {
                        notifications.Add(new Notification
                        {
                            Id = Guid.NewGuid(),
                            UserId = userId,
                            Title = request.Title,
                            Message = count == 1 ? request.Message : $"{request.Message} #{iteration + 1}",
                            Type = NotificationType.Events,
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false,
                        });
                    }
                }

                _db.Notifications.AddRange(notifications);
                await _db.SaveChangesAsync(cancellationToken);
                await _dispatcher.DispatchAsync(notifications, cancellationToken);

                return new Response(notifications.Count, notifications.Select(notification => notification.Id).ToList());
            }
        }
    }

    public sealed class CreateTestNotificationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost(
                "/notifications/test/in-app",
                async (
                    [FromBody] CreateTestNotification.Request request,
                    ISender sender) =>
                {
                    var result = await sender.Send(new CreateTestNotification.Command(
                        request.UserIds ?? [],
                        request.Title,
                        request.Message,
                        request.Count));

                    return result.Match(
                        data => Results.Ok(data),
                        error => error.ToResponse());
                })
                .Produces<CreateTestNotification.Response>(200)
                .Produces<Error>(400)
                .IncludeInOpenApi()
                .WithName("CreateTestNotification")
                .WithTags("Notifications");
        }
    }
}
