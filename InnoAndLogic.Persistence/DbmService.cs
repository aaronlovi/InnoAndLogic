using System;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Persistence.Migrations;
using InnoAndLogic.Persistence.Statements;
using Microsoft.Extensions.Logging;

namespace InnoAndLogic.Persistence;

public sealed class DbmService : IDbmService {
    private readonly ILogger<DbmService> _logger;
    private readonly PostgresExecutor _exec;
    private readonly SemaphoreSlim _generatorMutex;

    private ulong _lastUsed;
    private ulong _endId;

    public DbmService(
        ILoggerFactory loggerFactory,
        PostgresExecutor exec,
        DatabaseOptions options,
        DbMigrations migrations) {
        _logger = loggerFactory.CreateLogger<DbmService>();
        _exec = exec;
        string connStr = options.ConnectionString;
        if (string.IsNullOrEmpty(connStr))
            throw new InvalidOperationException("Connection string is empty");
        _generatorMutex = new(1);

        // Perform the DB migrations synchronously
        try {
            migrations.Up();
        } catch (Exception ex) {
            _logger.LogError(ex, "Failed to perform DB migrations, aborting");
            throw;
        }
    }

    #region Generator

    public ValueTask<ulong> GetNextId64(CancellationToken ct) => GetIdRange64(1, ct);

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
        DbStmtResult res = await _exec.ExecuteWithRetry(stmt, ct, 0);

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
