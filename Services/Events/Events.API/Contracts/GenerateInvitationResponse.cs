namespace Events.API.Contracts
{
    public class GenerateInvitationResponse
    {
        public string InvitationLink { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}