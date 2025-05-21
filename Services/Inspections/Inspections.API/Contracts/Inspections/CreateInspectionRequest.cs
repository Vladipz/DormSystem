namespace Inspections.API.Contracts.Inspections
{
    public class CreateInspectionRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public string Mode { get; set; } = "manual";

        public Guid? DormitoryId { get; set; }

        public bool IncludeSpecialRooms { get; set; }

        public List<RoomDto> Rooms { get; set; } = new();
    }

    public class RoomDto
    {
        public Guid RoomId { get; set; }

        public string? RoomNumber { get; set; }

        public string? Floor { get; set; }

        public string? Building { get; set; }
    }
}
