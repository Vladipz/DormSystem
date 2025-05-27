using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Entities;

using ErrorOr;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shared.PagedList;

namespace Auth.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;

        public UserService(UserManager<User> userManager)
        {
            _userManager = userManager;
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
                        Email = u.Email ?? string.Empty
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
    }
} 