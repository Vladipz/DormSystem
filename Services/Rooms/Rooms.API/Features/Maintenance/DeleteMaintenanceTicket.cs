using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Maintenance
{
    public static class DeleteMaintenanceTicket
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedMaintenanceTicketResponse>>
        {
            public Guid TicketId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TicketId)
                    .NotEmpty()
                    .WithMessage("Ticket ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedMaintenanceTicketResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<DeletedMaintenanceTicketResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<DeletedMaintenanceTicketResponse>();
                }

                var ticket = await _dbContext.MaintenanceTickets
                    .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct);

                if (ticket is null)
                {
                    return Error.NotFound(
                        code: "MaintenanceTicket.NotFound",
                        description: $"Maintenance ticket with ID {request.TicketId} was not found.");
                }

                _dbContext.MaintenanceTickets.Remove(ticket);
                await _dbContext.SaveChangesAsync(ct);

                return new DeletedMaintenanceTicketResponse
                {
                    Id = ticket.Id,
                };
            }
        }
    }

    public sealed class DeleteMaintenanceTicketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/maintenance-tickets/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteMaintenanceTicket.Command { TicketId = id };
                var result = await sender.Send(command);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<DeletedMaintenanceTicketResponse>(200)
            .Produces<Error>(404)
            .WithName("Maintenance.DeleteMaintenanceTicket")
            .WithTags("Maintenance")
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete maintenance ticket by ID";
                op.Parameters[0].Description = "Maintenance ticket ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}