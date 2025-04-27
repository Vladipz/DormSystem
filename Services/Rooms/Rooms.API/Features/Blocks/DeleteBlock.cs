using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Block;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Blocks
{
    public static class DeleteBlock
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedBlockResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Block ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedBlockResponse>>
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

            public async Task<ErrorOr<DeletedBlockResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    _logger.LogWarning(
                       "Block deletion validation failed: {Errors}",
                       string.Join(", ", validation.Errors.Select(e => e.ErrorMessage)));

                    return validation.ToValidationError<DeletedBlockResponse>();
                }

                var block = await _dbContext.Blocks
                    .Include(b => b.Rooms)
                    .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

                if (block is null)
                {
                    return Error.NotFound(
                        code: "Block.NotFound",
                        description: $"Block with ID {request.Id} was not found.");
                }

                if (block.Rooms.Any())
                {
                    return Error.Conflict(
                        code: "Block.HasRooms",
                        description: "Cannot delete a block that has rooms. Remove rooms first.");
                }

                _dbContext.Blocks.Remove(block);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogInformation("Block with ID {BlockId} deleted successfully", block.Id);

                return new DeletedBlockResponse { Id = block.Id };
            }
        }
    }

    public sealed class DeleteBlockEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/blocks/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteBlock.Command { Id = id };
                var result = await sender.Send(command);

                return result.Match(
                    deleted => Results.Ok(deleted),
                    error => error.ToResponse());
            })
            .Produces<DeletedBlockResponse>(200)
            .Produces(404)
            .Produces(409)
            .WithName("DeleteBlock")
            .WithTags("Blocks")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete block by ID";
                op.Parameters[0].Description = "Block ID";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
} 