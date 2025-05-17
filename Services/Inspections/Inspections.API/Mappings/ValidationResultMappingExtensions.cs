using ErrorOr;
using FluentValidation.Results;

namespace Inspections.API.Mappings
{
    public static class ValidationResultMappingExtensions
    {
        public static ErrorOr<TResult> ToValidationError<TResult>(this ValidationResult validationResult)
        {
            ArgumentNullException.ThrowIfNull(validationResult);

            return validationResult.Errors.ConvertAll(vf => Error.Validation(vf.PropertyName, vf.ErrorMessage));
        }
    }
}