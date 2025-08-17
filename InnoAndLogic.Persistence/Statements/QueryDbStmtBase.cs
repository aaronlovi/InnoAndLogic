using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared.Models;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for all query database statements executed against a
/// database. This abstract class provides the framework for
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
public abstract class QueryDbStmtBase<TConnectionType, TParameterType, TCommandType>
    (string _sql, string _className) : StatementBase<TConnectionType, TParameterType>
    where TConnectionType : DbConnection
    where TParameterType : DbParameter
    where TCommandType : DbCommand {
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
    public override async Task<DbStmtResult> Execute(TConnectionType conn, CancellationToken ct) {
        ClearResults();

        try {
            using TCommandType cmd = CreateCommand(_sql, conn);
            foreach (TParameterType boundParam in GetBoundParameters())
                _ = cmd.Parameters.Add(boundParam);
            await cmd.PrepareAsync(ct);
            using DbDataReader reader = await cmd.ExecuteReaderAsync(ct);

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
    protected virtual void BeforeRowProcessing(DbDataReader reader) { }

    /// <summary>
    /// Performs any necessary cleanup or finalization after processing all rows in the query result set.
    /// </summary>
    protected virtual void AfterLastRowProcessing() { }

    /// <summary>
    /// Processes the current row in the query result set.
    /// </summary>
    /// <param name="reader">The <see cref="DbDataReader"/> used to read the current row.</param>
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
    protected abstract bool ProcessCurrentRow(DbDataReader reader);
}
