using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Floor;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Floors
{
    public static class GetFloorById
    {
        internal sealed class Query : IRequest<ErrorOr<FloorDetailsResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Floor ID must not be empty");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<FloorDetailsResponse>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext db, IValidator<Query> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<FloorDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<FloorDetailsResponse>();
                }

                var floor = await _db.Floors
                    .AsNoTracking()
                    .Include(f => f.Building)
                    .Include(f => f.Blocks)
                    .FirstOrDefaultAsync(f => f.Id == request.Id, cancellationToken);

                if (floor is null)
                {
                    return Error.NotFound(
                        code: "Floor.NotFound",
                        description: $"Floor with ID {request.Id} was not found");
                }

                var response = new FloorDetailsResponse
                {
                    Id = floor.Id,
                    BuildingId = floor.BuildingId,
                    Number = floor.Number,
                    BlocksCount = floor.Blocks.Count,
                    BuildingName = floor.Building.Name,
                    BuildingAddress = floor.Building.Address
                };

                return response;
            }
        }
    }

    public sealed class GetFloorByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/floors/{id:guid}", async (
                Guid id,
                ISender sender) =>
            {
                var query = new GetFloorById.Query { Id = id };
                var result = await sender.Send(query);

                return result.Match(
                    success => Results.Ok(success),
                    error => error.ToResponse());
            })
            .Produces<FloorDetailsResponse>(200)
            .Produces(404)
            .WithName("GetFloorById")
            .WithTags("Floors")
            .WithOpenApi(op =>
            {
                op.Summary = "Get floor details by ID";
                op.Parameters[0].Description = "Floor ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}