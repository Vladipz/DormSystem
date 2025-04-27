using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Block;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Blocks
{
    public static class UpdateBlock
    {
        internal sealed class Command : IRequest<ErrorOr<UpdateBlockResponse>>
        {
            public Guid Id { get; set; }

            public string Label { get; set; } = string.Empty;

            public string GenderRule { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Block ID must not be empty");

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

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdateBlockResponse>>
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

            public async Task<ErrorOr<UpdateBlockResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Block update validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<UpdateBlockResponse>();
                }

                var block = await _dbContext.Blocks
                    .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

                if (block is null)
                {
                    return Error.NotFound(
                        code: "Block.NotFound",
                        description: $"Block with ID {request.Id} was not found");
                }

                block.Label = request.Label;
                block.GenderRule = request.GenderRule;

                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Block updated successfully with ID: {BlockId}", block.Id);

                return new UpdateBlockResponse { Id = block.Id };
            }
        }
    }

    public sealed class UpdateBlockEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/blocks/{id:guid}", async (
                Guid id,
                UpdateBlockRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<UpdateBlock.Command>();
                command.Id = id;
                
                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Ok(response),
                    error => error.ToResponse());
            })
            .Produces<UpdateBlockResponse>(200)
            .Produces(400)
            .Produces(404)
            .WithName("UpdateBlock")
            .WithTags("Blocks")
            .Accepts<UpdateBlockRequest>("application/json")
            .WithOpenApi(op =>
            {
                op.Summary = "Update a block";
                op.Parameters[0].Description = "Block ID";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
} 