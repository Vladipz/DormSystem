using Auth.BLL.Models;

using ErrorOr;

using Shared.PagedList;

namespace Auth.BLL.Interfaces
{
    /// <summary>
    /// Provides user management services.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Gets a paginated list of users.
        /// </summary>
        /// <param name="pageNumber">The page number (starting from 1).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A paginated response containing users or an error.</returns>
        Task<ErrorOr<PagedResponse<UserResponse>>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
} 