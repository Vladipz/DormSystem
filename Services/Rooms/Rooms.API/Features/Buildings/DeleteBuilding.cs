using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using MediatR;

using Microsoft.EntityFrameworkCore;

using Rooms.API.Contracts.Building;
using Rooms.API.Data;
using Rooms.API.Mappings;

namespace Rooms.API.Features.Buildings
{
    public static class DeleteBuilding
    {
        internal sealed class Command : IRequest<ErrorOr<DeletedBuildingResponse>>
        {
            public Guid Id { get; set; }
        }

        internal sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.Id)
                    .NotEmpty()
                    .WithMessage("Building ID must not be empty.");
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<DeletedBuildingResponse>>
        {
            private readonly ApplicationDbContext _dbContext;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext dbContext, IValidator<Command> validator)
            {
                _dbContext = dbContext;
                _validator = validator;
            }

            public async Task<ErrorOr<DeletedBuildingResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var validation = await _validator.ValidateAsync(request, cancellationToken);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<DeletedBuildingResponse>();
                }

                var building = await _dbContext.Buildings
                    .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

                if (building is null)
                {
                    return Error.NotFound(
                        code: "Building.NotFound",
                        description: $"Building with ID {request.Id} was not found.");
                }

                _dbContext.Buildings.Remove(building);
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new DeletedBuildingResponse { Id = building.Id };
            }
        }
    }

    public sealed class DeleteBuildingEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapDelete("/buildings/{id:guid}", async (Guid id, ISender sender) =>
            {
                var command = new DeleteBuilding.Command { Id = id };
                var result = await sender.Send(command);

                return result.Match(
                    deleted => Results.Ok(deleted),
                    error => error.ToResponse());
            })
            .Produces<DeletedBuildingResponse>(200)
            .Produces<Error>(404)
            .WithName("Buildings.DeleteBuilding")
            .WithTags("Buildings")
            .RequireAuthorization("AdminOnly")
            .WithOpenApi(op =>
            {
                op.Summary = "Delete building by ID";
                op.Parameters[0].Description = "Building ID";
                return op;
            })
            .IncludeInOpenApi();
        }
    }
}