using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Tutorial9.Model;

public enum ErrorType
{
    NotFound,
    Validation,
    Conflict,
    Internal
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public ErrorType? ErrorType { get; }
    public string? ErrorMessage { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, T? value, ErrorType? errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result<T> Success(T value) =>
        new Result<T>(true, value, null, null);

    public static Result<T> Failure(ErrorType errorType, string errorMessage) =>
        new Result<T>(false, default, errorType, errorMessage);
}


public class Result
{
    public bool IsSuccess { get; }
    public ErrorType? ErrorType { get; }
    public string? ErrorMessage { get; }

    public bool IsFailure => !IsSuccess;

    private Result(bool isSuccess, ErrorType? errorType, string? errorMessage)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(ErrorType errorType, string message) => new(false, errorType, message);

}

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(result.ErrorMessage),
            ErrorType.Validation => new BadRequestObjectResult(result.ErrorMessage),
            ErrorType.Conflict => new ConflictObjectResult(result.ErrorMessage),
            _ => new ObjectResult(result.ErrorMessage)
            {
                StatusCode = 500
            }
        };
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkResult();
        }

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(result.ErrorMessage),
            ErrorType.Validation => new BadRequestObjectResult(result.ErrorMessage),
            ErrorType.Conflict => new ConflictObjectResult(result.ErrorMessage),
            _ => new ObjectResult(result.ErrorMessage)
            {
                StatusCode = 500
            }
        };
    }
}
