namespace Shared.UserServiceClient
{
    public class UserDto
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    }
}