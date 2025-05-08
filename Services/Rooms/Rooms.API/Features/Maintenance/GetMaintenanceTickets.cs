using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;
using Rooms.API.Services;

using Shared.PagedList;

namespace Rooms.API.Features.Maintenance
{
    public static class GetMaintenanceTickets
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<MaintenanceTicketResponse>>>
        {
            public Guid? RoomId { get; set; }

            public MaintenanceStatus? Status { get; set; }

            public Guid? ReporterById { get; set; }

            public Guid? AssignedToId { get; set; }

            public MaintenancePriority? Priority { get; set; }

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

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PagedResponse<MaintenanceTicketResponse>>>
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

            public async Task<ErrorOr<PagedResponse<MaintenanceTicketResponse>>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<MaintenanceTicketResponse>>();
                }

                IQueryable<MaintenanceTicket> query = _dbContext.MaintenanceTickets
                    .Include(mt => mt.Room) // Include room data
                    .AsNoTracking();

                if (request.RoomId is not null)
                {
                    query = query.Where(x => x.RoomId == request.RoomId);
                }

                if (request.Status is not null)
                {
                    query = query.Where(x => x.Status == request.Status);
                }

                if (request.ReporterById is not null)
                {
                    query = query.Where(x => x.ReporterById == request.ReporterById);
                }

                if (request.AssignedToId is not null)
                {
                    query = query.Where(x => x.AssignedToId == request.AssignedToId);
                }

                if (request.BuildingId is not null)
                {
                    query = query.Where(x => x.Room.Floor.BuildingId == request.BuildingId ||
                                              x.Room.Block.Floor.BuildingId == request.BuildingId);
                }

                if (request.Priority is not null)
                {
                    query = query.Where(x => x.Priority == request.Priority);
                }

                // First map to regular response and create paged list
                var tickets = query
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectToType<MaintenanceTicketResponse>();

                var pagedList = await PagedList<MaintenanceTicketResponse>.CreateAsync(
                    tickets,
                    request.Page,
                    request.PageSize,
                    cancellationToken);

                // Enrich with user information
                var enrichedTickets = await _ticketEnricher.EnrichMaintenanceTicketsAsync(pagedList.Items.ToList());

                pagedList.Items.Clear();
                pagedList.Items.AddRange(enrichedTickets);

                return PagedResponse<MaintenanceTicketResponse>.FromPagedList(pagedList);
            }
        }
    }

    public sealed class GetMaintenanceTicketsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/maintenance-tickets", async ([AsParameters] GetMaintenanceTickets.Query query, ISender sender) =>
            {
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<PagedResponse<MaintenanceTicketResponse>>(200)
            .Produces<Error>(400)
            .WithName("Maintenance.GetMaintenanceTickets")
            .WithTags("Maintenance")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of maintenance tickets with filters";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}