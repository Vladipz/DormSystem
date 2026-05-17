using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Contracts.MaintenanceAnalytics;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;
using Rooms.API.Services;

namespace Rooms.API.Features.MaintenanceAnalytics
{
    public static class GetMaintenanceDrilldown
    {
        internal sealed class Query : IRequest<ErrorOr<MaintenanceDrilldownResponse>>
        {
            public Guid BuildingId { get; set; }

            public Guid FloorId { get; set; }

            public Guid BlockId { get; set; }

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
                RuleFor(q => q.FloorId).NotEmpty();
                RuleFor(q => q.BlockId).NotEmpty();
                RuleFor(q => q.DateTo)
                    .GreaterThanOrEqualTo(q => q.DateFrom)
                    .When(q => q.DateFrom.HasValue && q.DateTo.HasValue);
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<MaintenanceDrilldownResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;
            private readonly MaintenanceTicketEnricher _ticketEnricher;

            public Handler(
                ApplicationDbContext dbContext,
                IValidator<Query> validator,
                MaintenanceTicketEnricher ticketEnricher)
            {
                _dbContext = dbContext;
                _validator = validator;
                _ticketEnricher = ticketEnricher;
            }

            public async Task<ErrorOr<MaintenanceDrilldownResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<MaintenanceDrilldownResponse>();
                }

                var block = await _dbContext.Blocks
                    .AsNoTracking()
                    .Include(b => b.Floor)
                    .ThenInclude(f => f.Building)
                    .FirstOrDefaultAsync(
                        b => b.Id == request.BlockId &&
                             b.FloorId == request.FloorId &&
                             b.Floor.BuildingId == request.BuildingId,
                        cancellationToken);

                if (block is null)
                {
                    return Error.NotFound("Block.NotFound", "Block was not found for the selected building and floor.");
                }

                var ticketEntities = await ApplyFilters(
                        _dbContext.MaintenanceTickets
                            .AsNoTracking()
                            .Include(mt => mt.Room),
                        request)
                    .Where(mt => mt.Room.BlockId == request.BlockId)
                    .OrderByDescending(mt => mt.CreatedAt)
                    .ToListAsync(cancellationToken);

                var ticketResponses = ticketEntities
                    .Adapt<List<MaintenanceTicketResponse>>();
                var enrichedTickets = await _ticketEnricher.EnrichMaintenanceTicketsAsync(ticketResponses);

                var drilldownTickets = enrichedTickets
                    .Select(ticket => new MaintenanceDrilldownTicketResponse
                    {
                        Id = ticket.Id,
                        RoomId = ticket.Room.Id,
                        RoomLabel = ticket.Room.Label,
                        Title = ticket.Title,
                        Description = ticket.Description,
                        CreatedAt = ticket.CreatedAt,
                        ResolvedAt = ticket.ResolvedAt,
                        AssignedTo = ticket.AssignedTo,
                        Status = ticket.Status,
                        Priority = ticket.Priority,
                        DaysInWork = GetDaysInWork(ticket.CreatedAt, ticket.ResolvedAt),
                    })
                    .ToList();

                var summary = CreateSummary(drilldownTickets);

