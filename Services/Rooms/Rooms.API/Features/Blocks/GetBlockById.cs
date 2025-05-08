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
    public static class GetBlockById
    {
        internal sealed class Query : IRequest<ErrorOr<BlockDetailsResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Block ID must not be empty");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<BlockDetailsResponse>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<BlockDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<BlockDetailsResponse>();
                }

                var block = await _db.Blocks
                    .AsNoTracking()
                    .Include(b => b.Floor)
                    .ThenInclude(f => f.Building)
                    .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

                if (block is null)
                {
                    return Error.NotFound(
                        code: "Block.NotFound",
                        description: $"Block with ID {request.Id} was not found");
                }

                var response = new BlockDetailsResponse
                {
                    Id = block.Id,
                    FloorId = block.FloorId,
                    Label = block.Label,
                    GenderRule = block.GenderRule,
                    FloorNumber = block.Floor.Number.ToString(),
                    BuildingName = block.Floor.Building.Name
                };

                return response;
            }
        }
    }

    public sealed class GetBlockByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/blocks/{id:guid}", async (
                Guid id,
                ISender sender) =>
            {
                var query = new GetBlockById.Query { Id = id };
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<BlockDetailsResponse>(200)
            .Produces(404)
            .WithName("GetBlockById")
            .WithTags("Blocks")
            .WithOpenApi(op =>
            {
                op.Summary = "Get block details by ID";
                op.Parameters[0].Description = "Block ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}