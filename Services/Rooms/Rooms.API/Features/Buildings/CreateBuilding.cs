using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Mapster;

using MediatR;

using Rooms.API.Contracts.Building;
using Rooms.API.Data;
using Rooms.API.Entities;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Buildings
{
    public static class CreateBuilding
    {
        internal sealed class Command : IRequest<ErrorOr<CreateBuildingResponse>>
        {
            public string Name { get; set; } = string.Empty;

            public string Address { get; set; } = string.Empty;

            public int FloorsCount { get; set; }

            public int YearBuilt { get; set; }

            public string AdministratorContact { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
                RuleFor(x => x.FloorsCount).GreaterThan(0).LessThanOrEqualTo(200);
                RuleFor(x => x.YearBuilt).InclusiveBetween(1800, DateTime.UtcNow.Year);
                RuleFor(x => x.AdministratorContact).NotEmpty().MaximumLength(100);
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<CreateBuildingResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<CreateBuildingResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<CreateBuildingResponse>();
                }

                var building = new Building
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
                    Address = request.Address,
                    FloorsCount = request.FloorsCount,
                    YearBuilt = request.YearBuilt,
                    AdministratorContact = request.AdministratorContact,
                    IsActive = request.IsActive,
                };

                _dbContext.Buildings.Add(building);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new CreateBuildingResponse { Id = building.Id };
            }
        }
    }

    public sealed class CreateBuildingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/buildings", async (CreateBuildingRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateBuilding.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    response => Results.Created($"/buildings/{response.Id}", response),
                    error => error.ToResponse());
            })
            .Produces<CreateBuildingResponse>(201)
            .Produces<Error>(400)
            .WithName("Buildings.CreateBuilding")
            .WithTags("Buildings")
            .RequireAuthorization("AdminOnly")
            .Accepts<CreateBuildingRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}