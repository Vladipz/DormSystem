using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Entities;
using Events.API.Mappins;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.TokenService.Services;

namespace Events.API.Features.Events
{
    public static class GenerateEventInvitation
    {
        public class Command : IRequest<ErrorOr<InvitationResult>>
        {
            public Guid EventId { get; set; }

            public Guid OwnerId { get; set; }
        }

        public class InvitationResult
        {
            public string Token { get; set; } = string.Empty;

            public DateTime ExpiresAt { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<InvitationResult>>
        {
            private const int ExpiresInDays = 30;
            private readonly EventsDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(EventsDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<InvitationResult>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.Errors.ConvertAll(e => Error.Validation(e.PropertyName, e.ErrorMessage));
                }

                // Check if the event exists
                var eventEntity = await _dbContext.Events
                    .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

                if (eventEntity == null)
                {
                    return Error.NotFound("Event.NotFound", "Event not found");
                }

                if (eventEntity.IsPublic)
                {
                    // Для публічних подій — не потрібен токен, може генерувати будь-хто
                    return new InvitationResult
                    {
                        Token = string.Empty,
                        ExpiresAt = DateTime.UtcNow.AddDays(ExpiresInDays), // не використовується
                    };
                }

                // Для приватних — тільки власник
                if (eventEntity.OwnerId != request.OwnerId)
                {
                    return Error.Forbidden("Event.NotOwner", "Only event owner can generate invitations for private events");
                }

                // Generate a unique token
                string token = Guid.NewGuid().ToString("N");

                // Set expiration to 30 days from now
                var expiresAt = DateTime.UtcNow.AddDays(ExpiresInDays);

                // Save to database
                var invitationToken = new InvitationToken
                {
                    Token = token,
                    EventId = request.EventId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt,
                    IsActive = true,
                };

                _dbContext.InvitationTokens.Add(invitationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new InvitationResult
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                };
            }
        }
    }

    public sealed class GenerateEventInvitationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet(
                "/events/{id}/generate-invitation",
                async (
                    Guid id,
                    IMediator mediator,
                    HttpContext httpContext,
                    ITokenService tokenService) =>
                {
                    var dbContext = httpContext.RequestServices.GetRequiredService<EventsDbContext>();
                    var eventEntity = await dbContext.Events.FirstOrDefaultAsync(e => e.Id == id);
                    if (eventEntity == null)
                    {
                        return Results.NotFound();
                    }

                    var userIdResult = tokenService.GetUserId(httpContext);
                    Guid? ownerId = null;
                    if (!eventEntity.IsPublic)
                    {
                    if (userIdResult.IsError)
                    {
                        return Results.Unauthorized();
                        }

                    ownerId = userIdResult.Value;
                    }

                    var command = new GenerateEventInvitation.Command
                    {
                        EventId = id,
                        OwnerId = ownerId ?? Guid.Empty,
                    };

                    var result = await mediator.Send(command);

                    return result.Match(
                        invitationData =>
                        {
                            string invitationLink = eventEntity.IsPublic
                                ? $"/events/{id}/invite"
                                : $"/events/{id}/invite?token={invitationData.Token}";
                            return Results.Ok(new GenerateInvitationResponse
                            {
                                InvitationLink = invitationLink,
                                Token = invitationData.Token,
                                ExpiresAt = invitationData.ExpiresAt,
                            });
                        },
                        error => error.ToResponse());
                })
                .WithTags("Events")
                .WithName("GenerateEventInvitation")
                .Produces<GenerateInvitationResponse>(200)
                .Produces(401)
                .Produces(403)
                .Produces(404)
                .IncludeInOpenApi();
        }
    }
}