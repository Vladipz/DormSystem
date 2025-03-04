namespace Auth.API.Models.Requests
{
    public record AuthRequestModel(string Email, string Password, string CodeChallenge);
}