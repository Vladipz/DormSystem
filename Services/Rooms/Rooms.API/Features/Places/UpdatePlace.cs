using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Place;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Places
{
    public static class UpdatePlace
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatePlaceResponse>>
        {
            public Guid Id { get; set; }

            public int Index { get; set; }

            public Guid? OccupiedByUserId { get; set; }

            public DateTime? MovedInAt { get; set; }

            public DateTime? MovedOutAt { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Index).GreaterThanOrEqualTo(1);
                RuleFor(x => x.MovedInAt)
                    .NotEmpty()
                    .When(x => x.OccupiedByUserId is not null)
                    .WithMessage("Move-in date is required when the place is occupied.");
                RuleFor(x => x.MovedOutAt)
                    .GreaterThan(x => x.MovedInAt)
                    .When(x => x.MovedInAt.HasValue && x.MovedOutAt.HasValue)
                    .WithMessage("Move-out date must be after move-in date.");
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

                // Check for index conflicts (only if index is being changed)
                if (place.Index != request.Index)
                {
                    var indexExists = await _dbContext.Places
                        .AnyAsync(p => p.RoomId == place.RoomId && p.Index == request.Index && p.Id != request.Id, ct);

                    if (indexExists)
                    {
                        return Error.Conflict(
                            code: "Place.IndexAlreadyExists",
                            description: $"Place with index {request.Index} already exists in the room.");
                    }
                }

                place.Index = request.Index;
                place.OccupiedByUserId = request.OccupiedByUserId;
                place.MovedInAt = request.MovedInAt;
                place.MovedOutAt = request.MovedOutAt;

                await _dbContext.SaveChangesAsync(ct);

                return new UpdatePlaceResponse { Id = place.Id };
            }
        }
    }

    public sealed class UpdatePlaceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/places/{id:guid}", static async (Guid id, UpdatePlaceRequest request, ISender sender) =>
            {
                if (id != request.Id)
                {
                    return Results.BadRequest(Error.Validation(
                        code: "Place.IdMismatch",
                        description: "URL ID and request body ID do not match."));
                }

                var command = request.Adapt<UpdatePlace.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    updated => Results.Ok(updated),
                    error => error.ToResponse());
            })
            .Produces<UpdatePlaceResponse>(200)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("Place.Update")
            .WithTags("Places")
            .Accepts<UpdatePlaceRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
} 