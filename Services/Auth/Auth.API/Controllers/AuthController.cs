using Auth.BLL.Interfaces;

using ErrorOr;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        public record RegisterRequest(string Email, string Password);
        public record AuthRequestModel(string Email, string Password, string CodeChallenge);
        public record TokenRequestModel(string Code, string CodeVerifier);
        public record RefreshTokenRequestModel(string RefreshToken);

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterUserAsync(request.Email, request.Password);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => errors[0].Type switch
                {
                    ErrorType.Conflict => Conflict(errors[0].Description),
                    ErrorType.Unexpected => StatusCode(500, errors[0].Description),
                });
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Authorize([FromBody] AuthRequestModel request)
        {
            var result = await _authService.GenerateAuthCodeAsync(request.Email, request.Password, request.CodeChallenge);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => errors[0].Type switch
                {
                    ErrorType.NotFound => NotFound(errors[0].Description),
                });
        }

        // TODO: need refactoring
        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] TokenRequestModel request)
        {
            var result = await _authService.VerifyAuthCodeAsync(request.Code, request.CodeVerifier);

            if (!result)
            {
                return NotFound();
            }

            var tokenResult = await _authService.CreateTokenAsync(request.Code);

            if (tokenResult.IsError)
            {
                return StatusCode(500, tokenResult.Errors[0].Description);
            }

            var refreshTokenResult = await _authService.CreateRefreshTokenAsync(request.Code);

            if (refreshTokenResult.IsError)
            {
                return StatusCode(500, refreshTokenResult.Errors[0].Description);
            }

            return Ok(new { AccessToken = tokenResult.Value, RefreshToken = refreshTokenResult.Value });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestModel request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);

            return result.Match<IActionResult>(
                success => Ok(success),
                errors => errors[0].Type switch
                {
                    ErrorType.NotFound => NotFound(errors[0].Description),
                    ErrorType.Unexpected => StatusCode(500, errors[0].Description),
                });
        }

        // test protected endpoint
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            return Ok("Protected");
        }
    }
}