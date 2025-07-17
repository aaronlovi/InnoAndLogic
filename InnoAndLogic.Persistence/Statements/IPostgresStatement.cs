using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

public interface IPostgresStatement {
    Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct);
}
