namespace Shared.UserServiceClient
{
    internal sealed class GetUsersByIdsRequest
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
    }
}
