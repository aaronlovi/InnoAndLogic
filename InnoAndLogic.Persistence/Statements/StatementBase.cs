using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for database statements that operate on a specific type of database connection.
/// </summary>
/// <typeparam name="TConnectionType">The type of the database connection.</typeparam>
/// <typeparam name="TParameterType">The type of the database parameter.</typeparam>
public abstract class StatementBase<TConnectionType, TParameterType>
    where TConnectionType : DbConnection
    where TParameterType : DbParameter {
    /// <summary>
    /// Executes the database statement using the specified connection.
    /// </summary>
    /// <param name="conn">The database connection to use for executing the statement.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the result of the statement execution.</returns>
    public abstract Task<DbStmtResult> Execute(TConnectionType conn, CancellationToken ct);

    /// <summary>
    /// Gets the collection of parameters bound to the database statement.
    /// </summary>
    /// <returns>A read-only collection of bound parameters.</returns>
    protected abstract IReadOnlyCollection<TParameterType> GetBoundParameters();
}
