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
    public static class EditEvent
    {
        internal sealed class Command : IRequest<ErrorOr<bool>>
        {
            public Guid Id { get; set; }

            public string Name { get; set; } = string.Empty;

            public DateTime Date { get; set; }

            public string Location { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;

            public int? NumberOfAttendees { get; set; }

            public bool IsPublic { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();

                RuleFor(x => x.Name).NotEmpty();

                RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.UtcNow)
                    .WithMessage("The event date must be in the future.");

                RuleFor(x => x.Location).NotEmpty();

                RuleFor(x => x.Description).MaximumLength(2000)
                    .WithMessage("Description cannot exceed 2000 characters.");

                RuleFor(x => x.NumberOfAttendees)
                    .Must(x => x == null || x > 0)
                    .WithMessage("Number of attendees must be greater than 0 if provided.");
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

                var existingEvent = await _eventDbContext.Events
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (existingEvent is null)
                {
                    return Error.NotFound("Event.NotFound", "The specified event was not found.");
                }

                // Update the event properties
                existingEvent.Name = request.Name;
                existingEvent.Date = request.Date;
                existingEvent.Location = request.Location;
                existingEvent.Description = request.Description;
                existingEvent.NumberOfAttendees = request.NumberOfAttendees;
                existingEvent.IsPublic = request.IsPublic;

                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return true;
            }
        }
    }

    public sealed class EditEventEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/events/{id}", async (Guid id, EditEventRequest request, IMediator mediator) =>
            {
                // TODO: add check for user ID
                var command = request.Adapt<EditEvent.Command>();
                command.Id = id;

                var result = await mediator.Send(command);

                return result.Match(
                    success => Results.NoContent(),
                    error => error.ToResponse());
            })
            .Produces(204)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithTags("Events")
            .WithName("EditEvent")
            .Accepts<EditEventRequest>("application/json")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Update an existing event";
                operation.Parameters[0].Description = "Event ID";
                return operation;
            })
            .IncludeInOpenApi();
        }
    }
}