using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared.Models;
using Npgsql;
using NpgsqlTypes;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Provides utility methods for working with Npgsql parameters.
/// </summary>
public static class DbUtils {
    /// <summary>
    /// Creates an Npgsql parameter for a nullable DateTimeOffset value.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="nullableDate">The nullable DateTimeOffset value.</param>
    /// <param name="dbType"></param>
    /// <returns>An NpgsqlParameter representing the nullable DateTimeOffset value.</returns>
    public static NpgsqlParameter CreateNullableDateTimeOffsetParam(
        string paramName, DateTimeOffset? nullableDate, NpgsqlDbType dbType = NpgsqlDbType.TimestampTz) {
        var param = new NpgsqlParameter(paramName, dbType) {
            Value = nullableDate?.UtcDateTime ?? (object)DBNull.Value
        };
        return param;
    }

    /// <summary>
    /// Creates an Npgsql parameter for a nullable DateTime value.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="nullableDate">The nullable DateTimeOffset value.</param>
    /// <param name="dbType">The NpgsqlDbType for the parameter, default is Timestamp.</param>
    /// <returns>An NpgsqlParameter representing the nullable DateTimeOffset value.</returns>
    public static NpgsqlParameter CreateNullableDateTimeParam(
        string paramName, DateTime? nullableDate, NpgsqlDbType dbType = NpgsqlDbType.Timestamp) {
        var param = new NpgsqlParameter(paramName, dbType) {
            Value = nullableDate ?? (object)DBNull.Value
        };
        return param;
    }
}

/// <summary>
/// Represents the base class for database statements that bind parameters.
/// </summary>
public abstract class DbStmtBase {
    /// <summary>
    /// Gets the collection of parameters bound to the database statement.
    /// </summary>
    /// <returns>A read-only collection of Npgsql parameters.</returns>
    protected abstract IReadOnlyCollection<NpgsqlParameter> GetBoundParameters();
}

/// <summary>
/// Represents the base class for all query database statements executed against a
/// PostgreSQL database using Npgsql. This abstract class provides the framework for
/// executing SQL queries, preparing commands, and processing result sets.
/// </summary>
/// <remarks>
/// Derived classes should implement the <see cref="ProcessCurrentRow"/> method to
/// define how individual rows in the result set are processed.
/// Additionally, they may override the <see cref="BeforeRowProcessing"/> method to
/// perform any necessary setup or initialization before row processing begins.
/// This class handles common tasks such as command preparation, parameter binding,
/// and execution of the reader loop, allowing derived classes to focus on the
/// specifics of their respective queries.
/// </remarks>
public abstract class QueryDbStmtBase(string _sql, string _className) : DbStmtBase, IPostgresStatement {
    /// <summary>
    /// Executes the SQL query defined in the derived class against the provided NpgsqlConnection.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation, containing the result of the query
    /// execution as a <see cref="DbStmtResult"/>.
    /// </returns>
    /// <remarks>
    /// This method prepares the SQL command, binds any parameters required for the query, and
    /// executes the command asynchronously. It iterates over the result set, processing each 
    /// row using the <see cref="ProcessCurrentRow"/> method implemented in the derived class.
    /// Before processing the rows, it calls <see cref="BeforeRowProcessing"/> to allow for
    /// any necessary setup.
    /// This method handles exceptions by clearing any results and returning a failure result,
    /// ensuring that the caller can gracefully handle errors.
    /// </remarks>
    public async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        ClearResults();

