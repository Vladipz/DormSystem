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
    public static class UpdateBuilding
    {
        internal sealed class Command : IRequest<ErrorOr<UpdatedBuildingResponse>>
        {
            public Guid Id { get; set; }

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
                RuleFor(x => x.Id).NotEmpty();
                RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
                RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
                RuleFor(x => x.FloorsCount).GreaterThan(0).LessThanOrEqualTo(200);
                RuleFor(x => x.YearBuilt).InclusiveBetween(1800, DateTime.UtcNow.Year);
                RuleFor(x => x.AdministratorContact).NotEmpty().MaximumLength(100);
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<UpdatedBuildingResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<UpdatedBuildingResponse>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<UpdatedBuildingResponse>();
                }

                var building = await _dbContext.Buildings
                    .FirstOrDefaultAsync(b => b.Id == request.Id, ct);

                if (building is null)
                {
                    return Error.NotFound(
                        code: "Building.NotFound",
                        description: $"Building with ID {request.Id} was not found.");
                }

                building.Name = request.Name;
                building.Address = request.Address;
                building.FloorsCount = request.FloorsCount;
                building.YearBuilt = request.YearBuilt;
                building.AdministratorContact = request.AdministratorContact;
                building.IsActive = request.IsActive;

                await _dbContext.SaveChangesAsync(ct);

                return new UpdatedBuildingResponse { Id = building.Id };
            }
        }
    }

    public sealed class UpdateBuildingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/buildings/{id:guid}", async (Guid id, UpdateBuildingRequest request, ISender sender) =>
            {
                if (id != request.Id)
                {
                    return Results.BadRequest(Error.Validation(
                        code: "Building.IdMismatch",
                        description: "URL ID and request body ID do not match."));
                }

                var command = request.Adapt<UpdateBuilding.Command>();
                var result = await sender.Send(command);

                return result.Match(
                    updated => Results.Ok(updated),
                    error => error.ToResponse());
            })
            .Produces<UpdatedBuildingResponse>(200)
            .Produces<Error>(400)
            .Produces<Error>(404)
            .WithName("Buildings.UpdateBuilding")
            .WithTags("Buildings")
            .Accepts<UpdateBuildingRequest>("application/json")
            .IncludeInOpenApi()
            .RequireAuthorization("AdminOnly");
        }
    }
}