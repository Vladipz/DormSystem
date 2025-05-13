using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Services;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class GetEvent
    {
        internal sealed class Query : IRequest<ErrorOr<EventDetailsResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<EventDetailsResponse>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly ParticipantEnricher _participantEnricher;

            public Handler(EventsDbContext eventDbContext, ParticipantEnricher participantEnricher)
            {
                _eventDbContext = eventDbContext;
                _participantEnricher = participantEnricher;
            }

            public async Task<ErrorOr<EventDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var eventEntity = await _eventDbContext.Events
                    .Where(x => x.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventEntity is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                // Get current participants count
                var participantsCount = await _eventDbContext.EventParticipants
                    .CountAsync(p => p.EventId == request.Id, cancellationToken);

                // Get all participants for the event
                var participants = await _eventDbContext.EventParticipants
                    .Where(p => p.EventId == request.Id)
                    .OrderByDescending(p => p.JoinedAt)
                    .Select(p => new ParticipantShortResponse
                    {
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                    })
                    .ToListAsync(cancellationToken);

                var detailedParticipants = await _participantEnricher.EnrichParticipantsAsync(participants);

                // Create event details response
                var eventDetails = new EventDetailsResponse
                {
                    Id = eventEntity.Id,
                    OwnerId = eventEntity.OwnerId,
                    Name = eventEntity.Name,
                    Date = eventEntity.Date,
                    Location = eventEntity.Location,
                    Description = eventEntity.Description,
                    NumberOfAttendees = eventEntity.NumberOfAttendees,
                    IsPublic = eventEntity.IsPublic,
                    CurrentParticipantsCount = participantsCount,
                    Participants = detailedParticipants,
                    BuildingId = eventEntity.BuildingId,
                    RoomId = eventEntity.RoomId,
                };

                return eventDetails;
            }
        }
    }

    public class GetEventEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/events/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetEvent.Query { Id = id };
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    errors => Results.NotFound(errors));
            })
            .Produces<EventDetailsResponse>(200)
            .ProducesProblem(404)
            .WithName("GetEvent")
            .WithTags("Events")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get event by ID";
                operation.Parameters[0].Description = "Event ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}