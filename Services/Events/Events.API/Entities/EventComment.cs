namespace Events.API.Entities
{
    public sealed class EventComment
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public DormEvent Event { get; set; } = null!;

        public Guid AuthorUserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }
    }
}
