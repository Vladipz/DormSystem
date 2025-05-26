using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Data;
using NotificationCore.API.Entities;
using NotificationCore.API.Mappings;

using static NotificationCore.API.Features.UserSettings.GetUserSettings;

namespace NotificationCore.API.Features.UserSettings
{
    public static class GetUserSettings
    {
        public sealed record Query(Guid UserId) : IRequest<ErrorOr<Response>>;

        public sealed record Response(
            Guid userId,
            List<SettingDto> settings,
            List<ChannelDto> channels);

        public sealed record SettingDto(NotificationType Type, bool Enabled);

        public sealed record ChannelDto(NotificationChannel Channel, bool Enabled, string ExternalId);

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<Response>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ErrorOr<Response>> Handle(Query request, CancellationToken cancellationToken)
            {
                var settings = await _db.UserNotificationSettings
                    .Where(s => s.UserId == request.UserId)
                    .Select(s => new SettingDto(s.NotificationType, s.Enabled))
                    .ToListAsync(cancellationToken);

                var channels = await _db.UserChannels
                    .Where(c => c.UserId == request.UserId)
                    .Select(c => new ChannelDto(c.Channel, c.Enabled, c.ExternalReference))
                    .ToListAsync(cancellationToken);

                return new Response(request.UserId, settings, channels);
            }
        }
    }

    public sealed class Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/notifications/settings/{userId:guid}",
                async (Guid userId, [FromServices] ISender sender) =>
            {
                var result = await sender.Send(new Query(userId));

                return result.Match(
                    data => Results.Ok(data),
                    error => error.ToResponse());
            })
            .Produces<Response>(200)
            .Produces<Error>(400)
            .IncludeInOpenApi()
            .WithName("ListUserNotificationSettings")
            .WithTags("Notifications");
        }
    }
}