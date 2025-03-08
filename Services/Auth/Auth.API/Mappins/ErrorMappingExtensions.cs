using ErrorOr;

using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Mappins
{
    public static class ErrorMappingExtensions
    {
        public static IActionResult ToResponse(this Error error)
        {
            return error.Type switch
            {
                ErrorType.Validation => new BadRequestObjectResult(error.Description),
                ErrorType.Unauthorized => new UnauthorizedResult(),
                ErrorType.Forbidden => new ForbidResult(),
                ErrorType.NotFound => new NotFoundResult(),
                ErrorType.Conflict => new ConflictObjectResult(error.Description),
                ErrorType.Failure or ErrorType.Unexpected or _ => new ObjectResult(new ProblemDetails()) { StatusCode = 500 },
            };
        }

        public static IActionResult ToResponse(this List<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            return errors.All(static e => e.Type == ErrorType.Validation)
                ? errors.ToValidationResponse()
                : errors[0].ToResponse();
        }

        public static IActionResult ToValidationResponse(this List<Error> errors)
        {
            Dictionary<string, string[]> dictionary = errors.GroupBy(static e => e.Code)
                .ToDictionary(static g => g.Key, static g => g.Select(static e => e.Description).ToArray());
            return new BadRequestObjectResult(new ValidationProblemDetails(dictionary));
        }
    }
}