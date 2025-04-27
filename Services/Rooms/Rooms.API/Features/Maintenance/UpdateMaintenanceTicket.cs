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
    public static class UpdateMaintenanceTicket
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatedMaintenanceTicketResponse>>
        {
            public Guid TicketId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.TicketId)
                    .NotEmpty()
                    .WithMessage("Ticket ID must not be empty.");

                RuleFor(x => x.Title)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Description)
                    .MaximumLength(1000);
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdatedMaintenanceTicketResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<UpdatedMaintenanceTicketResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UpdatedMaintenanceTicketResponse>();
                }

                var ticket = await _dbContext.MaintenanceTickets
                    .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct);

                if (ticket is null)
                {
                    return Error.NotFound(
                        code: "MaintenanceTicket.NotFound",
                        description: $"Maintenance ticket with ID {request.TicketId} was not found.");
                }

                ticket.Title = request.Title;
                ticket.Description = request.Description;

                await _dbContext.SaveChangesAsync(ct);

                return new UpdatedMaintenanceTicketResponse
                {
                    Id = ticket.Id,
                };
            }
        }
    }

    public sealed class UpdateMaintenanceTicketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/maintenance-tickets/{id:guid}", async (Guid id, UpdateMaintenanceTicketRequest request, ISender sender) =>
            {
                if (id != request.TicketId)
                {
                    return Results.BadRequest(Error.Validation(
                        code: "MaintenanceTicket.IdMismatch",
                        description: "URL ID and request TicketId do not match."));
                }

                var command = request.Adapt<UpdateMaintenanceTicket.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<UpdatedMaintenanceTicketResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("Maintenance.UpdateMaintenanceTicket")
            .WithTags("Maintenance")
            .RequireAuthorization("AdminOnly")
            .Accepts<UpdateMaintenanceTicketRequest>("application/json")
            .WithOpenApi(op =>
            {
                op.Summary = "Update maintenance ticket (title and description)";
                op.Parameters[0].Description = "Maintenance ticket ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}