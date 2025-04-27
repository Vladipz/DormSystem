using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Floor;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Floors
{
    public static class UpdateFloor
    {
        internal sealed class Command : IRequest<ErrorOr<UpdateFloorResponse>>
        {
            public Guid Id { get; set; }

            public int Number { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Floor ID must not be empty");

                RuleFor(x => x.Number)
                    .GreaterThan(0)
                    .WithMessage("Floor number must be greater than 0");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdateFloorResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            private readonly ILogger<Handler> _logger;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator, ILogger<Handler> logger)
            {
                _dbContext = dbContext;
                _validator = validator;
                _logger = logger;
            }

            public async Task<ErrorOr<UpdateFloorResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Floor update validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<UpdateFloorResponse>();
                }

                var floor = await _dbContext.Floors
                    .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

                if (floor is null)
                {
                    return Error.NotFound(
                        code: "Floor.NotFound",
                        description: $"Floor with ID {request.Id} was not found");
                }

                // Check if another floor with the same number already exists in this building
                var conflictingFloor = await _dbContext.Floors
                    .FirstOrDefaultAsync(
                        f =>
                        f.BuildingId == floor.BuildingId &&
                        f.Number == request.Number &&
                        f.Id != request.Id,
                        cancellationToken);

                if (conflictingFloor is not null)
                {
                    return Error.Conflict(
                        code: "Floor.NumberConflict",
                        description: $"Floor with number {request.Number} already exists in this building");
                }

                floor.Number = request.Number;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Floor updated successfully with ID: {FloorId}", floor.Id);

                return new UpdateFloorResponse { Id = floor.Id };
            }
        }
    }

    public sealed class UpdateFloorEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/floors/{id:guid}", async (
                Guid id,
                UpdateFloorRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<UpdateFloor.Command>();
                command.Id = id;

                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Ok(response),
                    error => error.ToResponse());
            })
            .Produces<UpdateFloorResponse>(200)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("UpdateFloor")
            .WithTags("Floors")
            .Accepts<UpdateFloorRequest>("application/json")
            .WithOpenApi(op =>
            {
                op.Summary = "Update a floor";
                op.Parameters[0].Description = "Floor ID";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}