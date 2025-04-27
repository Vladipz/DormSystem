using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Place;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

using static Rooms.API.Features.Places.CreatePlace;

namespace Rooms.API.Features.Places
{
    public static class CreatePlace
    {
        internal sealed class Command : IRequest<ErrorOr<CreatePlaceResponse>>
        {
            public Guid RoomId { get; set; }

            public int Index { get; set; }

            public Guid? OccupiedByUserId { get; set; }

            public DateTime? MovedInAt { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RoomId).NotEmpty();
                RuleFor(x => x.Index).GreaterThanOrEqualTo(1);
                RuleFor(x => x.MovedInAt)
                    .NotEmpty()
                    .When(x => x.OccupiedByUserId is not null)
                    .WithMessage("Move-in date is required when the place is occupied.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreatePlaceResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly ILogger<Handler> _logger;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _logger = logger;
                _validator = validator;
            }

            public async Task<ErrorOr<CreatePlaceResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Place creation validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<CreatePlaceResponse>();
                }

                // Check if the room exists
                var roomExists = await _dbContext.Rooms
                    .AnyAsync(r => r.Id == request.RoomId, cancellationToken);

                if (!roomExists)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.RoomId} does not exist.");
                }

                // Check if the index is already taken in this room
                var indexExists = await _dbContext.Places
                    .AnyAsync(p => p.RoomId == request.RoomId && p.Index == request.Index, cancellationToken);

                if (indexExists)
                {
                    return Error.Conflict(
                        code: "Place.IndexAlreadyExists",
                        description: $"Place with index {request.Index} already exists in the room.");
                }

                var place = new Place
                {
                    Id = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    Index = request.Index,
                    OccupiedByUserId = request.OccupiedByUserId,
                    MovedInAt = request.MovedInAt
                };

                _dbContext.Places.Add(place);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Place created successfully with ID: {PlaceId}", place.Id);

                return new CreatePlaceResponse { Id = place.Id };
            }
        }
    }

    public sealed class CreatePlaceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/places", async (
                CreatePlaceRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<Command>();
                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Created($"/places/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreatePlaceResponse>(201)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("Place.Create")
            .WithTags("Places")
            .Accepts<CreatePlaceRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
} 