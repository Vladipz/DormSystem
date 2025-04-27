using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Floor;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Floors
{
    public static class DeleteFloor
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedFloorResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Floor ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedFloorResponse>>
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

            public async Task<ErrorOr<DeletedFloorResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    _logger.LogWarning(
                       "Floor deletion validation failed: {Errors}",
                       string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

                    return validation.ToValidationError<DeletedFloorResponse>();
                }

                var floor = await _dbContext.Floors
                    .Include(f => f.Blocks)
                    .FirstOrDefaultAsync(f => f.Id == request.Id, ct);

                if (floor is null)
                {
                    return Error.NotFound(
                        code: "Floor.NotFound",
                        description: $"Floor with ID {request.Id} was not found.");
                }

                if (floor.Blocks.Any())
                {
                    return Error.Conflict(
                        code: "Floor.HasBlocks",
                        description: "Cannot delete a floor that has blocks. Remove blocks first.");
                }

                _dbContext.Floors.Remove(floor);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogInformation("Floor with ID {FloorId} deleted successfully", floor.Id);

                return new DeletedFloorResponse { Id = floor.Id };
            }
        }
    }

    public sealed class DeleteFloorEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/floors/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteFloor.Command { Id = id };
                var result = await sender.Send(command);

                return result.Match(
                    deleted => Results.Ok(deleted),
                    error => error.ToResponse());
            })
            .Produces<DeletedFloorResponse>(200)
            .Produces(404)
            .Produces(409)
            .WithName("DeleteFloor")
            .WithTags("Floors")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete floor by ID";
                op.Parameters[0].Description = "Floor ID";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}