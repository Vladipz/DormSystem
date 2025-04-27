using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Place;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Places
{
    public static class OccupyPlace
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatePlaceResponse>>
        {
            public Guid Id { get; set; }
            
            public Guid UserId { get; set; }
            
            public DateTime? MovedInAt { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.UserId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdatePlaceResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<UpdatePlaceResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UpdatePlaceResponse>();
                }

                var place = await _dbContext.Places
                    .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

                if (place is null)
                {
                    return Error.NotFound(
                        code: "Place.NotFound",
                        description: $"Place with ID {request.Id} was not found.");
                }

                if (place.OccupiedByUserId is not null)
                {
                    return Error.Conflict(
                        code: "Place.AlreadyOccupied",
                        description: "This place is already occupied by a user.");
                }

                place.OccupiedByUserId = request.UserId;
                place.MovedInAt = request.MovedInAt ?? DateTime.UtcNow;
                place.MovedOutAt = null;

                await _dbContext.SaveChangesAsync(ct);

                return new UpdatePlaceResponse { Id = place.Id };
            }
        }
    }

    public sealed class OccupyPlaceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/places/{id:guid}/occupy", async (Guid id, OccupyPlaceRequest request, ISender sender) =>
            {
                var command = new OccupyPlace.Command 
                { 
                    Id = id,
                    UserId = request.UserId,
                    MovedInAt = DateTime.UtcNow
                };
                
                var result = await sender.Send(command);

                return result.Match(
                    updated => Results.Ok(updated),
                    error => error.ToResponse());
            })
            .Produces<UpdatePlaceResponse>(200)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("Place.Occupy")
            .WithTags("Places")
            .Accepts<OccupyPlaceRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
} 