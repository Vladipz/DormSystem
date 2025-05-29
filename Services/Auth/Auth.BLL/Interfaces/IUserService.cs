using Auth.BLL.Models;

using ErrorOr;

using Microsoft.AspNetCore.Http;

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

        /// <summary>
        /// Uploads or updates a user's avatar.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="avatarFile">The avatar image file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success result with avatar URL or an error.</returns>
        Task<ErrorOr<string>> UploadUserAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user's avatar.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success result or an error.</returns>
        Task<ErrorOr<bool>> DeleteUserAvatarAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by ID with avatar URL.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>User response with avatar URL or an error.</returns>
        Task<ErrorOr<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}