using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Database;
using Events.API.Mappins;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class DeleteEvent
    {
        internal sealed class Command : IRequest<ErrorOr<bool>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<bool>>
        {
            private readonly EventsDbContext _eventDbContext;

            public Handler(EventsDbContext eventDbContext)
            {
                _eventDbContext = eventDbContext;
            }

            public async Task<ErrorOr<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var eventToDelete = await _eventDbContext.Events
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (eventToDelete is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                _eventDbContext.Events.Remove(eventToDelete);
                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }

    public sealed class DeleteEventEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/events/{id}", async (Guid id, IMediator mediator) =>
            {
                var command = new DeleteEvent.Command { Id = id };
                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.NoContent(),
                    error => error.ToResponse());
            })
            .Produces(204)
            .Produces<Error>(404)
            .WithTags("Events")
            .WithName("DeleteEvent")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Delete an existing event";
                operation.Parameters[0].Description = "Event ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}