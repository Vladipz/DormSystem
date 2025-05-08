using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Place;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

using Shared.PagedList;

namespace Rooms.API.Features.Places
{
    public static class GetPlaces
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<PlacesResponse>>>
        {
            public Guid? RoomId { get; set; }

            public bool? IsOccupied { get; set; }

            public int Page { get; set; } = 1;

            public int PageSize { get; set; } = 20;
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                // Page and PageSize already have default values in the Query class
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<PlacesResponse>>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<PlacesResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<PlacesResponse>>();
                }

                IQueryable<Place> baseQuery = _db.Places
                    .AsNoTracking()
                    .Include(p => p.Room);

                if (request.RoomId is not null)
                {
                    baseQuery = baseQuery.Where(p => p.RoomId == request.RoomId);
                }

                if (request.IsOccupied is not null)
                {
                    baseQuery = request.IsOccupied.Value
                        ? baseQuery.Where(p => p.OccupiedByUserId != null)
                        : baseQuery.Where(p => p.OccupiedByUserId == null);
                }

                var items = baseQuery
                    .Select(p => new PlacesResponse
                    {
                        Id = p.Id,
                        RoomId = p.RoomId,
                        Index = p.Index,
                        IsOccupied = p.OccupiedByUserId != null,
                        MovedInAt = p.MovedInAt,
                        RoomLabel = p.Room.Label,
                        OccupiedByUserId = p.OccupiedByUserId,
                    })
                    .OrderBy(p => p.RoomLabel)
                    .ThenBy(p => p.Index);

                var pagedList = await PagedList<PlacesResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                return PagedResponse<PlacesResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class GetPlacesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            // Carter automatically binds query-string parameters to the Query object
            app.MapGet("/places", async (
                ISender sender,
                int pageNumber = 1,
                int pageSize = 20,
                Guid? roomId = null,
                bool? isOccupied = null) =>
            {
                var query = new GetPlaces.Query
                {
                    Page = pageNumber,
                    PageSize = pageSize,
                    RoomId = roomId,
                    IsOccupied = isOccupied,
                };
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<PlacesResponse>>(200)
            .Produces(400)
            .WithName("Place.GetList")
            .WithTags("Places")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of places";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}