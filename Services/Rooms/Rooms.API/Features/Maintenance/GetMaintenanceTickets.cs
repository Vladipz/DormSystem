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

using Shared.PagedList;

namespace Rooms.API.Features.Maintenance
{
    public static class GetMaintenanceTickets
    {
        internal sealed class Query : IRequest<ErrorOr<PagedResponse<MaintenanceTicketResponse>>>
        {
            public Guid? RoomId { get; set; }

            public MaintenanceStatus? Status { get; set; }

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

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<PagedResponse<MaintenanceTicketResponse>>> Handle(Query request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PagedResponse<MaintenanceTicketResponse>>();
                }

                IQueryable<MaintenanceTicket> query = _dbContext.MaintenanceTickets.AsNoTracking();

                if (request.RoomId is not null)
                {
                    query = query.Where(x => x.RoomId == request.RoomId);
                }

                if (request.Status is not null)
                {
                    query = query.Where(x => x.Status == request.Status);
                }

                var items = query
                    .OrderByDescending(x => x.CreatedAt)
                    .ProjectToType<MaintenanceTicketResponse>();

                var pagedList = await PagedList<MaintenanceTicketResponse>.CreateAsync(
                    items,
                    request.Page,
                    request.PageSize,
                    ct);

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
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a paged list of maintenance tickets with filters";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}