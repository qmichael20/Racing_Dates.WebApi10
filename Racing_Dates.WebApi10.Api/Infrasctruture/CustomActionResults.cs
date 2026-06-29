using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace Racing_Dates.WebApi10.Infrasctruture
{
    public static class CustomActionResults
    {
        public static IActionResult Problem(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException();
            }

            return new ObjectResult(
                new ProblemDetails
                {
                    Title = GetTitle(result.Error),
                    Detail = GetDetail(result.Error),
                    Type = GetType(result.Error.Type),
                    Status = GetStatusCode(result.Error.Type),
                    Extensions = GetErrors(result)
                }
            );

            static string GetTitle(Error error) =>
                error.Type switch
                {
                    ErrorType.Validation => error.Code,
                    ErrorType.Problem => error.Code,
                    ErrorType.NotFound => error.Code,
                    ErrorType.Conflict => error.Code,
                    _ => "Server failure"
                };

            static string GetDetail(Error error) =>
                error.Type switch
                {
                    ErrorType.Validation => error.Description,
                    ErrorType.Problem => error.Description,
                    ErrorType.NotFound => error.Description,
                    ErrorType.Conflict => error.Description,
                    _ => "An unexpected error occurred"
                };

            static string GetType(ErrorType errorType) =>
                errorType switch
                {
                    ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                    _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };

            static int GetStatusCode(ErrorType errorType) =>
                errorType switch
                {
                    ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
                    ErrorType.NotFound => StatusCodes.Status404NotFound,
                    ErrorType.Conflict => StatusCodes.Status409Conflict,
                    _ => StatusCodes.Status500InternalServerError
                };

            static Dictionary<string, object?>? GetErrors(Result result)
            {
                if (result.Error is not ValidationError validationError)
                {
                    return null;
                }

                return new Dictionary<string, object?>
                {
                    { "errors", validationError.Errors }
                };
            }
        }
    }
}
