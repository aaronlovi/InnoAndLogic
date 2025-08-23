using Npgsql;

namespace InnoAndLogic.Persistence.Statements.Postgres;

/// <summary>
/// Represents a PostgreSQL-specific implementation of the <see cref="NonQueryBatchedDbStmtBase{TConnectionType, TParameterType, TBatchType, TBatchCommandType}"/> class.
/// </summary>
/// <remarks>
/// This class provides a concrete implementation for PostgreSQL batched non-query operations,
/// using the <see cref="NpgsqlBatch"/> and <see cref="NpgsqlBatchCommand"/> classes to create database batches.
/// It is designed to simplify the execution of multiple non-query statements in a single batch operation
/// specifically for PostgreSQL databases, improving performance by reducing round-trips to the database.
/// </remarks>
public abstract class PostgresNonQueryBatchedDbStmtBase :
    NonQueryBatchedDbStmtBase<NpgsqlConnection, NpgsqlParameter, NpgsqlBatch, NpgsqlBatchCommand> {

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresNonQueryBatchedDbStmtBase"/> class.
    /// </summary>
    /// <param name="className">The name of the class implementing the batched non-query statement.</param>
    public PostgresNonQueryBatchedDbStmtBase(string className)
        : base(className) { }

    /// <inheritdoc/>
    public override NpgsqlBatch CreateBatch(NpgsqlConnection conn) => new(conn);

    /// <inheritdoc/>
    public override NpgsqlBatchCommand CreateBatchCommand(string sql) => new(sql);
}