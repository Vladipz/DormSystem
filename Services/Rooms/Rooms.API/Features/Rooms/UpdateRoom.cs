using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Room;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;
using Shared.FileServiceClient;

namespace Rooms.API.Features.Rooms
{
    public static class UpdateRoom
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatedRoomResponse>>
        {
            public Guid Id { get; set; }

            public Guid? BlockId { get; set; }

            public string Label { get; set; } = string.Empty;

            public int Capacity { get; set; }

            public RoomStatus Status { get; set; }

            public RoomType RoomType { get; set; }

            public string? Purpose { get; set; }

            public List<string> Amenities { get; set; } =
                [];

            public List<string> PhotoIds { get; set; } = [];
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Label).NotEmpty().MaximumLength(50);
                RuleFor(x => x.Capacity).GreaterThan(0);
                RuleFor(x => x.Purpose)
                    .NotEmpty()
                    .When(x => x.RoomType != RoomType.Regular)
                    .WithMessage("Purpose is required for non-regular rooms.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdatedRoomResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;
            private readonly IFileServiceClient _fileServiceClient;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator, IFileServiceClient fileServiceClient)
            {
                _dbContext = dbContext;
                _validator = validator;
                _fileServiceClient = fileServiceClient;
            }

            public async Task<ErrorOr<UpdatedRoomResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UpdatedRoomResponse>();
                }

                var room = await _dbContext.Rooms
                    .Include(r => r.Places)
                    .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

                if (room is null)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.Id} was not found.");
                }

                // Store old photo IDs for cleanup
                var oldPhotoIds = room.PhotoIds.ToList();

                room.BlockId = request.BlockId;
                room.Label = request.Label;
                room.Capacity = request.Capacity;
                room.Status = request.Status;
                room.RoomType = request.RoomType;
                room.Purpose = request.Purpose;
                room.Amenities.Clear();
                foreach (var amenity in request.Amenities)
                {
                    room.Amenities.Add(amenity);
                }

                // Update photo IDs
                room.PhotoIds.Clear();
                foreach (var photoId in request.PhotoIds)
                {
                    room.PhotoIds.Add(photoId);
                }

                await _dbContext.SaveChangesAsync(ct);

                // Clean up old photos that are no longer associated with the room
                var photosToDelete = oldPhotoIds.Except(request.PhotoIds).ToList();
                foreach (var photoId in photosToDelete)
                {
                    try
                    {
                        await _fileServiceClient.DeleteFileAsync(photoId);
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't fail the update operation
                        Console.WriteLine($"Warning: Failed to delete photo {photoId}: {ex.Message}");
                    }
                }

                return new UpdatedRoomResponse { Id = room.Id };
            }
        }
    }

    public sealed class UpdateRoomEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/rooms/{id:guid}", static async (Guid id, UpdateRoomRequest request, ISender sender) =>
            {
                if (id != request.Id)
                {
                    return Results.BadRequest(Error.Validation(
                        code: "Room.IdMismatch",
                        description: "URL ID and request body ID do not match."));
                }

                var command = request.Adapt<UpdateRoom.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    updated => Results.Ok(updated),
                    error => error.ToResponse());
            })
            .Produces<UpdatedRoomResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("UpdateRoom")
            .WithTags("Rooms")
            .Accepts<UpdateRoomRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}