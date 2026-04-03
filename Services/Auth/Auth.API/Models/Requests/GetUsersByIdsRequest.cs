namespace Auth.API.Models.Requests
{
    public sealed class GetUsersByIdsRequest
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
