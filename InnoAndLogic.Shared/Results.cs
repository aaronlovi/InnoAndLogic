using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using InnoAndLogic.Shared.Models;

namespace InnoAndLogic.Shared;

/// <summary>
/// Represents the result of an operation, containing error information if unsuccessful.
/// </summary>
public interface IResult {
    /// <summary>
    /// Gets the error code associated with the result.
    /// </summary>
    ErrorCodes ErrorCode { get; }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    bool IsFailure { get; }

    /// <summary>
    /// Gets the error message associated with the result.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// Gets additional parameters providing context for the error.
    /// </summary>
    IReadOnlyCollection<string> ErrorParams { get; }
}

/// <summary>
/// Represents a non-generic result of an operation, with error information if unsuccessful.
/// </summary>
public record Result : IResult {
    /// <summary>
    /// A static instance representing a successful result.
    /// </summary>
    public static readonly Result Success = new(ErrorCodes.None);

    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="errorCode">The error code representing the result of the operation.</param>
    /// <param name="errorMessage">The error message, if any, associated with the operation.</param>
    /// <param name="errorParams">Optional parameters providing additional context for the error.</param>
    public Result(ErrorCodes errorCode, string errorMessage = "", IReadOnlyCollection<string>? errorParams = null) {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorParams = errorParams ?? [];
    }

    /// <inheritdoc/>
    [JsonPropertyName("errorCode")] public ErrorCodes ErrorCode { get; init; }

    /// <inheritdoc/>
    [JsonPropertyName("errorMessage")] public string ErrorMessage { get; init; }

    /// <inheritdoc/>
    [JsonPropertyName("errorParams")] public IReadOnlyCollection<string> ErrorParams { get; init; }

    /// <inheritdoc/>
    [JsonIgnore] public bool IsSuccess => ErrorCode is ErrorCodes.None;

    /// <inheritdoc/>
    [JsonIgnore] public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code representing the failure reason.</param>
    /// <param name="errMsg">The error message describing the failure.</param>
    /// <param name="errorParams">Optional parameters providing additional context for the error.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(ErrorCodes errorCode, string errMsg = "", params string[] errorParams)
        => new(errorCode, errMsg, errorParams.Length == 0 ? null : errorParams);

    /// <summary>
    /// Creates a failed result based on an existing result.
    /// </summary>
    /// <param name="res">The existing result to base the failure on.</param>
    /// <returns>A failed <see cref="Result"/>.</returns>
    public static Result Failure(IResult res) => new(res.ErrorCode, res.ErrorMessage, res.ErrorParams);
}

/// <summary>
/// Represents a generic result of an operation, containing a value if successful, or error information if unsuccessful.
/// </summary>
/// <typeparam name="T">The type of the value contained in the result.</typeparam>
public record Result<T> : IResult {
    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="errorCode">The error code representing the result of the operation.</param>
    /// <param name="errorMessage">The error message, if any, associated with the operation.</param>
    /// <param name="errorParams">Optional parameters providing additional context for the error.</param>
    /// <param name="value">The value contained in the result, if successful.</param>
    public Result(ErrorCodes errorCode, string errorMessage = "", IReadOnlyCollection<string>? errorParams = null, T? value = default) {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorParams = errorParams ?? [];
        Value = value;
    }

    /// <inheritdoc/>
    [JsonPropertyName("errorCode")] public ErrorCodes ErrorCode { get; init; }

    /// <inheritdoc/>
    [JsonPropertyName("errorMessage")] public string ErrorMessage { get; init; }

    /// <inheritdoc/>
    [JsonPropertyName("errorParams")] public IReadOnlyCollection<string> ErrorParams { get; init; }

    /// <summary>
    /// Gets the value contained in the result, if successful.
    /// </summary>
    [JsonPropertyName("value")] public T? Value { get; init; }

    /// <inheritdoc/>
    [JsonIgnore] public bool IsSuccess => ErrorCode is ErrorCodes.None;

    /// <inheritdoc/>
    [JsonIgnore] public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The value contained in the result.</param>
    /// <returns>A successful <see cref="Result{T}"/>.</returns>
    public static Result<T> Success(T value) => new(ErrorCodes.None, "", null, value);

    /// <summary>
    /// Creates a failed result with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code representing the failure reason.</param>
    /// <param name="errMsg">The error message describing the failure.</param>
    /// <param name="errorParams">Optional parameters providing additional context for the error.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure(ErrorCodes errorCode, string errMsg = "", params string[] errorParams)
        => new(errorCode, errMsg, errorParams.Length == 0 ? null : errorParams);

    /// <summary>
    /// Creates a failed result based on an existing result.
    /// </summary>
    /// <param name="res">The existing result to base the failure on.</param>
    /// <returns>A failed <see cref="Result{T}"/>.</returns>
    public static Result<T> Failure(IResult res) => new(res.ErrorCode, res.ErrorMessage, res.ErrorParams);
}

/// <summary>
/// Provides extension methods for working with <see cref="Result"/> and <see cref="Result{T}"/>.
/// </summary>
public static class ResultFluentExtensions {
    /// <summary>
    /// Chains the execution of a function if the result is successful.
    /// </summary>
    /// <param name="result">The initial result.</param>
    /// <param name="fn">The function to execute if the result is successful.</param>
    /// <returns>The result of the function, or the initial result if it is a failure.</returns>
    public static Result Then(this Result result, Func<Result> fn) => ThenCore(result, fn);

