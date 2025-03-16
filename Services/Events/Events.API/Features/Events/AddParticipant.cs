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

namespace Events.API.Features.Events
{
    public static class AddParticipant
    {
        internal sealed class Command : IRequest<ErrorOr<bool>>
        {
            public Guid EventId { get; set; }

            public Guid UserId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.EventId).NotEmpty();
                RuleFor(x => x.UserId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<bool>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly IValidator<Command> _validator;

            public Handler(EventsDbContext eventDbContext, IValidator<Command> validator)
            {
                _eventDbContext = eventDbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<bool>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<bool>();
                }

                // Check if event exists
                var eventEntity = await _eventDbContext.Events
                    .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

                if (eventEntity is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                // Check if participant already exists
                var existingParticipant = await _eventDbContext.EventParticipants
                    .FirstOrDefaultAsync(p => p.EventId == request.EventId && p.UserId == request.UserId, cancellationToken);

                if (existingParticipant is not null)
                {
                    return Error.Conflict("Participant.AlreadyExists", "This user is already a participant in the event.");
                }

                // Add participant
                var participant = new EventParticipant
                {
                    Id = Guid.NewGuid(),
                    EventId = request.EventId,
                    UserId = request.UserId,
                    JoinedAt = DateTime.UtcNow,
                };

                _eventDbContext.EventParticipants.Add(participant);
                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }

    public sealed class AddParticipantEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/events/{eventId}/participants", async (Guid eventId, AddParticipantRequest request, IMediator mediator) =>
            {
                var command = new AddParticipant.Command
                {
                    EventId = eventId,
                    UserId = request.UserId,
                };

                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.Created($"/events/{eventId}/participants/{request.UserId}", success),
                    error => error.ToResponse());
            })
            .Produces(201)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .Produces<Error>(409)
            .WithTags("EventParticipants")
            .WithName("AddParticipant")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Add a participant to an event";
                operation.Parameters[0].Description = "Event ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}