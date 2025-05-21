using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Room;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

using Shared.PagedList;

namespace Rooms.API.Features.Rooms
{
    public static class GetRooms
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<RoomsResponse>>>
        {
            public Guid? BlockId { get; set; }

            public Guid? BuildingId { get; set; }

            public Guid? FloorId { get; set; }

            public bool? OnlyBlockless { get; set; }

            public RoomStatus? Status { get; set; }

            public RoomType? RoomType { get; set; }

            public int? Page { get; set; }

            public int? PageSize { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.Page)
                      .Must(p => p is null || p > 0)
                      .WithMessage("Page must be > 0");

                RuleFor(q => q.PageSize)
                    .Must(s => s is null || (s >= 1 && s <= 100))
                    .WithMessage("PageSize must be 1-100");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<RoomsResponse>>>
        {
            private const int MaxUnpaged = 500;

            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<RoomsResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<RoomsResponse>>();
                }

                IQueryable<Room> baseQuery = _db.Rooms
                    .Include(r => r.Block).ThenInclude(b => b.Floor)
                    .Include(r => r.Floor).ThenInclude(f => f.Building)
                    .Include(r => r.Building)
                    .AsNoTracking();

                // --- Filters ---
                if (request.BlockId is not null)
                {
                    baseQuery = baseQuery.Where(r => r.BlockId == request.BlockId);
                }

                if (request.FloorId is not null)
                {
                    baseQuery = baseQuery.Where(r =>
                        r.FloorId == request.FloorId ||
                        (r.Block != null && r.Block.Floor != null && r.Block.Floor.Id == request.FloorId));
                }

                if (request.OnlyBlockless is true)
                {
                    baseQuery = baseQuery.Where(r => r.BlockId == null);
                }

                if (request.BuildingId is not null)
                {
                    baseQuery = baseQuery.Where(r =>
                        r.BuildingId == request.BuildingId ||
                        (r.Floor != null && r.Floor.BuildingId == request.BuildingId) ||
                        (r.Block != null && r.Block.Floor != null && r.Block.Floor.BuildingId == request.BuildingId));
                }

                if (request.Status is not null)
                {
                    baseQuery = baseQuery.Where(r => r.Status == request.Status);
                }

                if (request.RoomType is not null)
                {
                    baseQuery = baseQuery.Where(r => r.RoomType == request.RoomType);
                }

                var projectedQuery = baseQuery
                    .ProjectToType<RoomsResponse>()
                    .OrderBy(r => r.Label);

                // --- No pagination: return full list (with max limit) ---
                if (request.Page is null || request.PageSize is null)
                {
                    var list = await projectedQuery
                        .Take(MaxUnpaged)
                        .ToListAsync(cancellationToken);

                    return new PagedResponse<RoomsResponse>
                    {
                        Items = list,
                        PageNumber = 1,
                        PageSize = list.Count,
                        TotalCount = list.Count,
                        TotalPages = 1,
                        HasPreviousPage = false,
                        HasNextPage = false,
                    };
                }

                // --- With pagination ---
                var pagedList = await PagedList<RoomsResponse>.CreateAsync(
                    projectedQuery,
                    request.Page.Value,
                    request.PageSize.Value,
                    cancellationToken);

                return PagedResponse<RoomsResponse>.FromPagedList(pagedList);
            }
        }

    }

    public sealed class GetRoomsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Carter automatically binds query-string parameters to the Query object
            app.MapGet("/rooms", async (
                [AsParameters] GetRooms.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<RoomsResponse>>(200)
            .Produces<Error>(400)
            .WithName("GetRooms")
            .WithTags("Rooms")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of rooms";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}