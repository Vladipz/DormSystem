using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Rooms.API.Data;
using Rooms.API.Mappings;

using Shared.FileServiceClient;

namespace Rooms.API.Features.Rooms
{
    public static class DeleteRoomPhoto
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedRoomPhotoResponse>>
        {
            public Guid RoomId { get; set; }

            public string PhotoId { get; set; } = string.Empty;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RoomId).NotEmpty();
                RuleFor(x => x.PhotoId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedRoomPhotoResponse>>
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

            public async Task<ErrorOr<DeletedRoomPhotoResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<DeletedRoomPhotoResponse>();
                }

                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == request.RoomId, ct);

                if (room is null)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.RoomId} was not found.");
                }

                if (!room.PhotoIds.Contains(request.PhotoId))
                {
                    return Error.NotFound(
                        code: "RoomPhoto.NotFound",
                        description: $"Photo with ID {request.PhotoId} is not associated with this room.");
                }

                try
                {
                    // Remove photo ID from room
                    room.PhotoIds.Remove(request.PhotoId);
                    await _dbContext.SaveChangesAsync(ct);

                    // Delete photo from FileStorage service
                    await _fileServiceClient.DeleteFileAsync(request.PhotoId);

                    return new DeletedRoomPhotoResponse
                    {
                        PhotoId = request.PhotoId,
                        Message = "Room photo deleted successfully"
                    };
                }
                catch (Exception ex)
                {
                    return Error.Failure(
                        code: "RoomPhoto.DeleteFailed",
                        description: $"Failed to delete room photo: {ex.Message}");
                }
            }
        }
    }

    public sealed class DeletedRoomPhotoResponse
    {
        public string PhotoId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class DeleteRoomPhotoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/rooms/{roomId:guid}/photos/{photoId}", async (Guid roomId, string photoId, ISender sender) =>
            {
                var command = new DeleteRoomPhoto.Command
                {
                    RoomId = roomId,
                    PhotoId = photoId
                };

                var result = await sender.Send(command);

                return result.Match(
                    response => Results.Ok(response),
                    error => error.ToResponse());
            })
            .Produces<DeletedRoomPhotoResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(404)
            .WithName("DeleteRoomPhoto")
            .WithTags("Rooms")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly")
            .DisableAntiforgery();
        }
    }
}