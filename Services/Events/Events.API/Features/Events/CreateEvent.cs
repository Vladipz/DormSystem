using Carter;
using Carter.OpenApi;

using ErrorOr;

using Events.API.Contracts;
using Events.API.Database;
using Events.API.Entities;
using Events.API.Mappins;

using FluentValidation;

using Mapster;

using MediatR;

using static Events.API.Features.Events.CreateEvent;

namespace Events.API.Features.Events
{
    public static class CreateEvent
    {
        internal sealed class Command : IRequest<ErrorOr<Guid>>
        {
            public string Name { get; set; } = string.Empty;

            public DateTime Date { get; set; }

            public string Location { get; set; } = string.Empty;

            public int? NumberOfAttendees { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty();

                RuleFor(x => x.Date).NotEmpty().GreaterThan(DateTime.UtcNow).WithMessage("The event date must be in the future.");

                RuleFor(x => x.Location).NotEmpty();

                RuleFor(x => x.NumberOfAttendees)
                    .Must(x => x == null || x > 0)
                    .WithMessage("Number of attendees must be greater than 0 if provided.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Guid>>
        {
            private readonly EventsDbContext _eventDbContext;
            private readonly IValidator<Command> _validator;

            public Handler(EventsDbContext eventDbContext, IValidator<Command> validator)
            {
                _eventDbContext = eventDbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    return validationResult.ToValidationError<Guid>();
                }

                var newEvent = request.Adapt<DormEvent>();

                _eventDbContext.Events.Add(newEvent);
                await _eventDbContext.SaveChangesAsync(cancellationToken);

                return newEvent.Id;
            }
        }
    }

    public sealed class CreateEventEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/events", async (CreateEventRequest request, IMediator mediator) =>
            {
                var command = request.Adapt<Command>();

                var result = await mediator.Send(command);

                return result.Match(
                    guid => Results.Created($"/events/{guid}", guid),
                    error => error.ToResponse());
            })
            .Produces<Guid>(201)
            .Produces<Error>(400)
            .WithTags("Events")
            .WithName("CreateEvent")
            .Accepts<Command>("application/json")
            .IncludeInOpenApi();
        }
    }
}