using InnoAndLogic.Shared;
using InnoAndLogic.Shared.Models;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents the result of a database statement execution.
/// </summary>
public record DbStmtResult : Result {
    private DbStmtResult(int numRows, ErrorCodes errorCode, string errMsg = "")
        : base(errorCode, errMsg, null) {
        NumRows = numRows;
    }

    /// <summary>
    /// Gets the number of rows affected by the statement execution.
    /// </summary>
    public int NumRows { get; init; }

    /// <summary>
    /// Creates a successful <see cref="DbStmtResult"/> with the specified number of rows affected.
    /// </summary>
    /// <param name="numRows">The number of rows affected by the statement execution.</param>
    /// <returns>A successful <see cref="DbStmtResult"/>.</returns>
    public static DbStmtResult StatementSuccess(int numRows) => new(numRows, ErrorCodes.None, string.Empty);

    /// <summary>
    /// Creates a failed <see cref="DbStmtResult"/> with the specified error code and message.
    /// </summary>
    /// <param name="errorCode">The error code representing the failure reason.</param>
    /// <param name="errMsg">The error message describing the failure.</param>
    /// <returns>A failed <see cref="DbStmtResult"/>.</returns>
    public static DbStmtResult StatementFailure(ErrorCodes errorCode, string errMsg) => new(0, errorCode, errMsg);
}