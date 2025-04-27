
using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Maintenance;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Maintenance
{
    public static class ChangeMaintenanceTicketStatus
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatedMaintenanceTicketStatusResponse>>
        {
            public Guid TicketId { get; set; }

            public MaintenanceStatus NewStatus { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TicketId)
                    .NotEmpty()
                    .WithMessage("Ticket ID must not be empty.");

                RuleFor(x => x.NewStatus)
                    .IsInEnum()
                    .WithMessage("Invalid maintenance status provided.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdatedMaintenanceTicketStatusResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<UpdatedMaintenanceTicketStatusResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UpdatedMaintenanceTicketStatusResponse>();
                }

                var ticket = await _dbContext.MaintenanceTickets
                    .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct);

                if (ticket is null)
                {
                    return Error.NotFound(
                        code: "MaintenanceTicket.NotFound",
                        description: $"Maintenance ticket with ID {request.TicketId} was not found.");
                }

                ticket.Status = request.NewStatus;

                if (request.NewStatus == MaintenanceStatus.Resolved)
                {
                    ticket.ResolvedAt = DateTime.UtcNow;
                }
                else
                {
                    ticket.ResolvedAt = null;
                }

                await _dbContext.SaveChangesAsync(ct);

                return new UpdatedMaintenanceTicketStatusResponse
                {
                    Id = ticket.Id,
                    NewStatus = ticket.Status,
                };
            }
        }
    }

    public sealed class ChangeMaintenanceTicketStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/maintenance-tickets/{id:guid}/status", async (Guid id, ChangeMaintenanceTicketStatusRequest request, ISender sender) =>
            {
                if (id != request.TicketId)
                {
                    return Results.BadRequest(Error.Validation(
                        code: "MaintenanceTicket.IdMismatch",
                        description: "URL ID and body TicketId do not match."));
                }

                var command = new ChangeMaintenanceTicketStatus.Command
                {
                    TicketId = request.TicketId,
                    NewStatus = request.NewStatus,
                };

                var result = await sender.Send(command);

                return result.Match(
                    updated => Results.Ok(updated),
                    error => error.ToResponse());
            })
            .Produces<UpdatedMaintenanceTicketStatusResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("Maintenance.ChangeMaintenanceTicketStatus")
            .WithTags("Maintenance")
            .RequireAuthorization("AdminOnly")
            .Accepts<ChangeMaintenanceTicketStatusRequest>("application/json")
            .WithOpenApi(op =>
            {
                op.Summary = "Change maintenance ticket status";
                op.Parameters[0].Description = "Maintenance ticket ID";
                return op;
            });
        }
    }
}