using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Database;
using Events.API.Mappins;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Events.API.Features.Events
{
    public static class RemoveParticipant
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

                // Check if participant exists
                var participant = await _eventDbContext.EventParticipants
                    .FirstOrDefaultAsync(p => p.EventId == request.EventId && p.UserId == request.UserId, cancellationToken);

                if (participant is null)
                {
                    return Error.NotFound("Participant.NotFound", "The specified participant was not found in this event.");
                }

                // Remove participant
                _eventDbContext.EventParticipants.Remove(participant);
                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }

    public sealed class RemoveParticipantEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/events/{eventId}/participants/{userId}", async (Guid eventId, Guid userId, IMediator mediator) =>
            {
                var command = new RemoveParticipant.Command
                {
                    EventId = eventId,
                    UserId = userId,
                };

                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.NoContent(),
                    error => error.ToResponse());
            })
            .Produces(204)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithTags("EventParticipants")
            .WithName("RemoveParticipant")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Remove a participant from an event";
                operation.Parameters[0].Description = "Event ID";
                operation.Parameters[1].Description = "User ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}