    /// <summary>
    /// Chains the execution of a function if the result is successful.
    /// </summary>
    /// <param name="result">The initial result.</param>
    /// <param name="fn">The function to execute if the result is successful.</param>
    /// <returns>The result of the function, or the initial result if it is a failure.</returns>
    public static Result Then(this Result result, Func<Result, Result> fn) => ThenCore(result, fn);

    /// <summary>
    /// Chains the execution of an asynchronous function if the result is successful.
    /// </summary>
    /// <param name="result">The initial result.</param>
    /// <param name="fn">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static Task<Result> Then(this Result result, Func<Task<Result>> fn) => ThenCoreAsync(result, fn);

    /// <summary>
    /// Chains the execution of an asynchronous function if the result is successful.
    /// </summary>
    /// <param name="result">The initial result.</param>
    /// <param name="fn">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static Task<Result> Then(this Result result, Func<Result, Task<Result>> fn) => ThenCoreAsync(result, fn);

    /// <summary>
    /// Chains the execution of a function if the result is successful.
    /// </summary>
    /// <param name="resultAsTask">A task representing the initial result.</param>
    /// <param name="fn">The function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static async Task<Result> Then(this Task<Result> resultAsTask, Func<Result> fn) => ThenCore(await resultAsTask, fn);

    /// <summary>
    /// Chains the execution of a function if the result is successful.
    /// </summary>
    /// <param name="resultAsTask">A task representing the initial result.</param>
    /// <param name="fn">The function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static async Task<Result> Then(this Task<Result> resultAsTask, Func<Result, Result> fn) => ThenCore(await resultAsTask, fn);

    /// <summary>
    /// Chains the execution of an asynchronous function if the result is successful.
    /// </summary>
    /// <param name="resultAsTask">A task representing the initial result.</param>
    /// <param name="fn">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static async Task<Result> Then(this Task<Result> resultAsTask, Func<Task<Result>> fn) => await ThenCoreAsync(await resultAsTask, fn);

    /// <summary>
    /// Chains the execution of an asynchronous function if the result is successful.
    /// </summary>
    /// <param name="resultAsTask">A task representing the initial result.</param>
    /// <param name="fn">The asynchronous function to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the result of the function or the initial result if it is a failure.</returns>
    public static async Task<Result> Then(this Task<Result> resultAsTask, Func<Result, Task<Result>> fn) => await ThenCoreAsync(await resultAsTask, fn);

    /// <summary>
    /// Executes actions based on the result's success or failure.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>The original result.</returns>
    public static Result OnCompletion(this Result result, Action<Result>? onSuccess = null, Action<Result>? onFailure = null)
        => OnCompletionCore(result, onSuccess, onFailure);

    /// <summary>
    /// Executes actions based on the result's success or failure.
    /// </summary>
    /// <param name="resultAsTask">A task representing the result to evaluate.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>A task representing the asynchronous operation, containing the original result.</returns>
    public static async Task<Result> OnCompletion(this Task<Result> resultAsTask, Action<Result>? onSuccess = null, Action<Result>? onFailure = null)
        => OnCompletionCore(await resultAsTask, onSuccess, onFailure);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <returns>The original result.</returns>
    public static Result OnSuccess(this Result result, Action<Result> onSuccess) => OnCompletion(result, onSuccess, null);

    /// <summary>
    /// Executes an action if the result is successful.
    /// </summary>
    /// <param name="resultAsTask">A task representing the result to evaluate.</param>
    /// <param name="onSuccess">The action to execute if the result is successful.</param>
    /// <returns>A task representing the asynchronous operation, containing the original result.</returns>
    public static Task<Result> OnSuccess(this Task<Result> resultAsTask, Action<Result> onSuccess) => OnCompletion(resultAsTask, onSuccess, null);

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>The original result.</returns>
    public static Result OnFailure(this Result result, Action<Result> onFailure) => OnCompletion(result, null, onFailure);

    /// <summary>
    /// Executes an action if the result is a failure.
    /// </summary>
    /// <param name="resultAsTask">A task representing the result to evaluate.</param>
    /// <param name="onFailure">The action to execute if the result is a failure.</param>
    /// <returns>A task representing the asynchronous operation, containing the original result.</returns>
    public static Task<Result> OnFailure(this Task<Result> resultAsTask, Action<Result> onFailure) => OnCompletion(resultAsTask, null, onFailure);

    #region PRIVATE HELPER METHODS

    private static Result ThenCore(Result result, Func<Result> fn) {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fn);
        return result.IsSuccess ? fn() : result;
    }

    private static Result ThenCore(Result result, Func<Result, Result> fn) {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fn);
        return result.IsSuccess ? fn(result) : result;
    }

    private static async Task<Result> ThenCoreAsync(Result result, Func<Task<Result>> fn) {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fn);
        return result.IsSuccess ? await fn() : result;
    }

    private static async Task<Result> ThenCoreAsync(Result result, Func<Result, Task<Result>> fn) {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(fn);
        return result.IsSuccess ? await fn(result) : result;
    }

    private static Result OnCompletionCore(Result result, Action<Result>? onSuccess, Action<Result>? onFailure) {
        ArgumentNullException.ThrowIfNull(result);
        if (result.IsSuccess)
            onSuccess?.Invoke(result);
        else
            onFailure?.Invoke(result);
        return result;
    }

    #endregion
}
