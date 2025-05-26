using System.Collections.ObjectModel;

using Carter;
using Carter.OpenApi;

using ErrorOr;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using NotificationCore.API.Contracts.Inspection;
using NotificationCore.API.Data;
using NotificationCore.API.Entities;
using NotificationCore.API.Mappings;

using Shared.TokenService.Services;

namespace NotificationCore.API.Features.UserSettings
{
    public static class UpdateNotificationPreferences
    {
        public record Command(Guid userId, Collection<UpdateTypeSetting> types, Collection<UpdateChannelSetting> channels)
            : IRequest<ErrorOr<Success>>;

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Success>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db) => _db = db;

            public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
            {
                // --- Update or insert notification types ---
                foreach (var type in request.types)
                {
                    var setting = await _db.UserNotificationSettings
                        .FirstOrDefaultAsync(s => s.UserId == request.userId && s.NotificationType == type.Type, cancellationToken);

                    if (setting is null)
                    {
                        _db.UserNotificationSettings.Add(new UserNotificationSetting
                        {
                            Id = Guid.NewGuid(),
                            UserId = request.userId,
                            NotificationType = type.Type,
                            Enabled = type.Enabled,
                        });
                    }
                    else
                    {
                        setting.Enabled = type.Enabled;
                    }
                }

                // --- Update or insert channels ---
                foreach (var channel in request.channels)
                {
                    var setting = await _db.UserChannels
                        .FirstOrDefaultAsync(s => s.UserId == request.userId && s.Channel == channel.Channel, cancellationToken);

                    if (setting is null)
                    {
                        _db.UserChannels.Add(new UserChannel
                        {
                            Id = Guid.NewGuid(),
                            UserId = request.userId,
                            Channel = channel.Channel,
                            Enabled = channel.Enabled,
                            ExternalReference = string.Empty, // can be updated later by user
                        });
                    }
                    else
                    {
                        setting.Enabled = channel.Enabled;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.Success;
            }
        }
    }

    public sealed class UpdateNotificationPreferencesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/notifications/settings/me", async(
                [FromBody] UpdateNotificationPreferencesRequest request,
                HttpContext httpContext,
                ISender sender,
                [FromServices] ITokenService tokenService) =>
            {
                var userId = tokenService.GetUserId(httpContext);
                if (userId.IsError)
                {
                    return Results.Unauthorized();
                }

                var command = new UpdateNotificationPreferences.Command(userId.Value, request.Types, request.Channels);

                var result = await sender.Send(command);
                return result.Match(
                    _ => Results.NoContent(),
                    error => error.ToResponse());
            })
            .WithTags("Notification")
            .WithName("UpdateNotificationPreferences")
            .WithSummary("Update current user's notification preferences")
            .Produces(204)
            .Produces<Error>(400)
            .IncludeInOpenApi()
            .RequireAuthorization();
        }
    }
}