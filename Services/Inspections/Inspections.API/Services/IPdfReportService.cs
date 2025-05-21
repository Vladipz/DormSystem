using Inspections.API.Entities;

namespace Inspections.API.Services
{
    public interface IPdfReportService
    {
        byte[] GenerateSimpleReport(Inspection inspection);

        byte[] GenerateFancyReport(Inspection inspection);
    }
}