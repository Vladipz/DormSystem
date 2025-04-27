using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Building;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Buildings
{
    public static class GetBuildingById
    {
        internal sealed class Query : IRequest<ErrorOr<BuildingDetailsResponse>>
        {
            public Guid BuildingId { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.BuildingId)
                    .NotEmpty()
                    .WithMessage("Building ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Query, ErrorOr<BuildingDetailsResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Query> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Query> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<BuildingDetailsResponse>> Handle(Query request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<BuildingDetailsResponse>();
                }

                var building = await _dbContext.Buildings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == request.BuildingId, ct);

                if (building is null)
                {
                    return Error.NotFound(
                        code: "Building.NotFound",
                        description: $"Building with ID {request.BuildingId} was not found.");
                }

                return building.Adapt<BuildingDetailsResponse>();
            }
        }
    }

    public sealed class GetBuildingByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/buildings/{id:guid}", async (Guid id, ISender sender) =>
            {
                var query = new GetBuildingById.Query { BuildingId = id };
                var result = await sender.Send(query);

                return result.Match(
                    building => Results.Ok(building),
                    error => error.ToResponse());
            })
            .Produces<BuildingDetailsResponse>(200)
            .Produces<Error>(404)
            .WithName("Buildings.GetBuildingById")
            .WithTags("Buildings")
            .AllowAnonymous()
            .WithOpenApi(op =>
            {
                op.Summary = "Get building by ID";
                op.Parameters[0].Description = "Building ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}