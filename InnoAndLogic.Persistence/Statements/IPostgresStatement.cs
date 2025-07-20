using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents a PostgreSQL statement that can be executed against a database.
/// </summary>
public interface IPostgresStatement {
    /// <summary>
    /// Executes the PostgreSQL statement using the provided connection and cancellation token.
    /// </summary>
    /// <param name="conn">The <see cref="NpgsqlConnection"/> to use for executing the statement.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result of the statement execution as a <see cref="DbStmtResult"/>.</returns>
    Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct);
}
