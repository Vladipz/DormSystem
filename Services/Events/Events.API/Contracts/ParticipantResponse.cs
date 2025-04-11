namespace Events.API.Contracts
{
    /// <summary>
    /// Base class containing common participant properties.
    /// </summary>
    public abstract class BaseParticipantResponse
    {
        public Guid UserId { get; set; }

        public DateTime JoinedAt { get; set; }
    }

    /// <summary>
    /// Short participant response with minimal information.
    /// </summary>
    public sealed class ParticipantShortResponse : BaseParticipantResponse
    {
        // Inherits UserId and JoinedAt from base class
    }

    /// <summary>
    /// Detailed participant response with full user information.
    /// </summary>
    public sealed class ParticipantDetailedResponse : BaseParticipantResponse
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}