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

namespace Rooms.API.Features.Maintenance
{
    public static class CreateMaintenanceTicket
    {
        internal sealed class Command : IRequest<ErrorOr<CreateMaintenanceTicketResponse>>
        {
            public Guid RoomId { get; set; }

            public string Title { get; set; } = string.Empty;

            public string Description { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RoomId)
                    .NotEmpty()
                    .WithMessage("RoomId must not be empty.");

                RuleFor(x => x.Title)
                    .NotEmpty()
                    .MaximumLength(100);

                RuleFor(x => x.Description)
                    .MaximumLength(1000);
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreateMaintenanceTicketResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<CreateMaintenanceTicketResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<CreateMaintenanceTicketResponse>();
                }

                // Validate that Room exists
                var roomExists = await _dbContext.Rooms
                    .AnyAsync(r => r.Id == request.RoomId, cancellationToken);

                if (!roomExists)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.RoomId} does not exist.");
                }

                var ticket = new MaintenanceTicket
                {
                    Id = Guid.NewGuid(),
                    RoomId = request.RoomId,
                    Title = request.Title,
                    Description = request.Description,
                    IsResolved = false,
                    CreatedAt = DateTime.UtcNow,
                    Status = MaintenanceStatus.Open,
                };

                _dbContext.MaintenanceTickets.Add(ticket);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreateMaintenanceTicketResponse { Id = ticket.Id };
            }
        }
    }

    public sealed class CreateMaintenanceTicketEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/maintenance-tickets", async (CreateMaintenanceTicketRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateMaintenanceTicket.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    response => Results.Created($"/maintenance-tickets/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreateMaintenanceTicketResponse>(201)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("Maintenance.CreateMaintenanceTicket")
            .WithTags("Maintenance")
            .Accepts<CreateMaintenanceTicketRequest>("application/json")
            .RequireAuthorization() // User or Admin
            .IncludeInOpenApi();
        }
    }
}