                return new MaintenanceDrilldownResponse
                {
                    BuildingId = request.BuildingId,
                    BuildingName = block.Floor.Building.Name,
                    FloorId = block.FloorId,
                    FloorNumber = block.Floor.Number,
                    BlockId = block.Id,
                    BlockLabel = block.Label,
                    Summary = summary,
                    Tickets = drilldownTickets,
                    RoomBars = CreateRoomBars(drilldownTickets),
                    Timeline = CreateTimeline(drilldownTickets, request.DateFrom, request.DateTo),
                    DiagnosticMessage = CreateDiagnosticMessage(summary),
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

            private static MaintenanceDrilldownSummaryResponse CreateSummary(List<MaintenanceDrilldownTicketResponse> tickets)
            {
                var ticketsCount = tickets.Count;
                var resolvedCount = tickets.Count(t => t.Status == MaintenanceStatus.Resolved);

                return new MaintenanceDrilldownSummaryResponse
                {
                    TicketsCount = ticketsCount,
                    OpenCount = tickets.Count(t => t.Status == MaintenanceStatus.Open),
                    InProgressCount = tickets.Count(t => t.Status == MaintenanceStatus.InProgress),
                    ResolvedCount = resolvedCount,
                    ResolvedPercentage = ticketsCount == 0 ? 0 : Math.Round((decimal)resolvedCount * 100 / ticketsCount, 1),
                    AverageDaysInWork = ticketsCount == 0 ? 0 : Math.Round((decimal)tickets.Average(t => t.DaysInWork), 1),
                    MostLoadedRoomLabel = tickets
                        .GroupBy(t => t.RoomLabel)
                        .OrderByDescending(g => g.Count())
                        .ThenBy(g => g.Key)
                        .Select(g => g.Key)
                        .FirstOrDefault(),
                    MostFrequentPriority = tickets
                        .GroupBy(t => t.Priority)
                        .OrderByDescending(g => g.Count())
                        .ThenByDescending(g => g.Key)
                        .Select(g => (MaintenancePriority?)g.Key)
                        .FirstOrDefault(),
                };
            }

            private static List<MaintenanceRoomBarResponse> CreateRoomBars(List<MaintenanceDrilldownTicketResponse> tickets)
            {
                return tickets
                    .GroupBy(t => new { t.RoomId, t.RoomLabel })
                    .Select(g => new MaintenanceRoomBarResponse
                    {
                        RoomId = g.Key.RoomId,
                        RoomLabel = g.Key.RoomLabel,
                        TicketsCount = g.Count(),
                    })
                    .OrderByDescending(bar => bar.TicketsCount)
                    .ThenBy(bar => bar.RoomLabel)
                    .ToList();
            }

            private static List<MaintenanceTimelinePointResponse> CreateTimeline(
                List<MaintenanceDrilldownTicketResponse> tickets,
                DateTime? requestedDateFrom,
                DateTime? requestedDateTo)
            {
                if (tickets.Count == 0)
                {
                    return [];
                }

                var dateFrom = (requestedDateFrom ?? tickets.Min(t => t.CreatedAt)).Date;
                var dateTo = (requestedDateTo ?? tickets.Max(t => t.CreatedAt)).Date;
                var useDailyBuckets = (dateTo - dateFrom).TotalDays <= 31;

                return tickets
                    .GroupBy(t => useDailyBuckets ? t.CreatedAt.Date : GetWeekStart(t.CreatedAt.Date))
                    .Select(g => new MaintenanceTimelinePointResponse
                    {
                        PeriodStart = g.Key,
                        Label = useDailyBuckets ? g.Key.ToString("MMM d") : $"Week of {g.Key:MMM d}",
                        TicketsCount = g.Count(),
                    })
                    .OrderBy(point => point.PeriodStart)
                    .ToList();
            }

            private static string CreateDiagnosticMessage(MaintenanceDrilldownSummaryResponse summary)
            {
                if (summary.TicketsCount == 0)
                {
                    return "No maintenance requests match the selected filters for this block.";
                }

                if (summary.OpenCount + summary.InProgressCount > summary.ResolvedCount)
                {
                    return "Active requests exceed resolved requests. This block needs operational attention.";
                }

                if (summary.MostFrequentPriority is MaintenancePriority.Critical or MaintenancePriority.High)
                {
                    return "High-priority requests dominate this block. Check recurring infrastructure issues.";
                }

                if (summary.ResolvedPercentage >= 80)
                {
                    return "Most requests are resolved. Maintenance load for this block is under control.";
                }

                return "Maintenance load is moderate. Monitor this block for recurring requests.";
            }

            private static int GetDaysInWork(DateTime createdAt, DateTime? resolvedAt)
            {
                var end = resolvedAt ?? DateTime.UtcNow;
                return Math.Max(0, (int)Math.Ceiling((end - createdAt).TotalDays));
            }

            private static DateTime GetWeekStart(DateTime date)
            {
                var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                return date.AddDays(-1 * diff).Date;
            }
        }
    }

    public sealed class GetMaintenanceDrilldownEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/maintenance-analytics/drilldown", async (
                [AsParameters] GetMaintenanceDrilldown.Query query,
                ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<MaintenanceDrilldownResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("MaintenanceAnalytics.GetDrilldown")
            .WithTags("Maintenance Analytics")
            .RequireAuthorization("AdminOnly")
            .IncludeInOpenApi();
        }
    }
}
