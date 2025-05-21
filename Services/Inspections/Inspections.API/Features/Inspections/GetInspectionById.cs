using Carter;
using Carter.OpenApi;

using ErrorOr;

using Inspections.API.Contracts.Inspections;
using Inspections.API.Data;
using Inspections.API.Mappings;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace Inspections.API.Features.Inspections
{
    public static class GetInspectionById
    {
        // --- 1. Query DTO ---
        public sealed class Query : IRequest<ErrorOr<InspectionDetailsResponse>>
        {
            public Guid Id { get; set; }
        }

        // --- 2. Handler ---
        internal sealed class Handler : IRequestHandler<Query, ErrorOr<InspectionDetailsResponse>>
        {
            private readonly ApplicationDbContext _db;

            public Handler(ApplicationDbContext db)
            {
                _db = db;
            }

            public async Task<ErrorOr<InspectionDetailsResponse>> Handle(Query request, CancellationToken cancellationToken)
            {
                var inspection = await _db.Inspections
                    .Include(x => x.Rooms)
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

                if (inspection == null)
                {
                    return Error.NotFound("Inspection.NotFound", "Inspection not found");
                }

                // Мапінг у DTO для відповіді
                var response = new InspectionDetailsResponse
                {
                    Id = inspection.Id,
                    Name = inspection.Name,
                    Type = inspection.Type,
                    StartDate = inspection.StartDate,
                    Status = inspection.Status,
                    Rooms = inspection.Rooms.Select(room => new RoomInspectionDto
                    {
                        Id = room.Id,
                        RoomId = room.RoomId,
                        RoomNumber = room.RoomNumber,
                        Floor = room.Floor,
                        Building = room.Building,
                        Status = room.Status,
                        Comment = room.Comment,
                    }).ToList(),
                };

                return response;
            }
        }
    }

    // --- 3. Endpoint ---
    public sealed class GetInspectionByIdEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/api/inspections/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetInspectionById.Query { Id = id });

                return result.Match(
                    ok => Results.Ok(ok),
                    error => error.ToResponse());
            })
            .Produces<InspectionDetailsResponse>(200)
            .Produces(404)
            .WithTags("Inspections")
            .WithName("GetInspectionById")
            .IncludeInOpenApi();
        }
    }
}