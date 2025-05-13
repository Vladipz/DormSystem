namespace Rooms.API.Entities
{
    public class BuildingEvent
    {
        public Guid Id { get; set; }

        public Guid BuildingId { get; set; }

        public Building Building { get; set; } = null!;

        public Guid EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public bool IsPublic { get; set; }
    }
}