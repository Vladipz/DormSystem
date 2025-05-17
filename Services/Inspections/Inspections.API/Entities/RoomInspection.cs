namespace Inspections.API.Entities
{
    public class RoomInspection
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string RoomNumber { get; set; } = string.Empty;

        public string Floor { get; set; } = string.Empty;

        public string Building { get; set; } = string.Empty;

        public RoomInspectionStatus Status { get; set; }

        public string? Comment { get; set; } = string.Empty;

        public Guid InspectionId { get; set; }

        public Inspection Inspection { get; set; } = new Inspection();
    }
}