using Auth.API.Models.Requests;
using Auth.API.Models.Responses;
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

        // that first
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

        [HttpPost("token")]
        public async Task<IActionResult> Token([FromBody] TokenRequestModel request)
        {
            var result = await _authService.ValidateAndCreateTokensAsync(request.AuthCode, request.CodeVerifier);

            return result.Match<IActionResult>(
                success => Ok(new TokenResponse
                {
                    AccessToken = success.AccessToken,
                    RefreshToken = success.RefreshToken
                }),
                errors => errors[0].Type switch
                {
                    ErrorType.NotFound => NotFound(errors[0].Description),
                    ErrorType.Validation => BadRequest(errors[0].Description),
                    _ => StatusCode(500, errors[0].Description)
                });
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