using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared;
using InnoAndLogic.Shared.Models;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for batched non-query database statements executed against a database.
/// </summary>
public abstract class NonQueryBatchedDbStmtBase<TConnectionType, TParameterType, TBatchType, TBatchCommandType>
    (string _className) : StatementBase<TConnectionType, TParameterType>
    where TConnectionType : DbConnection
    where TParameterType : DbParameter
    where TBatchType : DbBatch
    where TBatchCommandType : DbBatchCommand {
    private readonly List<TBatchCommandType> _commands = [];

    /// <summary>
    /// Gets the number of rows affected by the most recent database operation.
    /// </summary>
    public int NumRowsAffected { get; protected set; }

    /// <summary>
    /// Creates a new batch for executing multiple commands in a single operation.
    /// </summary>
    /// <param name="conn">The database connection to associate with the batch.</param>
    /// <returns>A new instance of the batch.</returns>
    /// <remarks>
    /// This method is intended to be implemented by derived classes to provide a specific
    /// implementation of a database batch. The batch allows multiple commands to be executed
    /// together, improving performance by reducing the number of round-trips to the database.
    /// </remarks>
    public abstract TBatchType CreateBatch(TConnectionType conn);

    /// <summary>
    /// Creates a new batch command to be added to a batch.
    /// </summary>
    /// <param name="sql">The SQL command text for the batch command.</param>
    /// <returns>A new instance of the batch command.</returns>
    /// <remarks>
    /// This method is intended to be implemented by derived classes to provide a specific
    /// implementation of a batch command. The batch command represents a single SQL statement
    /// that can be executed as part of a batch.
    /// </remarks>
    public abstract TBatchCommandType CreateBatchCommand(string sql);

    /// <inheritdoc/>
    public override async Task<Result> Execute(TConnectionType conn, CancellationToken ct) {
        try {
            using TBatchType batch = CreateBatch(conn);
            foreach (TBatchCommandType cmd in _commands)
                batch.BatchCommands.Add(cmd);
            NumRowsAffected = await batch.ExecuteNonQueryAsync(ct);
            return Result.Success;
        //} catch (PostgresException ex) {
        //    string errMsg = $"{_className} failed - {ex.Message}";
        //    ErrorCodes failureReason = ex.SqlState == "23505" ? ErrorCodes.Duplicate : ErrorCodes.GenericError;
        //    return Result.Failure(failureReason, errMsg);
        } catch (Exception ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            return Result.Failure(ErrorCodes.GenericError, errMsg);
        }
    }

    /// <summary>
    /// Adds a command to the batch for execution.
    /// </summary>
    /// <param name="sql">The SQL command to add to the batch.</param>
    /// <param name="boundParams">The parameters to bind to the SQL command.</param>
    protected void AddCommandToBatch(string sql, IReadOnlyCollection<TParameterType> boundParams) {
        TBatchCommandType cmd = CreateBatchCommand(sql);
        foreach (TParameterType boundParam in boundParams)
            _ = cmd.Parameters.Add(boundParam);
        _commands.Add(cmd);
    }

    /// <inheritdoc/>
    protected override IReadOnlyCollection<TParameterType> GetBoundParameters() => throw new NotImplementedException();
}
