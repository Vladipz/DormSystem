using System.Collections.ObjectModel;

using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Rooms.API.Contracts.Room;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

using static Rooms.API.Features.Rooms.CreateRoom;

namespace Rooms.API.Features.Rooms
{
    public static class CreateRoom
    {
        internal sealed class Command : IRequest<ErrorOr<CreateRoomResponse>>
        {
            public Guid? BlockId { get; set; }

            public string Label { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public RoomStatus Status { get; set; }

            public RoomType RoomType { get; set; }

            public string? Purpose { get; set; }

            public List<string> Amenities { get; set; } = new List<string>();
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
                RuleFor(x => x.Capacity).GreaterThanOrEqualTo(0);
                RuleFor(x => x.Purpose)
                    .NotEmpty()
                    .When(x => x.RoomType != RoomType.Regular)
                    .WithMessage("Purpose is required for non-regular rooms.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreateRoomResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly ILogger<Handler> _logger;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, ILogger<Handler> logger, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _logger = logger;
                _validator = validator;
            }

            public async Task<ErrorOr<CreateRoomResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validationResult = await _validator.ValidateAsync(request, cancellationToken);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning(
                        "Room creation validation failed: {Errors}",
                        string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

                    return validationResult.ToValidationError<CreateRoomResponse>();
                }

                var room = new Room
                {
                    Id = Guid.NewGuid(),
                    BlockId = request.BlockId,
                    Label = request.Label,
                    Capacity = request.Capacity,
                    Status = request.Status,
                    RoomType = request.RoomType,
                    Purpose = request.Purpose,
                    Amenities = new Collection<string>(request.Amenities),
                };

                _dbContext.Rooms.Add(room);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Room created successfully with ID: {RoomId}", room.Id);

                return new CreateRoomResponse { Id = room.Id };
            }
        }
    }

    public sealed class CreateRoomEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/rooms", async (
                CreateRoomRequest request,
                IMediator mediator) =>
            {
                var command = request.Adapt<Command>();
                var result = await mediator.Send(command);

                return result.Match(
                    response => Results.Created($"/rooms/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreateRoomResponse>(201)
            .Produces<Error>(400)
            .WithName("CreateRoom")
            .WithTags("Rooms")
            .Accepts<CreateRoomRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}