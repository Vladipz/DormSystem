using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Entities;

using ErrorOr;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shared.FileServiceClient;
using Shared.PagedList;

namespace Auth.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IFileServiceClient _fileServiceClient;

        public UserService(UserManager<User> userManager, IFileServiceClient fileServiceClient)
        {
            _userManager = userManager;
            _fileServiceClient = fileServiceClient;
        }

        public async Task<ErrorOr<PagedResponse<UserResponse>>> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _userManager.Users
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .Select(u => new UserResponse
                    {
                        Id = u.Id,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email ?? string.Empty,
                        AvatarUrl = !string.IsNullOrEmpty(u.AvatarId) ? _fileServiceClient.GetFileUrl(u.AvatarId) : null,
                    });

                var pagedUsers = await PagedList<UserResponse>.CreateAsync(
                    query,
                    pageNumber,
                    pageSize,
                    cancellationToken);

                // Get roles for each user
                foreach (var user in pagedUsers.Items)
                {
                    var userEntity = await _userManager.FindByIdAsync(user.Id.ToString());
                    if (userEntity != null)
                    {
                        var roles = await _userManager.GetRolesAsync(userEntity);
                        user.Roles = roles.ToList();
                    }
                }

                return PagedResponse<UserResponse>.FromPagedList(pagedUsers);
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to retrieve users: {ex.Message}");
            }
        }

        public async Task<ErrorOr<string>> UploadUserAvatarAsync(Guid userId, IFormFile avatarFile, CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate file
                if (avatarFile == null || avatarFile.Length == 0)
                {
                    return Error.Validation(description: "No file provided");
                }

                // Validate file type
                var allowedContentTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                if (!allowedContentTypes.Contains(avatarFile.ContentType.ToLower()))
                {
                    return Error.Validation(description: "Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed");
                }

                // Validate file size (5MB limit)
                const long maxFileSize = 5 * 1024 * 1024;
                if (avatarFile.Length > maxFileSize)
                {
                    return Error.Validation(description: "File size too large. Maximum size is 5MB");
                }

                // Find user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Error.NotFound(description: "User not found");
                }

                // Delete old avatar if exists
                if (!string.IsNullOrEmpty(user.AvatarId))
                {
                    await _fileServiceClient.DeleteFileAsync(user.AvatarId);
                }

                // Upload new avatar using the shared client
                var uploadResult = await _fileServiceClient.UploadAvatarAsync(avatarFile);

                if (uploadResult == null)
                {
                    return Error.Failure(description: "Failed to upload avatar file");
                }

                // Update user's avatar ID
                user.AvatarId = uploadResult.Id;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    // Clean up uploaded file if user update fails
                    await _fileServiceClient.DeleteFileAsync(uploadResult.Id);
                    return Error.Failure(description: $"Failed to update user avatar: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }

                return uploadResult.Url;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to upload avatar: {ex.Message}");
            }
        }

        public async Task<ErrorOr<bool>> DeleteUserAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Error.NotFound(description: "User not found");
                }

                if (string.IsNullOrEmpty(user.AvatarId))
                {
                    return Error.NotFound(description: "User has no avatar to delete");
                }

                // Delete file from storage using the shared client
                var deleteResult = await _fileServiceClient.DeleteFileAsync(user.AvatarId);
                
                // Update user record (remove avatar ID even if file deletion failed)
                user.AvatarId = null;
                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return Error.Failure(description: $"Failed to update user: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }

                return deleteResult;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to delete avatar: {ex.Message}");
            }
        }

        public async Task<ErrorOr<UserResponse>> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return Error.NotFound(description: "User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);

                var userResponse = new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    AvatarUrl = !string.IsNullOrEmpty(user.AvatarId) ? _fileServiceClient.GetFileUrl(user.AvatarId) : null,
                    Roles = roles.ToList()
                };

                return userResponse;
            }
            catch (Exception ex)
            {
                return Error.Failure(description: $"Failed to retrieve user: {ex.Message}");
            }
        }
    }
} 