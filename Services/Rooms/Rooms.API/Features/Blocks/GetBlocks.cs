using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Block;
using Rooms.API.Data;
using Rooms.API.Mappings;

using Shared.PagedList;

namespace Rooms.API.Features.Blocks
{
    public static class GetBlocks
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<BlocksResponse>>>
        {
            public Guid? FloorId { get; set; }

            public int Page { get; set; } = 1;

            public int PageSize { get; set; } = 20;
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.Page).GreaterThan(0);
                RuleFor(q => q.PageSize).InclusiveBetween(1, 100);
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<BlocksResponse>>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<BlocksResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<BlocksResponse>>();
                }

                IQueryable<Entities.Block> baseQuery = _db.Blocks
                    .AsNoTracking()
                    .Include(b => b.Rooms);

                if (request.FloorId is not null)
                {
                    baseQuery = baseQuery.Where(b => b.FloorId == request.FloorId);
                }

                var items = baseQuery
                    .Select(b => new BlocksResponse
                    {
                        Id = b.Id,
                        FloorId = b.FloorId,
                        Label = b.Label,
                        GenderRule = b.GenderRule,
                        RoomsCount = b.Rooms.Count,
                    })
                    .OrderBy(b => b.Label);

                var pagedList = await PagedList<BlocksResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                return PagedResponse<BlocksResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class GetBlocksEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/blocks", async (
                [AsParameters] GetBlocks.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<BlocksResponse>>(200)
            .Produces(400)
            .WithName("GetBlocks")
            .WithTags("Blocks")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of blocks";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}