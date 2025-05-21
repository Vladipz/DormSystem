using System.Globalization;

using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Inspections.API.Contracts.Inspections;
using Inspections.API.Data;
using Inspections.API.Entities;
using Inspections.API.Mappings;

using Mapster;

using MediatR;

using Shared.RoomServiceClient;

using static Inspections.API.Features.Inspections.CreateInspection;

namespace Inspections.API.Features.Inspections
{
    public static class CreateInspection
    {
        // 1. --- Command DTO ---
        internal sealed class Command : IRequest<ErrorOr<Guid>>
        {
            public string Name { get; set; } = string.Empty;

            public DateTime StartDate { get; set; }

            public string Type { get; set; } = string.Empty;

            public string Mode { get; set; } = "manual";

            public Guid? DormitoryId { get; set; }

            public bool IncludeSpecialRooms { get; set; }

            public List<RoomInfo> Rooms { get; set; } = new();
        }

        public sealed class RoomInfo
        {
            public Guid RoomId { get; set; }

            public string? RoomNumber { get; set; }

            public string? Floor { get; set; }

            public string? Building { get; set; }
        }

        // 2. --- Validator ---
        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
                RuleFor(x => x.Type).NotEmpty().MaximumLength(64);
                RuleFor(x => x.StartDate)
                    .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
                    .WithMessage("Start date must be in the future.");

                RuleFor(x => x.Mode)
                    .Must(m => m == "manual" || m == "automatic")
                    .WithMessage("Mode must be either 'manual' or 'automatic'.");

                RuleFor(x => x)
                    .Must(x =>
                        (x.Mode == "manual" && x.Rooms.Count > 0) ||
                        (x.Mode == "automatic" && x.DormitoryId.HasValue))
                    .WithMessage("Either provide rooms manually or specify a dormitory.");

                When(x => x.Mode == "manual", () =>
                {
                    RuleForEach(x => x.Rooms).SetValidator(new RoomInfoValidator());
                });
            }
        }

        private sealed class RoomInfoValidator : AbstractValidator<RoomInfo>
        {
            public RoomInfoValidator()
            {
                RuleFor(r => r.RoomId).NotEmpty();
                RuleFor(r => r.RoomNumber).MaximumLength(32);
                RuleFor(r => r.Floor).MaximumLength(16);
                RuleFor(r => r.Building).MaximumLength(32);
            }
        }

        // 3. --- Handler ---
        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Guid>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Command> _validator;
            private readonly ILogger<Handler> _logger;
            private readonly IRoomService _roomService;

            public Handler(ApplicationDbContext db, IValidator<Command> validator, ILogger<Handler> logger, IRoomService roomService)
            {
                _db = db;
                _validator = validator;
                _logger = logger;
                _roomService = roomService;
            }

            public async Task<ErrorOr<Guid>> Handle(Command request, CancellationToken ct)
            {
                // Validation
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    _logger.LogWarning("CreateInspection validation failed: {Errors}", string.Join("; ", validation.Errors.Select(x => x.ErrorMessage)));
                    return validation.ToValidationError<Guid>();
                }

                List<RoomInfo> selectedRooms;

                if (request.Mode == "manual")
                {
                    selectedRooms = request.Rooms;
                }
                else
                {
                    var roomsResult = await _roomService.GetRoomsForInspectionAsync(request.DormitoryId!.Value, request.IncludeSpecialRooms, ct);

                    if (roomsResult.IsError)
                    {
                        _logger.LogError("Error getting rooms: {Error}", roomsResult.FirstError.Description);
                        return roomsResult.FirstError;
                    }

                    var rooms = roomsResult.Value;

                    if (!request.IncludeSpecialRooms)
                    {
                        rooms = rooms.Where(r => !r.IsSpecial).ToList();
                    }

                    selectedRooms = rooms.Select(r => new RoomInfo
                    {
                        RoomId = r.Id,
                        RoomNumber = r.RoomNumber,
                        Floor = r.Floor.ToString(CultureInfo.InvariantCulture),
                        Building = r.Building,
                    }).ToList();
                }

                var inspection = new Inspection
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Type = request.Type,
                    StartDate = request.StartDate,
                    Status = InspectionStatus.Scheduled,
                    Rooms = selectedRooms.Select(r => new RoomInspection
                    {
                        Id = Guid.NewGuid(),
                        RoomId = r.RoomId,
                        RoomNumber = r.RoomNumber,
                        Floor = r.Floor,
                        Building = r.Building,
                        Status = RoomInspectionStatus.Pending,
                        InspectionId = Guid.Empty,
                    }).ToList(),
                };

                _db.Inspections.Add(inspection);
                await _db.SaveChangesAsync(ct);

                _logger.LogInformation("Created inspection {InspectionId}", inspection.Id);

                return inspection.Id;
            }
        }
    }

    // 4. --- Endpoint ---
    public sealed class CreateInspectionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/inspections", async (
                CreateInspectionRequest request,
                IMediator mediator) =>
            {
                // Map CreateInspectionRequest → CreateInspection.Command
                var command = request.Adapt<Command>(); // через Mapster

                var result = await mediator.Send(command);

                return result.Match(
                    id => Results.Created($"/api/inspections/{id}", new { id }),
                    errors => errors.ToResponse());
            })
            .Produces<Guid>(201)
            .Produces<Error>(400)
            .WithTags("Inspections")
            .WithName("CreateInspection")
            .Accepts<CreateInspectionRequest>("application/json")
            .IncludeInOpenApi();
        }
    }
}