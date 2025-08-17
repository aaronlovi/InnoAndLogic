using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Persistence.Statements;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents the base class for executing database statements and managing transactions.
/// </summary>
/// <typeparam name="TConnectionType">The type of the database connection.</typeparam>
/// <typeparam name="TTransactionType">The type of the database transaction.</typeparam>
/// <typeparam name="TParameterType">The type of the database parameter</typeparam>
public abstract class StatementExecutorBase<TConnectionType, TTransactionType, TParameterType> : IDisposable
    where TConnectionType : DbConnection
    where TTransactionType : DbTransaction
    where TParameterType : DbParameter {
    /// <summary>
    /// Gets a value indicating whether the executor is enabled.
    /// </summary>
    public abstract bool IsEnabled { get; }

    /// <summary>
    /// Executes a query statement asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> ExecuteQuery(StatementBase<TConnectionType, TParameterType> stmt, CancellationToken ct);

    /// <summary>
    /// Executes a query statement with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <param name="transaction">Optional transaction to use for the execution.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> ExecuteQueryWithRetry(
        StatementBase<TConnectionType, TParameterType> stmt,
        CancellationToken ct,
        int? overrideMaxRetries = null,
        TransactionBase<TConnectionType, TTransactionType>? transaction = null);

    /// <summary>
    /// Executes a statement asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> Execute(StatementBase<TConnectionType, TParameterType> stmt, CancellationToken ct);

    /// <summary>
    /// Executes a statement within a transaction asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="transaction">The transaction to use for the execution.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> ExecuteUnderTransaction(
        StatementBase<TConnectionType, TParameterType> stmt,
        TransactionBase<TConnectionType, TTransactionType> transaction,
        CancellationToken ct);

    /// <summary>
    /// Executes a statement with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <param name="transaction">Optional transaction to use for the execution.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> ExecuteWithRetry(
        StatementBase<TConnectionType, TParameterType> stmt,
        CancellationToken ct,
        int? overrideMaxRetries = null,
        TransactionBase<TConnectionType, TTransactionType>? transaction = null);

    /// <summary>
    /// Executes a statement within a transaction with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="transaction">The transaction to use for the execution.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <returns>The result of the statement execution.</returns>
    public abstract Task<DbStmtResult> ExecuteUnderTransactionWithRetry(
        StatementBase<TConnectionType, TParameterType> stmt,
        TransactionBase<TConnectionType, TTransactionType> transaction,
        CancellationToken ct,
        int? overrideMaxRetries = null);

    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The created transaction.</returns>
    public abstract Task<TransactionBase<TConnectionType, TTransactionType>> BeginTransaction(CancellationToken ct);

    /// <summary>
    /// Releases the resources used by the executor.
    /// </summary>
    public abstract void Dispose();
}
