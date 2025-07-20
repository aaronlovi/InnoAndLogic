using System.Threading;
using System.Threading.Tasks;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Defines the interface for database management services, including ID generation.
/// </summary>
public interface IDbmService {
    /// <summary>
    /// Gets the next available 64-bit ID.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the next available 64-bit ID.</returns>
    ValueTask<ulong> GetNextId64(CancellationToken ct);

    /// <summary>
    /// Gets a range of 64-bit IDs.
    /// </summary>
    /// <param name="count">The number of IDs to reserve.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the first ID in the reserved range.</returns>
    ValueTask<ulong> GetIdRange64(uint count, CancellationToken ct);
}
