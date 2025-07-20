using System;
using Npgsql;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents a PostgreSQL transaction with additional resource management capabilities.
/// </summary>
public sealed class PostgresTransaction : IDisposable {
    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresTransaction"/> class.
    /// </summary>
    /// <param name="connection">The <see cref="NpgsqlConnection"/> associated with the transaction.</param>
    /// <param name="transaction">The <see cref="NpgsqlTransaction"/> to manage.</param>
    /// <param name="limiter">The <see cref="SemaphoreLocker"/> used to manage concurrent access.</param>
    public PostgresTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction, SemaphoreLocker limiter) {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        Limiter = limiter ?? throw new ArgumentNullException(nameof(limiter));
    }

    /// <summary>
    /// Gets the <see cref="NpgsqlConnection"/> associated with the transaction.
    /// </summary>
    public NpgsqlConnection Connection { get; }

    /// <summary>
    /// Gets the <see cref="NpgsqlTransaction"/> being managed.
    /// </summary>
    public NpgsqlTransaction Transaction { get; }

    /// <summary>
    /// Gets the <see cref="SemaphoreLocker"/> used to manage concurrent access.
    /// </summary>
    public SemaphoreLocker Limiter { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    public void Commit() => Transaction.Commit();

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    public void Rollback() => Transaction.Rollback();

    /// <summary>
    /// Releases the resources used by the transaction.
    /// </summary>
    public void Dispose() {
        Transaction.Dispose();
        Connection.Dispose();
        Limiter.Dispose();
    }
}
