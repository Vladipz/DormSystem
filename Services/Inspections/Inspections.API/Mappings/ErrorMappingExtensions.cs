using ErrorOr;

using Microsoft.AspNetCore.Mvc;

namespace Inspections.API.Mappings
{
    public static class ErrorMappingExtensions
    {
        public static IResult ToResponse(this Error error)
        {
            return error.Type switch
            {
                ErrorType.Validation => Results.BadRequest(error.Description),
                ErrorType.Unauthorized => Results.Unauthorized(),
                ErrorType.Forbidden => Results.Forbid(),
                ErrorType.NotFound => Results.NotFound(error.Description ?? string.Empty),
                ErrorType.Conflict => Results.Conflict(error.Description),
                ErrorType.Failure or ErrorType.Unexpected or _ => Results.Problem(statusCode: 500),
            };
        }

        public static IResult ToResponse(this List<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            return errors.All(e => e.Type == ErrorType.Validation)
                ? errors.ToValidationResponse()
                : errors[0].ToResponse();
        }

        public static IResult ToValidationResponse(this List<Error> errors)
        {
            Dictionary<string, string[]> dictionary = errors.GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());
            return Results.BadRequest(new ValidationProblemDetails(dictionary));
        }
    }
}