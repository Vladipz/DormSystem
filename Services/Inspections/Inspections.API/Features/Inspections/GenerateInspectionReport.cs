// Features/Inspections/GenerateInspectionReport.cs
using Carter;

using ErrorOr;

using Inspections.API.Data;
using Inspections.API.Entities;
using Inspections.API.Mappings;
using Inspections.API.Services;

using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inspections.API.Features.Inspections;

public static class GenerateInspectionReport
{
    // ───────────────────────────────────────────── Command/Query
    public sealed record Query(Guid InspectionId, ReportStyle Style) : IRequest<ErrorOr<byte[]>>;

    public enum ReportStyle
    {
        Simple,
        Fancy
    }

    // ───────────────────────────────────────────── Handler
    internal sealed class Handler : IRequestHandler<Query, ErrorOr<byte[]>>
    {
        private readonly ApplicationDbContext _db;
        private readonly IPdfReportService _pdf;

        public Handler(ApplicationDbContext db, IPdfReportService pdf)
        {
            _db = db;
            _pdf = pdf;
        }

        public async Task<ErrorOr<byte[]>> Handle(Query rq, CancellationToken ct)
        {
            var inspection = await _db.Inspections
                                      .Include(i => i.Rooms)
                                      .FirstOrDefaultAsync(i => i.Id == rq.InspectionId, ct);

            if (inspection is null)
                return Error.NotFound("Inspection.NotFound", "Inspection not found.");

            if (inspection.Status != InspectionStatus.Completed)
                return Error.Conflict("Inspection.NotCompleted",
                                      "Report available only after completion.");

            // ── PDF generation based on style
            byte[] pdf = rq.Style == ReportStyle.Fancy
                ? _pdf.GenerateFancyReport(inspection)
                : _pdf.GenerateSimpleReport(inspection);

            return pdf;
        }
    }
}

// ───────────────────────────────────────────── Endpoint
public sealed class GenerateInspectionReportEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        /*  GET /api/inspections/{id}/report?style=fancy
            styles: simple (default) | fancy                                    */
        app.MapGet("/api/inspections/{id:guid}/report", async (
            Guid id,
            [FromQuery] string? style,
            ISender sender) =>
        {
            // parse style, fallback = Simple
            var parsed = Enum.TryParse(
                style,
                ignoreCase: true,
                out GenerateInspectionReport.ReportStyle s)
                ? s
                : GenerateInspectionReport.ReportStyle.Simple;

            var query = new GenerateInspectionReport.Query(id, parsed);
            var result = await sender.Send(query);

            return result.Match(
                bytes => Results.File(
                    bytes,
                    "application/pdf",
                    $"inspection-{id}-{parsed.ToString().ToLowerInvariant()}.pdf"),
                err => err.ToResponse());
        })
        .Produces<FileContentResult>(200, "application/pdf")
        .Produces<Error>(400)
        .WithName("GenerateInspectionReport")
        .WithTags("Inspections")
        .WithOpenApi(op =>
        {
            op.Summary = "Download PDF report for a completed inspection";
            op.Parameters[1].Description =
                "Report style: 'simple' (default) or 'fancy'";
            return op;
        });
    }
}
