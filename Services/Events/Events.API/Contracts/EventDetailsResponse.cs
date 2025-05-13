namespace Events.API.Contracts
{
    public class EventDetailsResponse
    {
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int? NumberOfAttendees { get; set; }

        public bool IsPublic { get; set; }

        public int CurrentParticipantsCount { get; set; }

        public ICollection<ParticipantDetailedResponse> Participants { get; init; } = new List<ParticipantDetailedResponse>();

        public Guid? BuildingId { get; set; }

        public Guid? RoomId { get; set; }
    }
}