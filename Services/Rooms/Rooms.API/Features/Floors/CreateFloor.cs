using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Floor;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Floors
{
    public static class CreateFloor
    {
        internal sealed class Command : IRequest<ErrorOr<CreateFloorResponse>>
        {
            public Guid BuildingId { get; set; }

            public int Number { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.BuildingId)
                    .NotEmpty()
                    .WithMessage("Building ID is required");

                RuleFor(x => x.Number)
                    .GreaterThan(0)
                    .WithMessage("Floor number must be greater than 0");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreateFloorResponse>>
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

            public async Task<ErrorOr<CreateFloorResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Floor creation validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<CreateFloorResponse>();
                }

                // Check if building exists
                var buildingExists = await _dbContext.Buildings
                    .AnyAsync(b => b.Id == request.BuildingId, cancellationToken);

                if (!buildingExists)
                {
                    return Error.NotFound(
                        code: "Building.NotFound",
                        description: $"Building with ID {request.BuildingId} was not found.");
                }

                // Check if floor with same number already exists in this building
                var floorExists = await _dbContext.Floors
                    .AnyAsync(f => f.BuildingId == request.BuildingId && f.Number == request.Number, cancellationToken);

                if (floorExists)
                {
                    return Error.Conflict(
                        code: "Floor.AlreadyExists",
                        description: $"Floor with number {request.Number} already exists in this building.");
                }

                var floor = new Floor
                {
                    Id = Guid.NewGuid(),
                    BuildingId = request.BuildingId,
                    Number = request.Number,
                    BlocksCount = 0
                };

                _dbContext.Floors.Add(floor);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Floor created successfully with ID: {FloorId}", floor.Id);

                return new CreateFloorResponse { Id = floor.Id };
            }
        }
    }

    public sealed class CreateFloorEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/floors", async (
                CreateFloorRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<CreateFloor.Command>();
                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Created($"/floors/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreateFloorResponse>(201)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("CreateFloor")
            .WithTags("Floors")
            .Accepts<CreateFloorRequest>("application/json")
            .WithOpenApi(op =>
            {
                op.Summary = "Create a new floor";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}