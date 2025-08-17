using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared.Models;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for non-query database statements executed against a PostgreSQL database.
/// </summary>
public abstract class NonQueryDbStmtBase<TConnectionType, TParameterType>(string _sql, string _className) 
    : StatementBase<TConnectionType, TParameterType>
    where TConnectionType : DbConnection
    where TParameterType : DbParameter {
    /// <inheritdoc/>
    public async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        try {
            using var cmd = new NpgsqlCommand(_sql, conn);
            foreach (TParameterType boundParam in GetBoundParameters())
                _ = cmd.Parameters.Add(boundParam);
            await cmd.PrepareAsync(ct);
            int numRows = await cmd.ExecuteNonQueryAsync(ct);
            return DbStmtResult.StatementSuccess(numRows);
        } catch (Exception ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }
}
