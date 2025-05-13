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

            public RoomStatus? Status { get; set; }

            public RoomType? RoomType { get; set; }

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

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<RoomsResponse>>>
        {
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
                    .Include(r => r.Block)
                        .ThenInclude(b => b.Floor)
                    .AsNoTracking();

                if (request.BlockId is not null)
                {
                    baseQuery = baseQuery.Where(r => r.BlockId == request.BlockId);
                }

                if (request.BuildingId is not null)
                {
                    baseQuery = baseQuery.Where(r =>
                        r.BuildingId == request.BuildingId ||
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

                var items = baseQuery
                    .ProjectToType<RoomsResponse>()
                    .OrderBy(r => r.Label);

                var pagedList = await PagedList<RoomsResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
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