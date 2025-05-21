using Carter;
using Carter.OpenApi;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Data;
using Rooms.API.Entities;

using Shared.Data.Dtos;

namespace Rooms.API.Features.Rooms
{
    public static class GetRoomsForInspection
    {
        public sealed record Query(Guid DormitoryId, bool IncludeSpecial = true) : IRequest<List<RoomDto>>;

        internal sealed class Handler : IRequestHandler<Query, List<RoomDto>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<List<RoomDto>> Handle(Query request, CancellationToken ct)
            {
                var baseQuery = _db.Rooms
                    .Include(r => r.Block).ThenInclude(b => b.Floor).ThenInclude(f => f.Building)
                    .Include(r => r.Floor).ThenInclude(f => f.Building)
                    .AsNoTracking()
                    .Where(r =>
                        (r.Block != null && r.Block.Floor.Building.Id == request.DormitoryId) ||
                        (r.Floor != null && r.Floor.Building.Id == request.DormitoryId));

                if (!request.IncludeSpecial)
                {
                    baseQuery = baseQuery.Where(r => r.RoomType != RoomType.Specialized);
                }

                return await baseQuery.Select(r => new RoomDto(
                    r.Id,
                    r.Label,
                    r.Block != null ? r.Block.Floor.Number : r.Floor!.Number,
                    r.Block != null ? r.Block.Floor.Building.Name : r.Floor!.Building.Name,
                    r.RoomType == RoomType.Specialized)).ToListAsync(ct);
            }
        }
    }

    public sealed class GetRoomsForInspectionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/rooms/for-inspection", async (
                Guid dormitoryId,
                bool? includeSpecial,
                ISender sender) =>
            {
                var query = new GetRoomsForInspection.Query(dormitoryId, includeSpecial ?? true);
                var result = await sender.Send(query);

                return Results.Ok(result);
            })
            .Produces<List<RoomDto>>(200)
            .WithName("GetRoomsForInspection")
            .WithTags("Rooms - Inspection")
            .WithOpenApi(op =>
            {
                op.Summary = "Get all rooms for inspection in dormitory";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}
