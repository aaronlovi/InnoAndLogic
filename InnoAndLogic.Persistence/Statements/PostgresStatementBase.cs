using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents a PostgreSQL statement that can be executed against a database.
/// </summary>
public abstract class PostgresStatementBase : StatementBase<NpgsqlConnection, NpgsqlParameter> { }
