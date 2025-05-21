using Carter;
using Carter.OpenApi;

using ErrorOr;

using FluentValidation;

using Inspections.API.Data;
using Inspections.API.Entities;
using Inspections.API.Mappings;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Inspections.API.Features.Inspections
{
    public static class StartInspection
    {
        public sealed class Command : IRequest<ErrorOr<Unit>>
        {
            public Guid InspectionId { get; set; }
        }

        public sealed class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(x => x.InspectionId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, ErrorOr<Unit>>
        {
            private readonly ApplicationDbContext _db;
            private readonly IValidator<Command> _validator;

            public Handler(ApplicationDbContext db, IValidator<Command> validator)
            {
                _db = db;
                _validator = validator;
            }

            public async Task<ErrorOr<Unit>> Handle(Command request, CancellationToken ct)
            {
                var validation = await _validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    return validation.ToValidationError<Unit>();
                }

                var inspection = await _db.Inspections.FirstOrDefaultAsync(x => x.Id == request.InspectionId, ct);
                if (inspection is null)
                {
                    return Error.NotFound("Inspection.NotFound", "Inspection not found");
                }

                if (inspection.Status != InspectionStatus.Scheduled)
                {
                    return Error.Conflict("Inspection.InvalidState", "Inspection is not in a schedulable state");
                }

                inspection.Status = InspectionStatus.Active;

                await _db.SaveChangesAsync(ct);
                return Unit.Value;
            }
        }
    }

    public sealed class StartInspectionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/api/inspections/{id:guid}/start", async (Guid id, ISender sender) =>
            {
                var command = new StartInspection.Command { InspectionId = id };

                var result = await sender.Send(command);

                return result.Match(
                    _ => Results.NoContent(),
                    error => error.ToResponse());
            })
            .Produces(204)
            .Produces(400)
            .Produces(404)
            .Produces(409)
            .WithName("StartInspection")
            .WithTags("Inspections")
            .IncludeInOpenApi();
        }
    }
}