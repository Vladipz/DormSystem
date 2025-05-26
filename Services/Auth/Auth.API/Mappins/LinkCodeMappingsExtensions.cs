using Auth.API.Models;
using Auth.BLL.Models;

namespace Auth.API.Mappins
{
    public static class LinkCodeMappingsExtensions
    {
        /// <summary>
        /// Maps a BLL LinkCodeDto to an API LinkCodeResponse.
        /// </summary>
        /// <param name="dto">The BLL LinkCodeDto model.</param>
        /// <returns>An API LinkCodeResponse model.</returns>
        public static LinkCodeResponse ToResponse(this LinkCodeDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            return new LinkCodeResponse
            {
                Code = dto.Code,
                ExpiresAt = dto.ExpiresAt,
            };
        }

        /// <summary>
        /// Maps a ValidateLinkCodeRequest to a string code.
        /// </summary>
        /// <param name="request">The API ValidateLinkCodeRequest model.</param>
        /// <returns>The code string.</returns>
        public static string ToCode(this ValidateLinkCodeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return request.Code;
        }

        /// <summary>
        /// Maps a userId to a ValidateLinkCodeResponse.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>An API ValidateLinkCodeResponse model.</returns>
        public static ValidateLinkCodeResponse ToResponse(this Guid userId)
        {
            return new ValidateLinkCodeResponse
            {
                UserId = userId,
            };
        }
    }
}