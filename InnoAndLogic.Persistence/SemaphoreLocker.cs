using System;
using System.Threading;
using System.Threading.Tasks;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Provides a mechanism to manage access to a resource using a semaphore.
/// </summary>
public sealed class SemaphoreLocker : IDisposable {
    private readonly SemaphoreSlim _semaphore;
    private bool _isAcquired;

    /// <summary>
    /// Initializes a new instance of the <see cref="SemaphoreLocker"/> class.
    /// </summary>
    /// <param name="semaphore">The <see cref="SemaphoreSlim"/> to manage access to the resource.</param>
    public SemaphoreLocker(SemaphoreSlim semaphore) {
        _semaphore = semaphore;
    }

    /// <summary>
    /// Acquires the semaphore asynchronously.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting to acquire the semaphore.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task Acquire(CancellationToken ct) {
        await _semaphore.WaitAsync(ct);
        _isAcquired = true;
    }

    /// <summary>
    /// Releases the semaphore if it has been acquired.
    /// </summary>
    public void Dispose() {
        if (_isAcquired) {
            _ = _semaphore.Release();
            _isAcquired = false;
        }
    }
}
