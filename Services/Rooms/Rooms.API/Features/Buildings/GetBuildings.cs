using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Building;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

using Shared.PagedList;

namespace Rooms.API.Features.Buildings
{
    public static class GetBuildings
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<BuildingsResponse>>>
        {
            public bool? IsActive { get; set; }

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

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<BuildingsResponse>>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<BuildingsResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<BuildingsResponse>>();
                }

                IQueryable<Building> baseQuery = _dbContext.Buildings.AsNoTracking();

                if (request.IsActive.HasValue)
                {
                    baseQuery = baseQuery.Where(b => b.IsActive == request.IsActive);
                }

                var items = baseQuery
                    .ProjectToType<BuildingsResponse>()
                    .OrderBy(b => b.Name);

                var pagedList = await PagedList<BuildingsResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                return PagedResponse<BuildingsResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class GetBuildingsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/buildings", async (
                [AsParameters] GetBuildings.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<BuildingsResponse>>(200)
            .Produces<Error>(400)
            .WithName("Buildings.GetBuildings")
            .WithTags("Buildings")
            .AllowAnonymous()
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of buildings";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}