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
    public static class GetPlaceById
    {
        internal sealed class Query : IRequest<ErrorOr<PlaceDetailsResponse>>
        {
            public Guid PlaceId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.PlaceId)
                    .NotEmpty()
                    .WithMessage("Place ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<PlaceDetailsResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<PlaceDetailsResponse>> Handle(Query request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<PlaceDetailsResponse>();
                }

                var place = await _dbContext.Places
                    .AsNoTracking()
                    .Include(p => p.Room)
                    .Where(p => p.Id == request.PlaceId)
                    .FirstOrDefaultAsync(ct);

                if (place is null)
                {
                    return Error.NotFound(
                        code: "Place.NotFound",
                        description: $"Place with ID {request.PlaceId} was not found.");
                }

                var response = place.Adapt<PlaceDetailsResponse>();
                response.RoomLabel = place.Room.Label;

                return response;
            }
        }
    }

    public sealed class GetPlaceByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/places/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetPlaceById.Query { PlaceId = id };
                var result = await sender.Send(query);

                return result.Match(
                    place => Results.Ok(place),
                    error => error.ToResponse());
            })
            .Produces<PlaceDetailsResponse>(200)
            .Produces(404)
            .WithName("Place.GetById")
            .WithTags("Places")
            .WithOpenApi(op =>
            {
                op.Summary = "Get place by ID";
                op.Parameters[0].Description = "Place ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}