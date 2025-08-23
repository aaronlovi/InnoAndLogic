using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared;
using InnoAndLogic.Shared.Models;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for non-query database statements executed against a PostgreSQL database.
/// </summary>
public abstract class NonQueryDbStmtBase<TConnectionType, TParameterType, TCommandType>
    (string _sql, string _className)
    : StatementBase<TConnectionType, TParameterType>
    where TConnectionType : DbConnection
    where TParameterType : DbParameter
    where TCommandType : DbCommand {

    /// <summary>
    /// Gets the number of rows affected by the most recent database operation.
    /// </summary>
    public int NumRowsAffected { get; protected set; }

    /// <summary>
    /// Creates a new database command for executing a SQL statement.
    /// </summary>
    /// <param name="sql">The SQL statement to be executed by the command.</param>
    /// <param name="conn">The database connection to associate with the command.</param>
    /// <returns>A new instance of the database command.</returns>
    /// <remarks>
    /// This method is intended to be implemented by derived classes to provide a specific
    /// implementation of a database command. The command represents a single SQL statement
    /// that can be executed against the database.
    /// </remarks>
    protected abstract TCommandType CreateCommand(string sql, TConnectionType conn);

    /// <summary>
    /// Executes the SQL non-query statement defined in the derived class against the provided connection.
    /// </summary>
    /// <returns>
    /// A Task that represents the asynchronous operation, containing the result of the statement
    /// execution as a <see cref="Result"/>.
    /// </returns>
    /// <remarks>
    /// This method prepares the SQL command, binds any parameters required for the query, and
    /// executes the command asynchronously.
    /// This method handles exceptions by returning a failure result,
    /// ensuring that the caller can gracefully handle errors.
    /// </remarks>
    public override async Task<Result> Execute(TConnectionType conn, CancellationToken ct) {
        try {
            using TCommandType cmd = CreateCommand(_sql, conn);
            foreach (TParameterType boundParam in GetBoundParameters())
                _ = cmd.Parameters.Add(boundParam);
            await cmd.PrepareAsync(ct);
            NumRowsAffected = await cmd.ExecuteNonQueryAsync(ct);
            return Result.Success;
        } catch (Exception ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            return Result.Failure(ErrorCodes.GenericError, errMsg);
        }
    }
}
