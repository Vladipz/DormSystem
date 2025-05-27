using Auth.BLL.Interfaces;
using Auth.DAL.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Shared.PagedList;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserService _userService;

        public UserController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IUserService userService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="roleName">Role name.</param>
        /// <returns>Success or error response.</returns>
        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AddRoleToUser(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                return NotFound("Role not found");
            }

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role '{roleName}' added to user successfully" });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        /// <summary>
        /// Removes a role from a user.
        /// </summary>
        /// <param name="userId">User ID. </param>
        /// <param name="roleName">Role name.</param>
        /// <returns>Success or error response.</returns>
        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRoleFromUser(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
            {
                return BadRequest($"User is not in role '{roleName}'");
            }

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded)
            {
                return Ok(new { Message = $"Role '{roleName}' removed from user successfully" });
            }

            return BadRequest(new { Errors = result.Errors });
        }

        /// <summary>
        /// Gets a paginated list of users.
        /// </summary>
        /// <param name="pageNumber">Page number (starting from 1).</param>
        /// <param name="pageSize">Number of items per page (1-100).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Paginated list of users with their roles.</returns>
        /// <response code="200">Returns a paginated list of users with their information and roles.</response>
        /// <response code="400">Invalid pagination parameters or other error.</response>
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var result = await _userService.GetUsersAsync(pageNumber, pageSize, cancellationToken);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => BadRequest(new { Errors = errors.Select(e => e.Description) }));
        }

        /// <summary>
        /// Gets user information by ID.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <returns>User information including ID, username, email and roles.</returns>
        /// <response code="200">Returns the user's ID, username, email and assigned roles.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return NotFound("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                Roles = roles,
            });
        }
    }
}