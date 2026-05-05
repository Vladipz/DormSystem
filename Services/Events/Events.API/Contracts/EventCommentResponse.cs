using Shared.UserServiceClient;

namespace Events.API.Contracts
{
    public sealed class EventCommentResponse
    {
        public Guid Id { get; set; }

        public Guid EventId { get; set; }

        public Guid AuthorUserId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public UserDto? Author { get; set; }
    }
}
