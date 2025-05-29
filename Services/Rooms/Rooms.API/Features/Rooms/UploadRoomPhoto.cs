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
    public static class UploadRoomPhoto
    {
        internal sealed class Command : IRequest<ErrorOr<UploadRoomPhotoResponse>>
        {
            public Guid RoomId { get; set; }
            public IFormFile Photo { get; set; } = null!;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.RoomId).NotEmpty();
                RuleFor(x => x.Photo)
                    .NotNull()
                    .WithMessage("Photo file is required")
                    .Must(BeValidImageFile)
                    .WithMessage("File must be a valid image (JPEG, PNG, GIF, WebP)")
                    .Must(BeValidFileSize)
                    .WithMessage("File size must be less than 10MB");
            }

            private static bool BeValidImageFile(IFormFile? file)
            {
                if (file == null) return false;

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
                return allowedTypes.Contains(file.ContentType.ToLower());
            }

            private static bool BeValidFileSize(IFormFile? file)
            {
                if (file == null) return false;

                const long maxSize = 10 * 1024 * 1024; // 10MB
                return file.Length <= maxSize;
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UploadRoomPhotoResponse>>
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

            public async Task<ErrorOr<UploadRoomPhotoResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UploadRoomPhotoResponse>();
                }

                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == request.RoomId, ct);

                if (room is null)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.RoomId} was not found.");
                }

                try
                {
                    // Upload photo to FileStorage service using stream
                    using var stream = request.Photo.OpenReadStream();
                    var uploadResult = await _fileServiceClient.UploadFileAsync(
                        stream,
                        request.Photo.FileName,
                        request.Photo.ContentType,
                        "gallery");

                    // Add photo ID to room
                    room.PhotoIds.Add(uploadResult.Id);
                    await _dbContext.SaveChangesAsync(ct);

                    var photoUrl = _fileServiceClient.GetFileUrl(uploadResult.Id);

                    return new UploadRoomPhotoResponse
                    {
                        PhotoId = uploadResult.Id,
                        PhotoUrl = photoUrl,
                        Message = "Room photo uploaded successfully"
                    };
                }
                catch (Exception ex)
                {
                    return Error.Failure(
                        code: "RoomPhoto.UploadFailed",
                        description: $"Failed to upload room photo: {ex.Message}");
                }
            }
        }
    }

    public sealed class UploadRoomPhotoResponse
    {
        public string PhotoId { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public sealed class UploadRoomPhotoEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/rooms/{roomId:guid}/photos", async (Guid roomId, IFormFile photo, ISender sender) =>
            {
                var command = new UploadRoomPhoto.Command
                {
                    RoomId = roomId,
                    Photo = photo
                };

                var result = await sender.Send(command);

                return result.Match(
                    response => Results.Ok(response),
                    error => error.ToResponse());
            })
            .Produces<UploadRoomPhotoResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(404)
            .WithName("UploadRoomPhoto")
            .WithTags("Rooms")
            .Accepts<IFormFile>("multipart/form-data")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly")
            .DisableAntiforgery();
        }
    }
}