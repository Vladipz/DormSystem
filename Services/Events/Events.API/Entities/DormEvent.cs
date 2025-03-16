namespace Events.API.Entities
{
    public sealed class DormEvent
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public int? NumberOfAttendees { get; set; }

        public Guid OwnerId { get; set; }

        public ICollection<EventParticipant> Participants { get; init; } = new List<EventParticipant>();
    }
}