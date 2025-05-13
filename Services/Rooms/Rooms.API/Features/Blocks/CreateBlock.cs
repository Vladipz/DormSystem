using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Block;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Blocks
{
    public static class CreateBlock
    {
        internal sealed class Command : IRequest<ErrorOr<CreateBlockResponse>>
        {
            public Guid FloorId { get; set; }

            public string Label { get; set; } = string.Empty;

            public string GenderRule { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.FloorId)
                    .NotEmpty()
                    .WithMessage("Floor ID is required");

                RuleFor(x => x.Label)
                    .NotEmpty()
                    .MaximumLength(50)
                    .WithMessage("Label cannot be empty and must be less than 50 characters");

                RuleFor(x => x.GenderRule)
                    .NotEmpty()
                    .MaximumLength(20)
                    .WithMessage("Gender rule cannot be empty and must be less than 20 characters");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreateBlockResponse>>
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

            public async Task<ErrorOr<CreateBlockResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Block creation validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<CreateBlockResponse>();
                }

                var floorExists = await _dbContext.Floors.AnyAsync(f => f.Id == request.FloorId, cancellationToken);
                if (!floorExists)
                {
                    return Error.NotFound(
                        code: "Floor.NotFound",
                        description: $"Floor with ID {request.FloorId} was not found.");
                }

                var block = new Block
                {
                    Id = Guid.NewGuid(),
                    FloorId = request.FloorId,
                    Label = request.Label,
                    GenderRule = request.GenderRule,
                };

                _dbContext.Blocks.Add(block);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Block created successfully with ID: {BlockId}", block.Id);

                return new CreateBlockResponse { Id = block.Id };
            }
        }
    }

    public sealed class CreateBlockEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/blocks", async (
                CreateBlockRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<CreateBlock.Command>();
                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Created($"/blocks/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreateBlockResponse>(201)
            .Produces(400)
            .WithName("CreateBlock")
            .WithTags("Blocks")
            .Accepts<CreateBlockRequest>("application/json")
            .IncludeInOpenApi()
            .WithOpenApi(op =>
            {
                op.Summary = "Create a new block";
                return op;
            })
            .RequireAuthorization("AdminOnly");
        }
    }
}