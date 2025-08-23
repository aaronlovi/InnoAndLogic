using Npgsql;

namespace InnoAndLogic.Persistence.Statements.Postgres;

/// <summary>
/// Represents the base class for PostgreSQL non-query database statements.
/// </summary>
/// <remarks>This abstract class provides a foundation for executing non-query SQL commands against a PostgreSQL
/// database using the Npgsql library. Derived classes should implement specific non-query operations, such as inserts,
/// updates, or deletes.</remarks>
public abstract class PostgresNonQueryDbStmtBase : NonQueryDbStmtBase<NpgsqlConnection, NpgsqlParameter, NpgsqlCommand> {
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresNonQueryDbStmtBase"/> class.
    /// </summary>
    /// <param name="sql">The SQL command to be executed.</param>
    /// <param name="className">The name of the class implementing the non-query statement.</param>
    public PostgresNonQueryDbStmtBase(string sql, string className)
        : base(sql, className) { }

    /// <inheritdoc/>
    protected override NpgsqlCommand CreateCommand(string sql, NpgsqlConnection conn) => new(sql, conn);
}
