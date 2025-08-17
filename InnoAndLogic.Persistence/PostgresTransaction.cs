using Npgsql;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents a PostgreSQL transaction with additional resource management capabilities.
/// </summary>
public sealed class PostgresTransaction : TransactionBase<NpgsqlConnection, NpgsqlTransaction> {
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresTransaction"/> class.
    /// </summary>
    /// <param name="connection">The <see cref="NpgsqlConnection"/> associated with the transaction.</param>
    /// <param name="transaction">The <see cref="NpgsqlTransaction"/> to manage.</param>
    /// <param name="limiter">The <see cref="SemaphoreLocker"/> used to manage concurrent access.</param>
    public PostgresTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction, SemaphoreLocker limiter)
        : base(connection, transaction, limiter) {
    }
}
