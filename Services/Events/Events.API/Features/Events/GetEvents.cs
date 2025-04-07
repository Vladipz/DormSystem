using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Shared;

using MediatR;

namespace Events.API.Features.Events
{
    public static class GetEvents
    {
        internal sealed class Query : IRequest<ErrorOr<PagedEventsResponse>>
        {
            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 10;
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedEventsResponse>>
        {
            private readonly EventsDbContext _eventDbContext;

            public Handler(EventsDbContext eventDbContext)
            {
                _eventDbContext = eventDbContext;
            }

            public async Task<ErrorOr<PagedEventsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var query = _eventDbContext.Events
                    .OrderByDescending(e => e.Date)
                    .Select(static x => new EventResponce
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Date = x.Date,
                        Location = x.Location,
                        NumberOfAttendees = x.NumberOfAttendees,
                        OwnerId = x.OwnerId,
                        IsPublic = x.IsPublic,
                        LastParticipants = x.Participants
                            .OrderByDescending(p => p.JoinedAt)
                            .Take(3)
                            .Select(p => new ParticipantResponse
                            {
                                UserId = p.UserId,
                                JoinedAt = p.JoinedAt,
                            }).ToList(),
                    });

                var pagedEvents = await PagedList<EventResponce>.CreateAsync(
                    query,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                return PagedEventsResponse.FromPagedList(pagedEvents);
            }
        }
    }

    public class GetEventsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/events", async (
                ISender sender,
                int pageNumber = 1,
                int pageSize = 10) =>
            {
                var query = new GetEvents.Query
                {
                    PageNumber = Math.Max(1, pageNumber),
                    PageSize = Math.Clamp(pageSize, 1, 100),
                };

                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    errors => Results.Problem(errors.FirstOrDefault().Description));
            })
            .Produces<PagedEventsResponse>(200)
            .WithName("GetEvents")
            .WithTags("Events")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get paginated list of events";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}