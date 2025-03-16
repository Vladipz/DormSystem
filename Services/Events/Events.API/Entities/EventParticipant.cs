namespace Events.API.Entities
{
    public sealed class EventParticipant
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public DormEvent Event { get; set; } = null!;

        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}