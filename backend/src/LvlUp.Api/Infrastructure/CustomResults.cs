using LvlUp.SharedKernel;

namespace LvlUp.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot create a problem result from a success result.");
        }

        return Results.Problem(
            title: GetTitle(result.Error),
            detail: GetDetail(result.Error),
            type: GetProblemType(result.Error.Type),
            statusCode: GetStatusCode(result.Error.Type),
            extensions: GetErrors(result));
    }

    private static string GetTitle(Error error) => error.Type switch
    {
        ErrorType.Validation or
        ErrorType.Problem or
        ErrorType.NotFound or
        ErrorType.Conflict or
        ErrorType.Unauthorized => error.Code,
        _ => "Server failure",
    };

    private static string GetDetail(Error error) => error.Type switch
    {
        ErrorType.Validation or
        ErrorType.Problem or
        ErrorType.NotFound or
        ErrorType.Conflict or
        ErrorType.Unauthorized => error.Description,
        _ => "An unexpected error occurred.",
    };

    private static string GetProblemType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation or ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7235#section-3.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    };

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation or ErrorType.Problem => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        _ => StatusCodes.Status500InternalServerError,
    };

    private static Dictionary<string, object?>? GetErrors(Result result)
    {
        if (result.Error is not ValidationError validationError)
        {
            return null;
        }

        return new Dictionary<string, object?>
        {
            { "errors", validationError.Errors },
        };
    }
}
