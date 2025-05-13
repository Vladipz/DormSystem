using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Room;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Rooms
{
    public static class GetRoomById
    {
        internal sealed class Query : IRequest<ErrorOr<RoomDetailsResponse>>
        {
            public Guid RoomId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.RoomId)
                    .NotEmpty()
                    .WithMessage("Room ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<RoomDetailsResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<RoomDetailsResponse>> Handle(Query request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<RoomDetailsResponse>();
                }

                var room = await _dbContext.Rooms
                    .AsNoTracking()
                    .Include(r => r.Block)
                        .ThenInclude(b => b.Floor)
                            .ThenInclude(f => f.Building)
                    .Include(r => r.Floor)
                        .ThenInclude(f => f.Building)
                    .Include(r => r.Building)
                    .Where(r => r.Id == request.RoomId)
                    .FirstOrDefaultAsync(ct);

                if (room is null)
                {
                    return Error.NotFound(
                        code: "Room.NotFound",
                        description: $"Room with ID {request.RoomId} was not found.");
                }

                return room.Adapt<RoomDetailsResponse>();
            }
        }
    }

    public sealed class GetRoomByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/rooms/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetRoomById.Query { RoomId = id };
                var result = await sender.Send(query);

                return result.Match(
                    room => Results.Ok(room),
                    error => error.ToResponse());
            })
            .Produces<RoomDetailsResponse>(200)
            .Produces<Error>(404)
            .WithName("GetRoomById")
            .WithTags("Rooms")
            .WithOpenApi(op =>
            {
                op.Summary = "Get room by ID";
                op.Parameters[0].Description = "Room ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}