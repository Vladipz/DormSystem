namespace Events.API.Entities
{
    public class InvitationToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public Guid EventId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsActive { get; set; } = true;

        public DormEvent Event { get; set; } = null!;
    }
}