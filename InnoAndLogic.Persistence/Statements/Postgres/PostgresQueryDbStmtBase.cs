using Npgsql;

namespace InnoAndLogic.Persistence.Statements.Postgres;

/// <summary>
/// Represents a PostgreSQL-specific implementation of the <see cref="QueryDbStmtBase{TConnectionType, TParameterType, TCommandType}"/> class.
/// </summary>
/// <remarks>
/// This class provides a concrete implementation of the <see cref="CreateCommand"/> method for PostgreSQL,
/// using the <see cref="NpgsqlCommand"/> class to create database commands.
/// It is designed to simplify the creation of query statements specifically for PostgreSQL databases.
/// </remarks>
public abstract class PostgresQueryDbStmtBase :
    QueryDbStmtBase<NpgsqlConnection, NpgsqlParameter, NpgsqlCommand> {

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresQueryDbStmtBase"/> class.
    /// </summary>
    /// <param name="sql">The SQL query to be executed.</param>
    /// <param name="className">The name of the class implementing the query statement.</param>
    public PostgresQueryDbStmtBase(string sql, string className)
        : base(sql, className) { }

    /// <inheritdoc/>
    protected override NpgsqlCommand CreateCommand(string sql, NpgsqlConnection conn) => new(sql, conn);
}
