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
    public static class DeletePlace
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedPlaceResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Place ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedPlaceResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<DeletedPlaceResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<DeletedPlaceResponse>();
                }

                var place = await _dbContext.Places
                    .FirstOrDefaultAsync(p => p.Id == request.Id, ct);

                if (place is null)
                {
                    return Error.NotFound(
                        code: "Place.NotFound",
                        description: $"Place with ID {request.Id} was not found.");
                }

                _dbContext.Places.Remove(place);
                await _dbContext.SaveChangesAsync(ct);

                return new DeletedPlaceResponse { Id = place.Id };
            }
        }
    }

    public sealed class DeletePlaceEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/places/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeletePlace.Command { Id = id };
                var result = await sender.Send(command);

                return result.Match(
                    deleted => Results.Ok(deleted),
                    error => error.ToResponse());
            })
            .Produces<DeletedPlaceResponse>(200)
            .Produces(404)
            .WithName("Place.Delete")
            .WithTags("Places")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete place by ID";
                op.Parameters[0].Description = "Place ID";
                return op;
            })
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}