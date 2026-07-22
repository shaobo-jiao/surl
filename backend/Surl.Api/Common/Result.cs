namespace Surl.Api.Common;

/// <summary>
/// Represents a result from service layer. 
/// The result may contain the expected value or error if any.
/// A bool indicator IsSuccessful may be used to determine the presence of value or error.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TError"></typeparam>
public sealed class Result<TValue, TError>(TValue? value, TError? error, bool isSuccessful)
{
    public TValue? Value { get; } = value;
    public TError? Error { get; } = error;
    public bool IsSuccessful { get; } = isSuccessful;

    /// <summary>
    /// Returns a success result.
    /// </summary>
    public static Result<TValue, TError> Success(TValue value)
    {
        return new Result<TValue, TError>(value, default, true);
    }

    /// <summary>
    /// Returns a failure result.
    /// </summary>
    public static Result<TValue, TError> Failure(TError error)
    {
        return new Result<TValue, TError>(default, error, false);
    }
}

public sealed class Result
{
    
    public static Result<TValue, TError> Failure<TValue, TError>(TError error)
    {
        return new Result<TValue, TError>(default, error, false);
    }

    
    public static Result<TValue, object> Success<TValue>(TValue value)
    {
        return new Result<TValue, object>(value, default, true);
    }
}