using System;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Persistence.Migrations;
using InnoAndLogic.Persistence.Statements;
using InnoAndLogic.Shared;
using Microsoft.Extensions.Logging;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Provides database management services, including ID generation and database migrations.
/// </summary>
public class DbmService : IDbmService {
    private readonly ILogger<DbmService> _logger;
    private readonly SemaphoreSlim _generatorMutex;

    private ulong _lastUsed;
    private ulong _endId;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbmService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for the service.</param>
    /// <param name="exec">The <see cref="PostgresExecutor"/> used to execute database statements.</param>
    /// <param name="options">The <see cref="DatabaseOptions"/> containing the connection string and other database settings.</param>
    /// <param name="migrations">The <see cref="DbMigrations"/> instance used to apply database migrations.</param>
    /// <exception cref="InvalidOperationException">Thrown if the connection string is empty.</exception>
    public DbmService(
        ILoggerFactory loggerFactory,
        PostgresExecutor exec,
        DatabaseOptions options,
        DbMigrations migrations) {
        _logger = loggerFactory.CreateLogger<DbmService>();
        Executor = exec;
        if (options.IsConnectionStringRequired) {
            string connStr = options.ConnectionString;
            if (string.IsNullOrEmpty(connStr))
                throw new InvalidOperationException("Connection string is empty");
        }
        _generatorMutex = new(1);

        // Perform the DB migrations synchronously
        try {
            migrations.Up();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to perform DB migrations, aborting");
            throw;
        }
    }

    /// <summary>
    /// Gets the <see cref="PostgresExecutor"/> instance used to execute PostgreSQL statements.
    /// </summary>
    /// <remarks>
    /// The <see cref="PostgresExecutor"/> provides functionality for executing database queries,
    /// managing transactions, and handling retry logic for failed operations.
    /// This property allows access to the executor for advanced database operations.
    /// </remarks>
    public PostgresExecutor Executor { get; }

    #region Generator

    /// <summary>
    /// Gets the next available 64-bit ID.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the next available 64-bit ID.</returns>
    public ValueTask<ulong> GetNextId64(CancellationToken ct) => GetIdRange64(1, ct);

    /// <summary>
    /// Gets a range of 64-bit IDs.
    /// </summary>
    /// <param name="count">The number of IDs to reserve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first ID in the reserved range.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="count"/> is 0.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the database fails to reserve the requested range of IDs.</exception>
    public async ValueTask<ulong> GetIdRange64(uint count, CancellationToken ct) {
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be 0");

        // Optimistic path: in most cases
        lock (_generatorMutex) {
            if (_lastUsed + count <= _endId) {
                ulong result = _lastUsed + 1;
                _lastUsed += count;
                return result;
            }
        }

        // Lock the DB update mutex
        using var locker = new SemaphoreLocker(_generatorMutex);
        await locker.Acquire(ct);

        // May have been changed already by another thread, so check again
        lock (_generatorMutex) {
            if (_lastUsed + count <= _endId) {
                ulong result = _lastUsed + 1;
                _lastUsed += count;
                return result;
            }
        }

        // Update in blocks
        const uint BLOCK_SIZE = 65536;
        uint idRange = count - (count % BLOCK_SIZE) + BLOCK_SIZE;
        var stmt = new ReserveIdRangeStmt(idRange);
        Result res = await Executor.ExecuteWithRetry(stmt, ct, 0);

        if (res.IsSuccess) {
            lock (_generatorMutex) {
                _endId = (ulong)stmt.LastReserved;
                _lastUsed = (ulong)(stmt.LastReserved - BLOCK_SIZE);
                ulong result = _lastUsed + 1;
                _lastUsed += count;
                return result;
            }
        } else {
            throw new InvalidOperationException("Failed to get next id from database");
        }
    }

    #endregion
}
