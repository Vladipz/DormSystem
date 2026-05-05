namespace Events.API.Contracts
{
    public sealed class CreateEventCommentRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
