using Carter;
using Carter.OpenApi;

using ErrorOr;

using Inspections.API.Contracts.Inspections;
using Inspections.API.Data;
using Inspections.API.Entities;
using Inspections.API.Mappings;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.PagedList;

namespace Inspections.API.Features.Inspections
{
    public static class GetInspections
    {
        public sealed class Query : IRequest<ErrorOr<PagedResponse<InspectionListItemResponse>>>
        {
            public string? Status { get; set; }

            public string? Type { get; set; }

            public DateTime? From { get; set; }

            public DateTime? To { get; set; }

            public int PageNumber { get; set; } = 1;

            public int PageSize { get; set; } = 20;
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<InspectionListItemResponse>>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db) => _db = db;

            public async Task<ErrorOr<PagedResponse<InspectionListItemResponse>>> Handle(Query request, CancellationToken ct)
            {
                var query = _db.Inspections
                    .Include(x => x.Rooms)
                    .Select(i => new InspectionListItemResponse
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Type = i.Type,
                        StartDate = i.StartDate,
                        Status = i.Status,
                        RoomsCount = i.Rooms.Count,
                        PendingRoomsCount = i.Rooms.Count(r => r.Status == RoomInspectionStatus.Pending),
                        ConfirmedRoomsCount = i.Rooms.Count(r => r.Status == RoomInspectionStatus.Confirmed),
                        NotConfirmedRoomsCount = i.Rooms.Count(r => r.Status == RoomInspectionStatus.NotConfirmed),
                        NoAccessRoomsCount = i.Rooms.Count(r => r.Status == RoomInspectionStatus.NoAccess),
                    })
                    .AsQueryable();

                // Фільтрація
                if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<InspectionStatus>(request.Status, true, out var status))
                {
                    query = query.Where(x => x.Status == status);
                }

                if (!string.IsNullOrWhiteSpace(request.Type))
                {
                    query = query.Where(x => x.Type == request.Type);
                }

                if (request.From.HasValue)
                {
                    query = query.Where(x => x.StartDate >= request.From.Value);
                }

                if (request.To.HasValue)
                {
                    query = query.Where(x => x.StartDate <= request.To.Value);
                }

                var items = query.OrderBy(x => x.StartDate);

                // Пагінація
                var pagedList = await PagedList<InspectionListItemResponse>.CreateAsync(
                    items,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken: ct);

                return PagedResponse<InspectionListItemResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class ListInspectionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/inspections", async (
                [AsParameters] GetInspections.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    ok => Results.Ok(ok),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<InspectionListItemResponse>>(200)
            .Produces(400)
            .WithTags("Inspections")
            .WithName("ListInspections")
            .IncludeInOpenApi();
        }
    }
}