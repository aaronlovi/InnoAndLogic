﻿using System;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Persistence.Statements;
using InnoAndLogic.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Executes PostgreSQL statements and manages database connections.
/// </summary>
public class PostgresExecutor : IDisposable {
    private readonly ILogger<PostgresExecutor> _logger;
    private readonly int _maxRetries;
    private readonly int _retryDelayMilliseconds;
    private readonly int _maxConcurrentStatements;
    private readonly int _maxConcurrentReadStatements;
    private readonly string _connectionString;
    private readonly SemaphoreSlim _connectionLimiter;
    private readonly SemaphoreSlim _readConnectionLimiter;
    private bool _disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresExecutor"/> class.
    /// </summary>
    /// <param name="dbOptions">The database options containing configuration settings.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for the executor.</param>
    public PostgresExecutor(IOptions<DatabaseOptions> dbOptions, ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<PostgresExecutor>();
        _logger.LogInformation("Persistence is enabled");

        DatabaseOptions options = dbOptions.Value;
        _maxRetries = options.MaxRetries;
        _retryDelayMilliseconds = options.RetryDelayMilliseconds;
        _maxConcurrentStatements = options.MaxConcurrentStatements;
        _maxConcurrentReadStatements = options.MaxConcurrentReadStatements;
        _connectionString = options.ConnectionString;

        _logger.LogInformation("DatabaseManager settings: (maxRetries:{MaxRetries}, retryDelayMilliseconds: {RetryDelayMilliseconds}, maxConcurrentStatements: {MaxConcurrentStatements}, maxConcurrentReadStatements: {MaxConcurrentReadStatements})",
            _maxRetries, _retryDelayMilliseconds, _maxConcurrentStatements, _maxConcurrentReadStatements);

        _connectionLimiter = new(_maxConcurrentStatements);
        _readConnectionLimiter = new(_maxConcurrentReadStatements);
    }

    /// <summary>
    /// Gets a value indicating whether the executor is enabled.
    /// </summary>
    public bool IsEnabled => !string.IsNullOrEmpty(_connectionString);

