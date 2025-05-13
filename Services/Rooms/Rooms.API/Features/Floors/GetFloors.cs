using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Floor;
using Rooms.API.Data;
using Rooms.API.Mappings;

using Shared.PagedList;

namespace Rooms.API.Features.Floors
{
    public static class GetFloors
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<FloorsResponse>>>
        {
            public Guid? BuildingId { get; set; }

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

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<FloorsResponse>>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<FloorsResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<FloorsResponse>>();
                }

                IQueryable<Entities.Floor> baseQuery = _db.Floors
                    .AsNoTracking()
                    .Include(f => f.Building)
                    .Include(f => f.Blocks);

                if (request.BuildingId is not null)
                {
                    baseQuery = baseQuery.Where(f => f.BuildingId == request.BuildingId);
                }

                var items = baseQuery
                    .Select(f => new FloorsResponse
                    {
                        Id = f.Id,
                        BuildingId = f.BuildingId,
                        Number = f.Number,
                        BlocksCount = f.Blocks.Count,
                        BuildingName = f.Building.Name,
                    })
                    .OrderBy(f => f.Number);

                var pagedList = await PagedList<FloorsResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                return PagedResponse<FloorsResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class GetFloorsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/floors", async (
                [AsParameters] GetFloors.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<FloorsResponse>>(200)
            .Produces(400)
            .WithName("GetFloors")
            .WithTags("Floors")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of floors";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}