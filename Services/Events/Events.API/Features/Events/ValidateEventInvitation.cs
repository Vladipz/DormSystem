using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Mappins;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class ValidateEventInvitation
    {
        public class Query : IRequest<ErrorOr<EventDetailsResponse>>
        {
            public Guid EventId { get; set; }

            public string Token { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<EventDetailsResponse>>
        {
            private readonly EventsDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(EventsDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<EventDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<EventDetailsResponse>();
                }

                // Get event details
                var eventEntity = await _dbContext.Events
                    .Where(e => e.Id == request.EventId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventEntity == null)
                {
                    return Error.NotFound("Event.NotFound", "Event not found");
                }

                if (eventEntity.IsPublic)
                {
                    // Для публічних івентів токен не потрібен, повертаємо дані події
                    var participantsCount = await _dbContext.EventParticipants
                        .CountAsync(p => p.EventId == request.EventId, cancellationToken);
                    var eventDetails = eventEntity.Adapt<EventDetailsResponse>();
                    eventDetails.CurrentParticipantsCount = participantsCount;
                    return eventDetails;
                }

                // Для приватних івентів — перевіряємо токен
                if (string.IsNullOrWhiteSpace(request.Token))
                {
                    return Error.Validation("Token.Required", "This event requires an invitation token to join");
                }

                // Check if token exists and is valid
                var invitation = await _dbContext.InvitationTokens
                    .FirstOrDefaultAsync(i =>
                        i.Token == request.Token &&
                        i.EventId == request.EventId &&
                        i.IsActive &&
                        i.ExpiresAt > DateTime.UtcNow,
                        cancellationToken);

                if (invitation == null)
                {
                    return Error.NotFound("Invitation.Invalid", "The invitation is invalid or has expired");
                }

                // Get current participants count
                var participantsCountPrivate = await _dbContext.EventParticipants
                    .CountAsync(p => p.EventId == request.EventId, cancellationToken);
                var eventDetailsPrivate = eventEntity.Adapt<EventDetailsResponse>();
                eventDetailsPrivate.CurrentParticipantsCount = participantsCountPrivate;
                return eventDetailsPrivate;
            }
        }
    }

    public sealed class ValidateEventInvitationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/events/{id}/validate-invitation", async (
                Guid id,
                string token,
                IMediator mediator) =>
            {
                var query = new ValidateEventInvitation.Query
                {
                    EventId = id,
                    Token = token,
                };

                var result = await mediator.Send(query);

                return result.Match(
                    eventDetails => Results.Ok(eventDetails),
                    error => error.ToResponse());
            })
            .WithTags("Events")
            .WithName("ValidateEventInvitation")
            .Produces<EventDetailsResponse>(200)
            .Produces(400)
            .Produces(404)
            .IncludeInOpenApi();
        }
    }
}