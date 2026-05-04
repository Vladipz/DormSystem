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

        internal sealed class Handler(
            EventsDbContext eventDbContext,
            ParticipantEnricher participantEnricher,
            IMotivationFakeClient motivationFakeClient)
            : IRequestHandler<Query, ErrorOr<EventDetailsResponse>>
        {
            public async Task<ErrorOr<EventDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var eventEntity = await eventDbContext.Events
                    .AsNoTracking()
                    .Where(x => x.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (eventEntity is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                // Get all participants for the event
                var participants = await eventDbContext.EventParticipants
                    .AsNoTracking()
                    .Where(p => p.EventId == request.Id)
                    .OrderByDescending(p => p.JoinedAt)
                    .Select(p => new ParticipantShortResponse
                    {
                        UserId = p.UserId,
                        JoinedAt = p.JoinedAt,
                    })
                    .ToListAsync(cancellationToken);

                var enrichParticipantsTask = participantEnricher.EnrichParticipantsAsync(participants);
                var motivationalPhraseTask = motivationFakeClient.GetPhraseAsync(cancellationToken);
                await Task.WhenAll(enrichParticipantsTask, motivationalPhraseTask);
                var detailedParticipants = await enrichParticipantsTask;
                var motivationalPhrase = await motivationalPhraseTask;

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
                    CurrentParticipantsCount = participants.Count,
                    Participants = detailedParticipants,
                    BuildingId = eventEntity.BuildingId,
                    RoomId = eventEntity.RoomId,
                    MotivationalPhrase = motivationalPhrase,
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
            .IncludeInOpenApi();
        }
    }
}
