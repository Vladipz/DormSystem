namespace Events.API.Contracts
{
    public sealed class EventResponce
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public ICollection<ParticipantResponse> LastParticipants { get; init; } = new List<ParticipantResponse>();

        public int? NumberOfAttendees { get; set; }
    }
}