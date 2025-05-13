namespace Rooms.API.Entities
{
    public class RoomEvent
    {
        public Guid Id { get; set; }

        public Guid RoomId { get; set; }

        public Room Room { get; set; } = null!;

        public Guid EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
    }
}