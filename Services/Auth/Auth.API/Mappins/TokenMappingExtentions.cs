using Auth.API.Models.Responses;
using Auth.BLL.Models;

namespace Auth.API.Mappins
{
    public static class TokenMappingExtentions
    {
        public static TokenResponse ToResponse(this TokenModel tokenModel)
        {
            ArgumentNullException.ThrowIfNull(tokenModel);

            return new TokenResponse
            {
                AccessToken = tokenModel.AccessToken,
                RefreshToken = tokenModel.RefreshToken,
            };
        }
    }
}