using ErrorOr;

using Events.API.Contracts;

using Shared.UserServiceClient;

namespace Events.API.Services
{
    public class ParticipantEnricher
    {
        private readonly IAuthServiceClient _authService;
        private readonly ILogger<ParticipantEnricher> _logger;

        public ParticipantEnricher(IAuthServiceClient authService, ILogger<ParticipantEnricher> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Enriches a collection of short participants with user information, converting them to detailed responses.
        /// </summary>
        /// <param name="participants">The list of short participant responses to enrich.</param>
        /// <returns>A list of <see cref="ParticipantDetailedResponse"/> containing enriched participant information.</returns>
        public async Task<List<ParticipantDetailedResponse>> EnrichParticipantsAsync(List<ParticipantShortResponse> participants)
        {
            ArgumentNullException.ThrowIfNull(participants);

            if (participants.Count == 0)
            {
                return new List<ParticipantDetailedResponse>();
            }

            var userIds = participants.Select(p => p.UserId).ToList();

            var usersResult = await _authService.GetUsersByIdsAsync(userIds);

            if (usersResult.IsError)
            {
                _logger.LogWarning("Failed to get user information: {Error}", usersResult.FirstError.Description);
                return new List<ParticipantDetailedResponse>(); // Return empty list if enrichment failed
            }

            var users = usersResult.Value;
            var detailedParticipants = new List<ParticipantDetailedResponse>();

            // Convert short participants to detailed participants with user information
            foreach (var participant in participants)
            {
                var detailedParticipant = new ParticipantDetailedResponse
                {
                    UserId = participant.UserId,
                    JoinedAt = participant.JoinedAt,
                };

                if (users.TryGetValue(participant.UserId, out var user))
                {
                    detailedParticipant.FirstName = user.FirstName;
                    detailedParticipant.LastName = user.LastName;
                    detailedParticipant.Email = user.Email;
                }

                detailedParticipants.Add(detailedParticipant);
            }

            return detailedParticipants;
        }

        /// <summary>
        /// Enriches a single short participant with user information, converting it to a detailed response.
        /// </summary>
        public async Task<ParticipantDetailedResponse> EnrichParticipantAsync(ParticipantShortResponse participant)
        {
            ArgumentNullException.ThrowIfNull(participant);
            var userResult = await _authService.GetUserByIdAsync(participant.UserId);

            var detailedParticipant = new ParticipantDetailedResponse
            {
                UserId = participant.UserId,
                JoinedAt = participant.JoinedAt,
            };

            if (userResult.IsError)
            {
                _logger.LogWarning(
                    "Failed to get user information for {UserId}: {Error}",
                    participant.UserId,
                    userResult.FirstError.Description);
                return detailedParticipant; // Return participant without user details
            }

            var user = userResult.Value;

            detailedParticipant.FirstName = user.FirstName;
            detailedParticipant.LastName = user.LastName;
            detailedParticipant.Email = user.Email;

            return detailedParticipant;
        }
    }
}