namespace Events.API.Contracts
{
    public sealed class EventResponce
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public ICollection<ParticipantShortResponse> LastParticipants { get; init; } = new List<ParticipantShortResponse>();

        public int? NumberOfAttendees { get; set; }

        public bool IsPublic { get; set; }
    }
}