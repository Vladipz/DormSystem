using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Entities;

using ErrorOr;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly UserManager<User> _userManager;

        private static Dictionary<string, (string CodeChallenge, Guid UserId)> AuthCodes { get; } =
            [];

        private static Dictionary<string, string> RefreshTokens { get; } =
            [];

        public AuthService(IOptions<JwtSettings> jwtSettings, UserManager<User> userManager)
        {
            _jwtSettings = jwtSettings;
            _userManager = userManager;
        }

        public Task<ErrorOr<string>> CreateRefreshTokenAsync(string userId)
        {
            var oldRefreshToken = RefreshTokens.FirstOrDefault(x => x.Value == userId).Key;

            if (oldRefreshToken != null)
            {
                RefreshTokens.Remove(oldRefreshToken);
            }

            var refreshToken = Guid.NewGuid().ToString();

            RefreshTokens[refreshToken] = userId;

            return Task.FromResult<ErrorOr<string>>(refreshToken);
        }

        public Task<ErrorOr<string>> CreateTokenAsync(string userId)
        {
            using var sha256 = SHA256.Create();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Value.Issuer,
                audience: _jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Task.FromResult<ErrorOr<string>>(tokenString);
        }

        // TODO: refactor this method
        public async Task<ErrorOr<string>> GenerateAuthCodeAsync(string email, string password, string codeChallenge)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                var code = Convert.ToBase64String(Encoding.UTF8.GetBytes(codeChallenge));

                AuthCodes[code] = (codeChallenge, user.Id);
                return code;
            }
            else
            {
                return Error.NotFound(description: "Invalid email or password");
            }
        }

        public async Task<ErrorOr<(string accessToken, string refreshToken)>> RefreshTokenAsync(string refreshToken)
        {
            if (!RefreshTokens.TryGetValue(refreshToken, out var userId))
            {
                return Error.NotFound(description: "Invalid refresh token");
            }

            var accessTokenResult = await CreateTokenAsync(userId);
            if (accessTokenResult.IsError)
            {
                return accessTokenResult.Errors;
            }

            var newRefreshTokenResult = await CreateRefreshTokenAsync(userId);
            if (newRefreshTokenResult.IsError)
            {
                return newRefreshTokenResult.Errors;
            }

            return (accessTokenResult.Value, newRefreshTokenResult.Value);
        }

        public async Task<ErrorOr<Guid>> RegisterUserAsync(string email, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return Error.Conflict(description: "User with this email already exists");
            }

            var newUser = new User
            {
                Email = email,
                UserName = email,
            };

            var result = await _userManager.CreateAsync(newUser, password);

            if (result.Succeeded)
            {
                return newUser.Id;
            }
            else
            {
                return Error.Unexpected(description: "Failed to create user");
            }
        }

        public async Task<bool> VerifyAuthCodeAsync(string authCode, string codeVerifier)
        {
            if (!AuthCodes.TryGetValue(authCode, out var value))
            {
                return false;
            }

            using var sha256 = SHA256.Create();
            var hashedVerifier = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier)));

            return hashedVerifier == value.CodeChallenge;
        }
    }
}