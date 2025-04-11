// Events.API\Features\Events\GetEventParticipants.cs
using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Mappins;
using Events.API.Services;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class GetEventParticipants
    {
        internal sealed class Query : IRequest<ErrorOr<List<ParticipantDetailedResponse>>>
        {
            public Guid EventId { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<List<ParticipantDetailedResponse>>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly ParticipantEnricher _participantEnricher;

            public Handler(EventsDbContext eventDbContext, ParticipantEnricher participantEnricher)
            {
                _eventDbContext = eventDbContext;
                _participantEnricher = participantEnricher;
            }

            public async Task<ErrorOr<List<ParticipantDetailedResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                // Check if event exists
                var eventExists = await _eventDbContext.Events
                    .AnyAsync(e => e.Id == request.EventId, cancellationToken);

                if (!eventExists)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                // Get participants as short responses
                var shortParticipants = await _eventDbContext.EventParticipants
                    .Where(p => p.EventId == request.EventId)
                    .Select(p => new ParticipantShortResponse
                    {
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                    })
                    .ToListAsync(cancellationToken);

                // Enrich participants with user information, converting to detailed responses
                var detailedParticipants = await _participantEnricher.EnrichParticipantsAsync(shortParticipants);

                return detailedParticipants;
            }
        }
    }

    public sealed class GetEventParticipantsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/events/{eventId}/participants", async (Guid eventId, IMediator mediator) =>
            {
                var query = new GetEventParticipants.Query
                {
                    EventId = eventId,
                };

                var result = await mediator.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    errors => errors.ToResponse());
            })
            .Produces<List<ParticipantDetailedResponse>>(200)
            .Produces<ErrorOr.Error>(404)
            .WithTags("EventParticipants")
            .WithName("GetEventParticipants")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get all participants for an event";
                operation.Parameters[0].Description = "Event ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}