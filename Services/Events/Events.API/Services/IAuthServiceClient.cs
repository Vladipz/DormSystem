using ErrorOr;

using Events.API.Contracts;

namespace Events.API.Services
{
    public interface IAuthServiceClient
    {
        /// <summary>
        /// Gets user information by user ID.
        /// </summary>
        /// <param name="userId">The user ID to retrieve information for.</param>
        /// <returns>User information or error.</returns>
        Task<ErrorOr<UserDto>> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// Gets user information for multiple user IDs in a batch.
        /// </summary>
        /// <param name="userIds">Collection of user IDs.</param>
        /// <returns>Dictionary mapping user IDs to their information.</returns>
        Task<ErrorOr<Dictionary<Guid, UserDto>>> GetUsersByIdsAsync(IEnumerable<Guid> userIds);
    }
}