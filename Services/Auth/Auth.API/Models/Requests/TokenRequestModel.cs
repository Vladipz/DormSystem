namespace Auth.API.Models.Requests
{
    public record TokenRequestModel(string AuthCode, string CodeVerifier);
}