using System;
using Npgsql;

namespace InnoAndLogic.Persistence;

public sealed class PostgresTransaction : IDisposable {
    public PostgresTransaction(NpgsqlConnection connection, NpgsqlTransaction transaction, SemaphoreLocker limiter) {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        Limiter = limiter ?? throw new ArgumentNullException(nameof(limiter));
    }

    public NpgsqlConnection Connection { get; }
    public NpgsqlTransaction Transaction { get; }
    public SemaphoreLocker Limiter { get; }

    public void Commit() => Transaction.Commit();

    public void Rollback() => Transaction.Rollback();

    public void Dispose() {
        Transaction.Dispose();
        Connection.Dispose();
        Limiter.Dispose();
    }
}
