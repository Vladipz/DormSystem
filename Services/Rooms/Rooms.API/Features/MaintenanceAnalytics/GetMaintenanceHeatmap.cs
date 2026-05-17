using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.MaintenanceAnalytics;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

namespace Rooms.API.Features.MaintenanceAnalytics
{
    public static class GetMaintenanceHeatmap
    {
        internal sealed class Query : IRequest<ErrorOr<MaintenanceHeatmapResponse>>
        {
            public Guid BuildingId { get; set; }

            public DateTime? DateFrom { get; set; }

            public DateTime? DateTo { get; set; }

            public MaintenanceStatus? Status { get; set; }

            public MaintenancePriority? Priority { get; set; }

            public Guid? AssignedToId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(q => q.BuildingId).NotEmpty();
                RuleFor(q => q.DateTo)
                    .GreaterThanOrEqualTo(q => q.DateFrom)
                    .When(q => q.DateFrom.HasValue && q.DateTo.HasValue);
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<MaintenanceHeatmapResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<MaintenanceHeatmapResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<MaintenanceHeatmapResponse>();
                }

                var building = await _dbContext.Buildings
                    .AsNoTracking()
                    .Include(b => b.Floors)
                    .ThenInclude(f => f.Blocks)
                    .FirstOrDefaultAsync(b => b.Id == request.BuildingId, cancellationToken);

                if (building is null)
                {
                    return Error.NotFound("Building.NotFound", "Building was not found.");
                }

                var tickets = await ApplyFilters(
                        _dbContext.MaintenanceTickets.AsNoTracking(),
                        request)
                    .Where(mt => mt.Room.BlockId.HasValue && mt.Room.Block!.Floor.BuildingId == request.BuildingId)
                    .Select(mt => new TicketStats(mt.Room.BlockId!.Value, mt.Status, mt.Priority))
                    .ToListAsync(cancellationToken);

                var groupedTickets = tickets
                    .GroupBy(t => t.BlockId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var cells = building.Floors
                    .OrderBy(f => f.Number)
                    .SelectMany(floor => floor.Blocks
                        .OrderBy(block => block.Label)
                        .Select(block => CreateCell(floor.Id, floor.Number, block.Id, block.Label, groupedTickets)))
                    .ToList();

                return new MaintenanceHeatmapResponse
                {
                    BuildingId = building.Id,
                    BuildingName = building.Name,
                    DateFrom = request.DateFrom,
                    DateTo = request.DateTo,
                    MaxTicketsCount = cells.Count == 0 ? 0 : cells.Max(c => c.TicketsCount),
                    Cells = cells,
                };
            }

            private static IQueryable<MaintenanceTicket> ApplyFilters(IQueryable<MaintenanceTicket> query, Query request)
            {
                if (request.DateFrom is not null)
                {
                    query = query.Where(mt => mt.CreatedAt >= request.DateFrom.Value);
                }

                if (request.DateTo is not null)
                {
                    query = query.Where(mt => mt.CreatedAt <= request.DateTo.Value);
                }

                if (request.Status is not null)
                {
                    query = query.Where(mt => mt.Status == request.Status);
                }

                if (request.Priority is not null)
                {
                    query = query.Where(mt => mt.Priority == request.Priority);
                }

                if (request.AssignedToId is not null)
                {
                    query = query.Where(mt => mt.AssignedToId == request.AssignedToId);
                }

                return query;
            }

            private static MaintenanceHeatmapCellResponse CreateCell(
                Guid floorId,
                int floorNumber,
                Guid blockId,
                string blockLabel,
                Dictionary<Guid, List<TicketStats>> groupedTickets)
            {
                groupedTickets.TryGetValue(blockId, out var tickets);
                tickets ??= [];

                var ticketsCount = tickets.Count;
                var resolvedCount = tickets.Count(t => t.Status == MaintenanceStatus.Resolved);

                return new MaintenanceHeatmapCellResponse
                {
                    FloorId = floorId,
                    FloorNumber = floorNumber,
                    BlockId = blockId,
                    BlockLabel = blockLabel,
                    TicketsCount = ticketsCount,
                    OpenCount = tickets.Count(t => t.Status == MaintenanceStatus.Open),
                    InProgressCount = tickets.Count(t => t.Status == MaintenanceStatus.InProgress),
                    ResolvedCount = resolvedCount,
                    ResolvedPercentage = ticketsCount == 0 ? 0 : Math.Round((decimal)resolvedCount * 100 / ticketsCount, 1),
                    MostFrequentPriority = tickets
                        .GroupBy(t => t.Priority)
                        .OrderByDescending(g => g.Count())
                        .ThenByDescending(g => g.Key)
                        .Select(g => (MaintenancePriority?)g.Key)
                        .FirstOrDefault(),
                };
            }

            private sealed record TicketStats(Guid BlockId, MaintenanceStatus Status, MaintenancePriority Priority);
        }
    }

    public sealed class GetMaintenanceHeatmapEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/maintenance-analytics/heatmap", async (
                [AsParameters] GetMaintenanceHeatmap.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<MaintenanceHeatmapResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("MaintenanceAnalytics.GetHeatmap")
            .WithTags("Maintenance Analytics")
            .RequireAuthorization("AdminOnly")
            .IncludeInOpenApi();
        }
    }
}
