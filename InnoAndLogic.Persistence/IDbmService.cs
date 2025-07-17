using System.Threading;
using System.Threading.Tasks;

namespace InnoAndLogic.Persistence;

public interface IDbmService {
    // Id generator
    ValueTask<ulong> GetNextId64(CancellationToken ct);
    ValueTask<ulong> GetIdRange64(uint count, CancellationToken ct);
}
