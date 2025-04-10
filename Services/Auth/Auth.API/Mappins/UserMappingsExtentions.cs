using Auth.API.Models.Requests;
using Auth.BLL.Models;

namespace Auth.API.Mappins
{
    public static class UserMappingsExtentions
    {
        /// <summary>
        /// Maps an API RegisterRequest to a BLL RegisterUserRequest.
        /// </summary>
        /// <param name="request">The API registration request model.</param>
        /// <returns>A BLL RegisterUserRequest model.</returns>
        public static RegisterUserRequest ToModel(this RegisterRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new RegisterUserRequest
            {
                Email = request.Email,
                Password = request.Password,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
        }
    }
}