    /// <summary>
    /// Executes a query statement asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public async Task<DbStmtResult> ExecuteQuery(IPostgresStatement stmt, CancellationToken ct) {
        try {
            using var limiter = new SemaphoreLocker(_readConnectionLimiter);
            await limiter.Acquire(ct);
            using var connection = new NpgsqlConnection(_connectionString); // Does not throw
            await connection.OpenAsync(ct);
            return await stmt.Execute(connection, ct);
        } catch (NpgsqlException ex) {
            _logger.LogError(ex, "Error in ExecuteQuery");
            throw;
        }
    }

    /// <summary>
    /// Executes a query statement with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <param name="transaction">Optional transaction to use for the execution.</param>
    /// <returns>The result of the statement execution.</returns>
    public async Task<DbStmtResult> ExecuteQueryWithRetry(
        IPostgresStatement stmt,
        CancellationToken ct,
        int? overrideMaxRetries = null,
        PostgresTransaction? transaction = null) {
        int effectiveMaxRetries = overrideMaxRetries ?? _maxRetries;
        if (effectiveMaxRetries == 0)
            effectiveMaxRetries = int.MaxValue;

        for (int retries = 0; retries < effectiveMaxRetries; retries++) {
            try {
                return transaction is not null ? await ExecuteUnderTransaction(stmt, transaction, ct) : await ExecuteQuery(stmt, ct);
            } catch (PostgresException) {
                // generated by the database server, do not retry
                throw;
            } catch (NpgsqlException) {
                await Task.Delay(TimeSpan.FromMilliseconds(_retryDelayMilliseconds));
                // continue to retry
            }

            // The following is implicit, and occurs on cancellation
            //catch (OperationCanceledException) {
            //    throw;
            //}
        }

        return DbStmtResult.StatementFailure(ErrorCodes.TooManyRetries, "Too many retries");
    }


    /// <summary>
    /// Executes a statement asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public async Task<DbStmtResult> Execute(IPostgresStatement stmt, CancellationToken ct) {
        try {
            using var limiter = new SemaphoreLocker(_connectionLimiter);
            await limiter.Acquire(ct);

            using var connection = new NpgsqlConnection(_connectionString); // Does not throw
            await connection.OpenAsync(ct);
            return await stmt.Execute(connection, ct);
        } catch (NpgsqlException ex) {
            _logger.LogError(ex, "Error in Execute");
            throw;
        }
    }

    /// <summary>
    /// Executes a statement within a transaction asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="transaction">The transaction to use for the execution.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the statement execution.</returns>
    public async Task<DbStmtResult> ExecuteUnderTransaction(IPostgresStatement stmt, PostgresTransaction transaction, CancellationToken ct) {
        try {
            NpgsqlConnection connection = transaction.Connection;
            return await stmt.Execute(connection, ct);
        } catch (NpgsqlException ex) {
            _logger.LogError(ex, "Error in ExecuteUnderTransaction");
            throw;
        }
    }

    /// <summary>
    /// Executes a statement with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <param name="transaction">Optional transaction to use for the execution.</param>
    /// <returns>The result of the statement execution.</returns>
    public async Task<DbStmtResult> ExecuteWithRetry(
        IPostgresStatement stmt,
        CancellationToken ct,
        int? overrideMaxRetries = null,
        PostgresTransaction? transaction = null) {
        int effectiveMaxRetries = overrideMaxRetries ?? _maxRetries;
        if (effectiveMaxRetries == 0)
            effectiveMaxRetries = int.MaxValue;

        for (int retries = 0; retries < effectiveMaxRetries; retries++) {
            try {
                return transaction is not null ? await ExecuteUnderTransaction(stmt, transaction, ct) : await Execute(stmt, ct);
            } catch (PostgresException) {
                // generated by the database server, do not retry
                throw;
            } catch (NpgsqlException) {
                await Task.Delay(TimeSpan.FromMilliseconds(_retryDelayMilliseconds));
                // continue to retry
            }

            // The following is implicit, and occurs on cancellation
            //catch (OperationCanceledException) {
            //    throw;
            //}
        }

        return DbStmtResult.StatementFailure(ErrorCodes.TooManyRetries, "Too many retries");
    }

    /// <summary>
    /// Executes a statement within a transaction with retry logic asynchronously.
    /// </summary>
    /// <param name="stmt">The statement to execute.</param>
    /// <param name="transaction">The transaction to use for the execution.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <param name="overrideMaxRetries">Optional override for the maximum number of retries.</param>
    /// <returns>The result of the statement execution.</returns>
    public Task<DbStmtResult> ExecuteUnderTransactionWithRetry(
        IPostgresStatement stmt, PostgresTransaction transaction, CancellationToken ct, int? overrideMaxRetries = null)
        => ExecuteWithRetry(stmt, ct, overrideMaxRetries, transaction);

    /// <summary>
    /// Begins a new transaction asynchronously.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The created transaction.</returns>
    public async Task<PostgresTransaction> BeginTransaction(CancellationToken ct) {
        using var limiter = new SemaphoreLocker(_connectionLimiter);
        await limiter.Acquire(ct);

        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        NpgsqlTransaction transaction = await connection.BeginTransactionAsync(ct);

        return new PostgresTransaction(connection, transaction, limiter);
    }

    #region IDisposable implementation

    /// <summary>
    /// Releases the resources used by the executor.
    /// </summary>
    /// <param name="disposing">A value indicating whether the method is being called from the <see cref="Dispose()"/> method.</param>
    protected virtual void Dispose(bool disposing) {
        if (!_disposedValue) {
            if (disposing) {
                _connectionLimiter.Dispose();
                _readConnectionLimiter.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }

    /// <summary>
    /// Releases the resources used by the executor.
    /// </summary>
    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
