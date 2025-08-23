using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Provides database management services, including ID generation and database migrations.
/// All data in memory only. Suitable for unit tests.
/// </summary>
public class DbmInMemoryService : IDbmService {
    private readonly ILogger<DbmInMemoryService> _logger;

    // Generator
    private ulong _lastUsed;


    /// <summary>
    /// Gets the synchronization object used to ensure thread-safe operations within the service.
    /// </summary>
    /// <remarks>
    /// The <see cref="Locker"/> is used to lock critical sections of code to prevent race conditions
    /// during in-memory database operations.
    /// </remarks>
    protected object Locker { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbmInMemoryService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for the service.</param>
    /// <remarks>
    /// This service provides database management functionality entirely in memory,
    /// making it suitable for unit tests or scenarios where persistence is not required.
    /// </remarks>
    public DbmInMemoryService(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<DbmInMemoryService>();
        Locker = new();
        _logger.LogWarning("DbmInMemory is instantiated: persistence in RAM only");
    }

    #region GENERATOR

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
    public ValueTask<ulong> GetIdRange64(uint count, CancellationToken ct) {
        if (count == 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be 0");

        // Always in memory
        lock (Locker) {
            ulong result = _lastUsed + 1;
            _lastUsed += count;
            return ValueTask.FromResult(result);
        }
    }

    #endregion
}
