using System;
using System.Data.Common;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents a base class for database transactions, providing common functionality for managing transactions and resources.
/// </summary>
/// <typeparam name="TConnectionType">The type of the database connection.</typeparam>
/// <typeparam name="TTransactionType">The type of the database transaction.</typeparam>
public abstract class TransactionBase<TConnectionType, TTransactionType> : IDisposable
    where TConnectionType : DbConnection
    where TTransactionType : DbTransaction {
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionBase{TConnectionType, TTransactionType}"/> class.
    /// </summary>
    /// <param name="connection">The database connection associated with the transaction.</param>
    /// <param name="transaction">The database transaction to manage.</param>
    /// <param name="limiter">The <see cref="SemaphoreLocker"/> used to manage concurrent access.</param>
    /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
    protected TransactionBase(TConnectionType connection, TTransactionType transaction, SemaphoreLocker limiter) {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        Limiter = limiter ?? throw new ArgumentNullException(nameof(limiter));
    }

    /// <summary>
    /// Gets the database connection associated with the transaction.
    /// </summary>
    public TConnectionType Connection { get; }

    /// <summary>
    /// Gets the database transaction being managed.
    /// </summary>
    public TTransactionType Transaction { get; }

    /// <summary>
    /// Gets the <see cref="SemaphoreLocker"/> used to manage concurrent access.
    /// </summary>
    public SemaphoreLocker Limiter { get; }

    /// <summary>
    /// Commits the transaction.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the transaction has already been disposed.</exception>
    public void Commit() {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Transaction.Commit();
    }

    /// <summary>
    /// Rolls back the transaction.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the transaction has already been disposed.</exception>
    public void Rollback() {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Transaction.Rollback();
    }

    /// <summary>
    /// Releases the resources used by the transaction.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged and optionally managed resources used by the transaction.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing) {
        if (_disposed)
            return;

        if (disposing) {
            // Dispose managed resources
            Transaction?.Dispose();
            Limiter?.Dispose();
        }

        // Release unmanaged resources (if any)

        _disposed = true;
    }

    /// <summary>
    /// Finalizer to ensure resources are released if Dispose is not called.
    /// </summary>
    ~TransactionBase() {
        Dispose(false);
    }
}
