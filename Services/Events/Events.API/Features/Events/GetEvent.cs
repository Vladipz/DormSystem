using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class GetEvent
    {
        internal sealed class Query : IRequest<ErrorOr<EventResponce>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<EventResponce>>
        {
            private readonly EventsDbContext _eventDbContext;

            public Handler(EventsDbContext eventDbContext)
            {
                _eventDbContext = eventDbContext;
            }

            public async Task<ErrorOr<EventResponce>> Handle(Query request, CancellationToken cancellationToken)
            {
                var eventResponce = await _eventDbContext.Events.Where(x => x.Id == request.Id).Select(x => new EventResponce
                {
                    Id = x.Id,
                    Name = x.Name,
                    Date = x.Date,
                    Location = x.Location,
                    NumberOfAttendees = x.NumberOfAttendees,
                }).FirstOrDefaultAsync(cancellationToken);

                return eventResponce is null ? Error.NotFound() : eventResponce;
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
            .Produces<EventResponce>(200)
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