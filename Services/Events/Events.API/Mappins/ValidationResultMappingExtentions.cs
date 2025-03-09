using ErrorOr;

using FluentValidation.Results;

namespace Events.API.Mappins
{
    public static class ValidationResultMappingExtentions
    {
        public static ErrorOr<TResult> ToValidationError<TResult>(this ValidationResult validationResult)
        {
            ArgumentNullException.ThrowIfNull(validationResult);

            return validationResult.Errors.ConvertAll(vf => Error.Validation(vf.PropertyName, vf.ErrorMessage));
        }
    }
}