using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Auth.BLL.Interfaces;
using Auth.BLL.Models;
using Auth.DAL.Data;
using Auth.DAL.Entities;

using ErrorOr;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Auth.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly UserManager<User> _userManager;
        private readonly AuthDbContext _dbContext;

        public AuthService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<User> userManager,
            AuthDbContext dbContext)
        {
            _jwtSettings = jwtSettings;
            _userManager = userManager;
            _dbContext = dbContext;
        }

        public async Task<ErrorOr<string>> CreateRefreshTokenAsync(Guid userId)
        {
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Delete any existing refresh tokens for this user
                    // Using ExecuteDeleteAsync is more efficient and reduces concurrency issues
                    await _dbContext.RefreshTokens
                        .Where(rt => rt.UserId == userId)
                        .ExecuteDeleteAsync();

                    var refreshToken = Guid.NewGuid().ToString();

                    var refreshTokenEntity = new RefreshToken
                    {
                        UserId = userId,
                        Token = refreshToken,
                        Expires = DateTime.UtcNow.AddDays(_jwtSettings.Value.RefreshTokenExpirationDays),
                    };

                    _dbContext.RefreshTokens.Add(refreshTokenEntity);
                    await _dbContext.SaveChangesAsync();

                    return refreshToken;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If this is the last attempt, throw the error
                    if (attempt == maxRetries - 1)
                    {
                        return Error.Failure(description: "Could not create refresh token due to concurrency issues");
                    }

                    // Otherwise, wait a small amount of time and retry
                    await Task.Delay(50 * (attempt + 1)); // Exponential backoff
                }
                catch (Exception ex)
                {
                    return Error.Failure(description: $"Failed to create refresh token: {ex.Message}");
                }
            }

            // This should never be reached
            return Error.Unexpected(description: "Unexpected error during refresh token creation");
        }

        public async Task<ErrorOr<string>> CreateTokenAsync(Guid userId)
        {
            using var sha256 = SHA256.Create();

            // Get user and their roles
            User? user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                return Error.NotFound("User not found");
            }

            IList<string> roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, userId.ToString()),
                new (ClaimTypes.GivenName, user.FirstName),
                new (ClaimTypes.Surname, user.LastName),
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Value.Issuer,
                audience: _jwtSettings.Value.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenString;
        }

        public async Task<ErrorOr<string>> GenerateAuthCodeAsync(string email, string password, string codeChallenge)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user == null || !await _userManager.CheckPasswordAsync(user, password))
                {
                    return Error.NotFound(description: "Invalid email or password");
                }

                string code = Convert.ToBase64String(Encoding.UTF8.GetBytes(codeChallenge)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

                _dbContext.AuthCodes.Add(new AuthCode
                {
                    Code = code,
                    CodeChallenge = codeChallenge,
                    UserId = user.Id,
                });

                await _dbContext.SaveChangesAsync();

                return code;
            }
            catch (DbUpdateException)
            {
                return Error.Failure(description: "Database error occurred while saving authorization code");
            }
        }

        public async Task<ErrorOr<TokenModel>> RefreshTokenAsync(string refreshToken)
        {
            // First, retrieve the refresh token in a separate transaction with tracking disabled to avoid
            // concurrency issues when the same refresh token is requested multiple times
            var refreshTokenEntity = await _dbContext.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

            if (refreshTokenEntity == null)
            {
                return Error.NotFound(description: "Invalid refresh token");
            }

            // Store the user ID from the token entity
            var userId = refreshTokenEntity.UserId;

            // Begin concurrency handling with retry logic
            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Here we'll use a new context operation to delete the token
                    // This prevents the same token from being used more than once
                    var existingToken = await _dbContext.RefreshTokens
                        .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                    if (existingToken == null)
                    {
                        // Token was already used and deleted
                        return Error.Conflict(description: "Refresh token has already been used");
                    }

                    // Remove the old token
                    _dbContext.RefreshTokens.Remove(existingToken);
                    await _dbContext.SaveChangesAsync();

                    // Now generate the new tokens
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

                    return new Models.TokenModel
                    {
                        AccessToken = accessTokenResult.Value,
                        RefreshToken = newRefreshTokenResult.Value,
                    };
                }
                catch (DbUpdateConcurrencyException)
                {
                    // If this is the last attempt, throw the error
                    if (attempt == maxRetries - 1)
                    {
                        return Error.Conflict(description: "Could not process refresh token due to concurrency issues");
                    }

                    // Otherwise, wait a small amount of time and retry
                    await Task.Delay(50 * (attempt + 1)); // Exponential backoff
                }
            }

            // This should never be reached due to the return in the final attempt
            return Error.Unexpected(description: "Unexpected error during token refresh");
        }

        public async Task<ErrorOr<Guid>> RegisterUserAsync(RegisterUserRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Error.Conflict(description: "User with this email already exists");
            }

            var newUser = new User
            {
                Email = request.Email,
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);

            if (result.Succeeded)
            {
                return newUser.Id;
            }
            else
            {
                return Error.Unexpected(description: "Failed to create user");
            }
        }

        public async Task<ErrorOr<bool>> VerifyAuthCodeAsync(string authCode, string codeVerifier)
        {
            // move to fluent validation
            var validationResult = ValidateInputs(authCode, codeVerifier);
            if (validationResult.IsError)
            {
                return validationResult.Errors;
            }

            var authCodeResult = await GetAuthCode(authCode);
            if (authCodeResult.IsError)
            {
                return authCodeResult.Errors;
            }

            var authCodeEntity = authCodeResult.Value;

            if (!IsCodeVerifierValid(codeVerifier, authCodeEntity.CodeChallenge))
            {
                return false;
            }

            return true;
        }

        public async Task<ErrorOr<TokenModel>> ValidateAndCreateTokensAsync(string authCode, string codeVerifier)
        {
            var verifyResult = await VerifyAuthCodeAsync(authCode, codeVerifier);
            if (verifyResult.IsError)
            {
                return verifyResult.Errors;
            }

            if (!verifyResult.Value)
            {
                return Error.NotFound("Invalid code verifier");
            }

            // Get user ID from auth code
            var authCodeEntity = await _dbContext.AuthCodes.FirstOrDefaultAsync(ac => ac.Code == authCode);
            if (authCodeEntity == null)
            {
                return Error.NotFound("Auth code not found");
            }

            var userId = authCodeEntity.UserId;

            // Delete the auth code now that we've verified and retrieved the user ID
            var deleteResult = await DeleteAuthCodeAsync(authCode);
            if (deleteResult.IsError)
            {
                return deleteResult.Errors;
            }

            var accessTokenResult = await CreateTokenAsync(userId);
            if (accessTokenResult.IsError)
            {
                return accessTokenResult.Errors;
            }

            var refreshTokenResult = await CreateRefreshTokenAsync(userId);
            if (refreshTokenResult.IsError)
            {
                return refreshTokenResult.Errors;
            }

            return new TokenModel
            {
                AccessToken = accessTokenResult.Value,
                RefreshToken = refreshTokenResult.Value,
            };
        }

        private ErrorOr<Success> ValidateInputs(string authCode, string codeVerifier)
        {
            if (string.IsNullOrWhiteSpace(authCode))
            {
                return Error.Validation("Auth code cannot be empty");
            }

            if (string.IsNullOrWhiteSpace(codeVerifier))
            {
                return Error.Validation("Code verifier cannot be empty");
            }

            return Result.Success;
        }

        private async Task<ErrorOr<AuthCode>> GetAuthCode(string authCode)
        {
            var authCodeEntity = await _dbContext.AuthCodes
                .FirstOrDefaultAsync(ac => ac.Code == authCode);

            if (authCodeEntity is null)
            {
                return Error.NotFound("Auth code not found");
            }

            return authCodeEntity;
        }

        private bool IsCodeVerifierValid(string codeVerifier, string storedCodeChallenge)
        {
            var hashedVerifier = HashCodeVerifier(codeVerifier);
            return hashedVerifier == storedCodeChallenge;
        }

        private string HashCodeVerifier(string codeVerifier)
        {
            using SHA256 sha256 = SHA256.Create();
            string hashedVerifier = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier))).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return hashedVerifier;
        }

        private async Task<ErrorOr<Success>> DeleteAuthCodeAsync(string authCode)
        {
            try
            {
                var authCodeEntity = await _dbContext.AuthCodes
                    .FirstOrDefaultAsync(ac => ac.Code == authCode);

                if (authCodeEntity is null)
                {
                    return Error.NotFound("Auth code not found");
                }

                _dbContext.AuthCodes.Remove(authCodeEntity);
                await _dbContext.SaveChangesAsync();

                return Result.Success;
            }
            catch (DbUpdateException ex)
            {
                return Error.Failure("Failed to delete auth code", ex.Message);
            }
        }
    }
}