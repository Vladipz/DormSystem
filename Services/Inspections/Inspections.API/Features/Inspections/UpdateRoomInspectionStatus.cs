using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Inspections.API.Contracts.Inspections;
using Inspections.API.Data;
using Inspections.API.Entities;
using Inspections.API.Mappings;

using MassTransit;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Shared.Data;
using Shared.RoomServiceClient;

namespace Inspections.API.Features.Inspections
{
    public static class UpdateRoomInspectionStatus
    {
        public sealed class Command : IRequest<ErrorOr<Unit>>
        {
            public Guid InspectionId { get; set; }

            public Guid RoomInspectionId { get; set; }

            public string Status { get; set; } = string.Empty;

            public string? Comment { get; set; }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.InspectionId).NotEmpty();
                RuleFor(x => x.RoomInspectionId).NotEmpty();
                RuleFor(x => x.Status)
                    .NotEmpty()
                    .Must(s => Enum.TryParse<RoomInspectionStatus>(s, true, out _))
                    .WithMessage("Invalid status value.");
                // For "not_confirmed" and "no_access" — comment required
                When(x => x.Status is "not_confirmed" or "no_access", () =>
                {
                    RuleFor(x => x.Comment)
                        .NotEmpty().WithMessage("Comment is required for this status.");
                });
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Unit>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Command> _validator;
            private readonly IRoomService _roomService;
            private readonly IBus _bus;
            private readonly ILogger<Handler> _logger;

            public Handler(
                ApplicationDbContext db,
                IValidator<Command> validator,
                IRoomService roomService,
                IBus bus,
                ILogger<Handler> logger)
            {
                _db = db;
                _validator = validator;
                _roomService = roomService;
                _bus = bus;
                _logger = logger;
            }

            public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<Unit>();
                }

                var inspection = await _db.Inspections
                    .Include(i => i.Rooms)
                    .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken);

                if (inspection is null)
                {
                    return Error.NotFound("Inspection.NotFound", "Inspection not found");
                }

                var room = inspection.Rooms.FirstOrDefault(r => r.Id == request.RoomInspectionId);
                if (room is null)
                {
                    return Error.NotFound("RoomInspection.NotFound", "Room inspection not found");
                }

                if (!Enum.TryParse<RoomInspectionStatus>(request.Status, true, out var newStatus))
                {
                    return Error.Validation("Status", "Invalid status value.");
                }

                var oldStatus = room.Status;
                room.Status = newStatus;
                room.Comment = request.Comment ?? string.Empty;

                await _db.SaveChangesAsync(cancellationToken);

                // Only publish event if status actually changed
                if (oldStatus != newStatus)
                {
                    try
                    {
                        // Publish the room inspection status updated event
                        await _bus.Publish(
                            new RoomInspectionStatusUpdatedEvent
                            {
                                InspectionId = request.InspectionId,
                                RoomInspectionId = request.RoomInspectionId,
                                RoomId = room.RoomId,
                                InspectionName = inspection.Name,
                                InspectionType = inspection.Type,
                                NewStatus = newStatus.ToString(),
                                Comment = request.Comment,
                                UpdatedAt = DateTime.UtcNow,
                                RoomNumber = room.RoomNumber,
                                Building = room.Building,
                                Floor = room.Floor,
                            }, cancellationToken);

                        _logger.LogInformation(
                            "Published RoomInspectionStatusUpdatedEvent for room inspection {RoomInspectionId}, room {RoomId}, status changed from {OldStatus} to {NewStatus}",
                            request.RoomInspectionId,
                            room.RoomId,
                            oldStatus,
                            newStatus);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to publish RoomInspectionStatusUpdatedEvent for room inspection {RoomInspectionId}",
                            request.RoomInspectionId);

                        // Don't fail the whole operation if event publishing fails
                    }
                }

                return Unit.Value;
            }
        }
    }

    public sealed class UpdateRoomInspectionStatusEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/api/inspections/{inspectionId:guid}/rooms/{roomInspectionId:guid}", async (
                Guid inspectionId,
                Guid roomInspectionId,
                UpdateRoomInspectionStatusRequest request,
                ISender sender) =>
            {
                var command = new UpdateRoomInspectionStatus.Command
                {
                    InspectionId = inspectionId,
                    RoomInspectionId = roomInspectionId,
                    Status = request.Status,
                    Comment = request.Comment,
                };

                var result = await sender.Send(command);

                return result.Match(
                    _ => Results.NoContent(),
                    error => error.ToResponse());
            })
            .Accepts<UpdateRoomInspectionStatusRequest>("application/json")
            .Produces(204)
            .Produces(400)
            .Produces(404)
            .WithName("UpdateRoomInspectionStatus")
            .WithTags("Inspections")
            .IncludeInOpenApi();
        }
    }
}