using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Room;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Rooms
{
    public static class DeleteRoom
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedRoomResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Room ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedRoomResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<DeletedRoomResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<DeletedRoomResponse>();
                }

                var room = await _dbContext.Rooms
                    .FirstOrDefaultAsync(r => r.Id == request.Id, ct);

                if (room is null)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.Id} was not found.");
                }

                _dbContext.Rooms.Remove(room);
                await _dbContext.SaveChangesAsync(ct);

                return new DeletedRoomResponse { Id = room.Id };
            }
        }
    }

    public sealed class DeleteRoomEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/rooms/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteRoom.Command { Id = id };
                var result = await sender.Send(command);

                return result.Match(
                    deleted => Results.Ok(deleted),
                    error => error.ToResponse());
            })
            .Produces<DeletedRoomResponse>(200)
            .Produces<Error>(404)
            .WithName("DeleteRoom")
            .WithTags("Rooms")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete room by ID";
                op.Parameters[0].Description = "Room ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}