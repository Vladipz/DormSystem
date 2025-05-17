namespace Inspections.API.Entities
{
    public class Inspection
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public InspectionStatus Status { get; set; }

        public ICollection<RoomInspection> Rooms { get; set; } = new List<RoomInspection>();
    }
}