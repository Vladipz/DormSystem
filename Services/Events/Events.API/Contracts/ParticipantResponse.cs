namespace Events.API.Contracts
{
    public sealed class ParticipantResponse
    {
        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}