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
    public static class JoinEvent
    {
        public class Command : IRequest<ErrorOr<Success>>
        {
            public Guid EventId { get; set; }

            public string Token { get; set; } = string.Empty;

            public Guid UserId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
                RuleFor(x => x.Token).NotEmpty();
                RuleFor(x => x.UserId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Success>>
        {
            private readonly EventsDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(EventsDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<Success>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<Success>();
                }

                // Validate the invitation token
                var invitation = await _dbContext.InvitationTokens
                    .FirstOrDefaultAsync(
                        i =>
                        i.Token == request.Token &&
                        i.EventId == request.EventId &&
                        i.IsActive &&
                        i.ExpiresAt > DateTime.UtcNow,
                        cancellationToken);

                if (invitation == null)
                {
                    return Error.NotFound("Invitation.Invalid", "The invitation is invalid or has expired");
                }

                // Check if the event exists
                var eventEntity = await _dbContext.Events
                    .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

                if (eventEntity == null)
                {
                    return Error.NotFound("Event.NotFound", "Event not found");
                }

                // Check if user is already a participant
                var existingParticipation = await _dbContext.EventParticipants
                    .FirstOrDefaultAsync(
                        p =>
                        p.EventId == request.EventId &&
                        p.UserId == request.UserId,
                        cancellationToken);

                if (existingParticipation != null)
                {
                    return Error.Conflict("Event.AlreadyJoined", "You have already joined this event");
                }

                // Check if event has attendance limit and if it's reached
                if (eventEntity.NumberOfAttendees.HasValue)
                {
                    var currentParticipantsCount = await _dbContext.EventParticipants
                        .CountAsync(p => p.EventId == request.EventId, cancellationToken);

                    if (currentParticipantsCount >= eventEntity.NumberOfAttendees.Value)
                    {
                        return Error.Failure("Event.Full", "This event has reached its maximum number of attendees");
                    }
                }

                // Add user to participants
                var participant = new EventParticipant
                {
                    EventId = request.EventId,
                    UserId = request.UserId,
                    JoinedAt = DateTime.UtcNow,
                };

                _dbContext.EventParticipants.Add(participant);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return Result.Success;
            }
        }
    }

    public sealed class JoinEventEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/events/{id}/join", async (
                Guid id,
                JoinEventRequest request,
                IMediator mediator,
                HttpContext httpContext,
                ITokenService tokenService) =>
            {
                var userIdResult = tokenService.GetUserId(httpContext);

                if (userIdResult.IsError)
                {
                    return Results.Unauthorized();
                }

                var command = new JoinEvent.Command
                {
                    EventId = id,
                    Token = request.Token,
                    UserId = userIdResult.Value,
                };

                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Ok(),
                    error => error.ToResponse());
            })
            .WithTags("Events")
            .WithName("JoinEvent")
            .Accepts<JoinEventRequest>("application/json")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404)
            .Produces(409)
            .IncludeInOpenApi()
            .RequireAuthorization();
        }
    }
}