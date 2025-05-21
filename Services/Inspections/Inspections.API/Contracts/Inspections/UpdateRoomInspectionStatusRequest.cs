namespace Inspections.API.Contracts.Inspections
{
    public class UpdateRoomInspectionStatusRequest
    {
        public string Status { get; set; } = string.Empty; // "pending" | "confirmed" | "not_confirmed" | "no_access"

        public string? Comment { get; set; }
    }
}
