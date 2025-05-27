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
    public static class VacatePlace
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatePlaceResponse>>
        {
            public Guid Id { get; set; }

            public DateTime? MovedOutAt { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.MovedOutAt)
                    .NotEmpty()
                    .WithMessage("Move-out date is required when vacating a place.");
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

                if (place.OccupiedByUserId is null)
                {
                    return Error.Conflict(
                        code: "Place.NotOccupied",
                        description: "Cannot vacate a place that is not occupied.");
                }

                if (place.MovedInAt > request.MovedOutAt)
                {
                    return Error.Validation(
                        code: "Place.InvalidMoveOutDate",
                        description: "Move-out date must be after move-in date.");
                }

                place.MovedOutAt = request.MovedOutAt;
                place.OccupiedByUserId = null; // Clear the user ID when vacating
                await _dbContext.SaveChangesAsync(ct);

                return new UpdatePlaceResponse { Id = place.Id };
            }
        }
    }

    public sealed class VacatePlaceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/places/{id:guid}/vacate", async (Guid id, VacatePlaceRequest request, ISender sender) =>
            {
                var command = new VacatePlace.Command
                {
                    Id = id,
                    MovedOutAt = request.MovedOutAt ?? DateTime.UtcNow,
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
            .WithName("Place.Vacate")
            .WithTags("Places")
            .Accepts<VacatePlaceRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}