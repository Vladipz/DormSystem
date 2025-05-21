using Inspections.API.Entities;

namespace Inspections.API.Contracts.Inspections
{
    public class InspectionDetailsResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public InspectionStatus Status { get; set; }

        public ICollection<RoomInspectionDto> Rooms { get; set; } = new List<RoomInspectionDto>();
    }

    public class RoomInspectionDto
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomNumber { get; set; }

        public string? Floor { get; set; }

        public string? Building { get; set; }

        public RoomInspectionStatus Status { get; set; }

        public string? Comment { get; set; }
    }
}