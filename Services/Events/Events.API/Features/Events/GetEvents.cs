using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;

using MediatR;

using Shared.PagedList;

namespace Events.API.Features.Events
{
    public static class GetEvents
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<EventResponce>>>
        {
            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 10;

            public string? Search { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<EventResponce>>>
        {
            private readonly EventsDbContext _eventDbContext;

            public Handler(EventsDbContext eventDbContext)
            {
                _eventDbContext = eventDbContext;
            }

            public async Task<ErrorOr<PagedResponse<EventResponce>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var eventsQuery = _eventDbContext.Events.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var search = request.Search.Trim();
                    eventsQuery = eventsQuery.Where(e => e.Name.Contains(search));
                }

                var query = eventsQuery
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
                            .Select(p => new ParticipantShortResponse
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

                return PagedResponse<EventResponce>.FromPagedList(pagedEvents);
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
                int pageSize = 10,
                string? search = null) =>
            {
                var query = new GetEvents.Query
                {
                    PageNumber = Math.Max(1, pageNumber),
                    PageSize = Math.Clamp(pageSize, 1, 100),
                    Search = search,
                };

                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    errors => Results.Problem(errors.FirstOrDefault().Description));
            })
            .Produces<PagedResponse<EventResponce>>(StatusCodes.Status200OK)
            .WithName("GetEvents")
            .WithTags("Events")
            .WithSummary("Get paginated list of events")
            .WithDescription("Retrieves a paginated list of events. Supports filtering by page number, page size, and event name search. Returns event details including name, dates, location, and participant count.")
            .IncludeInOpenApi();
        }
    }
}
