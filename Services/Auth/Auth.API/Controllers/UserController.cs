using Auth.API.Mappins;
using Auth.BLL.Interfaces;
using Auth.DAL.Entities;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Shared.PagedList;
using Shared.TokenService.Services;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;

        public UserController(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IUserService userService,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Adds a role to a user.
        /// </summary>
        /// <param name="userId">User ID.</param>
        /// <param name="roleName">Role name.</param>
        /// <returns>Success or error response.</returns>
        [HttpPost("{userId}/roles/{roleName}")]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        /// <returns>User information including ID, username, email, avatar URL and roles.</returns>
        /// <response code="200">Returns the user's ID, username, email, avatar URL and assigned roles.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(Guid userId)
        {
            var result = await _userService.GetUserByIdAsync(userId);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => NotFound(new { Errors = errors.Select(e => e.Description) }));
        }

        /// <summary>
        /// Gets the current user's profile information.
        /// </summary>
        /// <returns>Current user's information including ID, username, email, avatar URL and roles.</returns>
        /// <response code="200">Returns the current user's ID, username, email, avatar URL and assigned roles.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            // Get user ID from token
            var userIdResult = _tokenService.GetUserId(HttpContext);
            if (userIdResult.IsError)
            {
                return Unauthorized(new { Errors = userIdResult.Errors.Select(e => e.Description) });
            }

            var result = await _userService.GetUserByIdAsync(userIdResult.Value);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => NotFound(new { Errors = errors.Select(e => e.Description) }));
        }

        /// <summary>
        /// Uploads or updates the current user's avatar.
        /// </summary>
        /// <param name="avatarFile">The avatar image file (JPEG, PNG, GIF, WebP, max 5MB).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success response with avatar URL or error.</returns>
        /// <response code="200">Avatar uploaded successfully.</response>
        /// <response code="400">Invalid file or validation error.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("avatar")]
        [Authorize] // Require authentication
        public async Task<IActionResult> UploadAvatar(
            IFormFile avatarFile,
            CancellationToken cancellationToken = default)
        {
            // Get user ID from token
            var userIdResult = _tokenService.GetUserId(HttpContext);
            if (userIdResult.IsError)
            {
                return Unauthorized(new { Errors = userIdResult.Errors.Select(e => e.Description) });
            }

            var result = await _userService.UploadUserAvatarAsync(userIdResult.Value, avatarFile, cancellationToken);

            return result.Match(
                success => Ok(new { AvatarUrl = success }),
                errors => errors.ToResponse());
        }

        /// <summary>
        /// Deletes the current user's avatar.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Success or error response.</returns>
        /// <response code="200">Avatar deleted successfully.</response>
        /// <response code="401">User not authenticated.</response>
        /// <response code="404">User or avatar not found.</response>
        [HttpDelete("avatar")]
        [Authorize] // Require authentication
        public async Task<IActionResult> DeleteAvatar(CancellationToken cancellationToken = default)
        {
            // Get user ID from token
            var userIdResult = _tokenService.GetUserId(HttpContext);
            if (userIdResult.IsError)
            {
                return Unauthorized(new { Errors = userIdResult.Errors.Select(e => e.Description) });
            }

            var result = await _userService.DeleteUserAvatarAsync(userIdResult.Value, cancellationToken);

            return result.Match(
                success => Ok(),
                errors => errors.ToResponse());
        }
    }
}