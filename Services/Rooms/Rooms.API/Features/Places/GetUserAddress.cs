using Carter;

using FluentValidation;

using MapsterMapper;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Place;
using Rooms.API.Data;
using Rooms.API.Entities;

namespace Rooms.API.Features.Places
{
    public static class GetUserAddress
    {
        public sealed record Query(Guid UserId) : IRequest<UserAddressResponse?>;

        public sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.UserId)
                    .NotEmpty()
                    .WithMessage("User ID is required.");
            }
        }

        public sealed class Handler : IRequestHandler<Query, UserAddressResponse?>
        {
            private readonly ApplicationDbContext _context;

            public Handler(ApplicationDbContext context)
            {
                _context = context;
            }

            public async Task<UserAddressResponse?> Handle(Query request, CancellationToken cancellationToken)
            {
                var place = await _context.Places
                    .Include(p => p.Room)
                        .ThenInclude(r => r!.Floor)
                            .ThenInclude(f => f!.Building)
                    .Include(p => p.Room)
                        .ThenInclude(r => r!.Block)
                            .ThenInclude(b => b!.Floor)
                                .ThenInclude(f => f!.Building)
                    .Include(p => p.Room)
                        .ThenInclude(r => r!.Building)
                    .Where(p => p.OccupiedByUserId == request.UserId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (place == null)
                {
                    return null;
                }

                var room = place.Room;
                if (room == null)
                {
                    return null;
                }

                // Determine the floor and building based on room type
                Floor? floor = null;
                Building? building = null;

                if (room.Block != null)
                {
                    // Room is in a block - get floor and building from block
                    floor = room.Block.Floor;
                    building = room.Block.Floor?.Building;
                }
                else if (room.Floor != null)
                {
                    // Room is directly on a floor
                    floor = room.Floor;
                    building = room.Floor.Building;
                }
                else if (room.Building != null)
                {
                    // Room is directly in a building (no floor)
                    building = room.Building;
                }

                return new UserAddressResponse
                {
                    PlaceId = place.Id,
                    RoomId = place.RoomId,
                    RoomLabel = room.Label,
                    FloorId = floor?.Id,
                    FloorLabel = floor?.Number.ToString(),
                    BuildingId = building?.Id,
                    BuildingName = building?.Name,
                    BuildingAddress = building?.Address,
                    IsOccupied = true,
                    MovedInAt = place.MovedInAt,
                };
            }
        }
    }

    public class GetUserAddressEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/places/user/{userId:guid}/address", async (Guid userId, ISender sender) =>
            {
                var result = await sender.Send(new GetUserAddress.Query(userId));

                if (result == null)
                {
                    return Results.NotFound($"No address found for user with ID {userId}");
                }

                return Results.Ok(result);
            })
            .WithName("GetUserAddress")
            .WithSummary("Get user address by user ID")
            .WithDescription("Returns the complete address information (building, floor, room) for a user based on their current place assignment")
            .WithTags("Places")
            .Produces<UserAddressResponse>()
            .Produces(404);
        }
    }
}