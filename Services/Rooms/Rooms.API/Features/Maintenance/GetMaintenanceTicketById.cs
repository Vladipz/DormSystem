using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Maintenance
{
    public static class GetMaintenanceTicketById
    {
        internal sealed class Query : IRequest<ErrorOr<MaintenanceTicketDetailsResponse>>
        {
            public Guid TicketId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.TicketId)
                    .NotEmpty()
                    .WithMessage("Ticket ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<MaintenanceTicketDetailsResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<MaintenanceTicketDetailsResponse>> Handle(Query request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<MaintenanceTicketDetailsResponse>();
                }

                var ticket = await _dbContext.MaintenanceTickets
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct);

                if (ticket is null)
                {
                    return Error.NotFound(
                        code: "MaintenanceTicket.NotFound",
                        description: $"Maintenance ticket with ID {request.TicketId} was not found.");
                }

                return ticket.Adapt<MaintenanceTicketDetailsResponse>();
            }
        }
    }

    public sealed class GetMaintenanceTicketByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/maintenance-tickets/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetMaintenanceTicketById.Query { TicketId = id };
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<MaintenanceTicketDetailsResponse>(200)
            .Produces<Error>(404)
            .WithName("Maintenance.GetMaintenanceTicketById")
            .WithTags("Maintenance")
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(op =>
            {
                op.Summary = "Get a maintenance ticket by ID";
                op.Parameters[0].Description = "Maintenance Ticket ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}