        try {
            using var cmd = new NpgsqlCommand(_sql, conn);
            foreach (NpgsqlParameter boundParam in GetBoundParameters())
                _ = cmd.Parameters.Add(boundParam);
            await cmd.PrepareAsync(ct);
            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(ct);

            BeforeRowProcessing(reader);

            int numRows = 0;
            while (await reader.ReadAsync(ct)) {
                ++numRows;
                if (!ProcessCurrentRow(reader))
                    break;
            }

            AfterLastRowProcessing();

            return DbStmtResult.StatementSuccess(numRows);
        } catch (Exception ex) {
            ClearResults();
            string errMsg = $"{_className} failed - {ex.Message}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }

    /// <summary>
    /// Clears any results or state from a previous query execution.
    /// </summary>
    /// <remarks>
    /// This method is designed to reset the state of the derived query statement class,
    /// ensuring that it is ready for a new execution cycle. It should be called before
    /// executing a new query to prevent data from previous executions from affecting the
    /// results of the current execution. Derived classes should override this method to
    /// clear specific results or state information related to their query.
    /// </remarks>
    protected abstract void ClearResults();

    /// <summary>
    /// Performs any necessary setup or initialization before processing the rows of the
    /// query result set.
    /// </summary>
    /// <remarks>
    /// This method is called once before the row processing loop begins in the
    /// <see cref="Execute"/> method.
    /// It provides a hook for derived classes to perform any setup tasks that are necessary
    /// before processing individual rows.
    /// Common uses include caching column ordinals for efficient access during row processing
    /// or initializing data structures to hold the results.
    /// Derived classes overriding this method should ensure to call the base method if it
    /// contains implementation.
    /// </remarks>
    protected virtual void BeforeRowProcessing(NpgsqlDataReader reader) { }

    /// <summary>
    /// Performs any necessary cleanup or finalization after processing all rows in the query result set.
    /// </summary>
    protected virtual void AfterLastRowProcessing() { }

    /// <summary>
    /// Processes the current row in the query result set.
    /// </summary>
    /// <param name="reader">The <see cref="NpgsqlDataReader"/> used to read the current row.</param>
    /// <returns>
    /// A boolean value indicating whether to continue processing rows.
    /// Returning false will stop the row processing loop.
    /// </returns>
    /// <remarks>
    /// This method is called for each row in the result set of the query execution.
    /// Derived classes must implement this method to define how individual rows
    /// should be processed.
    /// The method provides direct access to the current row through the
    /// <paramref name="reader"/> parameter, allowing derived classes to read the
    /// necessary data from the row.
    /// Implementations can return false to prematurely stop the processing of rows,
    /// which can be useful in scenarios where not all rows need to be processed or
    /// certain conditions are met.
    /// </remarks>
    protected abstract bool ProcessCurrentRow(NpgsqlDataReader reader);
}

/// <summary>
/// Represents the base class for non-query database statements executed against a PostgreSQL database.
/// </summary>
public abstract class NonQueryDbStmtBase(string _sql, string _className) : DbStmtBase, IPostgresStatement {
    /// <inheritdoc/>
    public async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        try {
            using var cmd = new NpgsqlCommand(_sql, conn);
            foreach (NpgsqlParameter boundParam in GetBoundParameters())
                _ = cmd.Parameters.Add(boundParam);
            await cmd.PrepareAsync(ct);
            int numRows = await cmd.ExecuteNonQueryAsync(ct);
            return DbStmtResult.StatementSuccess(numRows);
        } catch (Exception ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }
}

/// <summary>
/// Represents the base class for batched non-query database statements executed against a PostgreSQL database.
/// </summary>
public abstract class NonQueryBatchedDbStmtBase(string _className) : IPostgresStatement {
    private readonly List<NpgsqlBatchCommand> _commands = [];

    /// <inheritdoc/>
    public async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        try {
            using var batch = new NpgsqlBatch(conn);
            foreach (NpgsqlBatchCommand cmd in _commands)
                batch.BatchCommands.Add(cmd);
            int numRows = await batch.ExecuteNonQueryAsync(ct);
            return DbStmtResult.StatementSuccess(numRows);
        } catch (PostgresException ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            ErrorCodes failureReason = ex.SqlState == "23505" ? ErrorCodes.Duplicate : ErrorCodes.GenericError;
            return DbStmtResult.StatementFailure(failureReason, errMsg);
        } catch (Exception ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }

    /// <summary>
    /// Adds a command to the batch for execution.
    /// </summary>
    /// <param name="sql">The SQL command to add to the batch.</param>
    /// <param name="boundParams">The parameters to bind to the SQL command.</param>
    protected void AddCommandToBatch(string sql, IReadOnlyCollection<NpgsqlParameter> boundParams) {
        var cmd = new NpgsqlBatchCommand(sql);
        foreach (NpgsqlParameter boundParam in boundParams)
            _ = cmd.Parameters.Add(boundParam);
        _commands.Add(cmd);
    }
}

/// <summary>
/// Represents the base class for bulk insert database statements executed against a PostgreSQL database.
/// </summary>
/// <typeparam name="T">The type of the items to be inserted.</typeparam>
public abstract class BulkInsertDbStmtBase<T>(string _className, IReadOnlyCollection<T> _items)
    : IPostgresStatement
    where T : class {
    /// <summary>
    /// Gets the SQL COPY command used for the bulk insert operation.
    /// </summary>
    /// <returns>The SQL COPY command as a string.</returns>
    protected abstract string GetCopyCommand();

    /// <summary>
    /// Writes an individual item to the binary importer.
    /// </summary>
    /// <param name="writer">The <see cref="NpgsqlBinaryImporter"/> used to write the item.</param>
    /// <param name="item">The item to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    protected abstract Task WriteItemAsync(NpgsqlBinaryImporter writer, T item);

    /// <inheritdoc/>
    public async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        T? failedItem = default;

        try {
            using NpgsqlBinaryImporter writer = conn.BeginBinaryImport(GetCopyCommand());

            foreach (T item in _items) {
                failedItem = item;
                await writer.StartRowAsync(ct);
                await WriteItemAsync(writer, item);
            }

            _ = await writer.CompleteAsync(ct);
            return DbStmtResult.StatementSuccess(_items.Count);
        } catch (PostgresException ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            ErrorCodes failureReason = ex.SqlState == "23505"
                ? ErrorCodes.Duplicate
                : ErrorCodes.GenericError;
            return DbStmtResult.StatementFailure(failureReason, errMsg);
        } catch (Exception ex) {
            string failedItemStr = failedItem?.ToString() ?? "NULL";
            string errMsg = $"{_className} failed - {ex.Message}. Item: {failedItemStr}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }
}
