using System.Collections.Generic;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

internal sealed class ReserveIdRangeStmt : QueryDbStmtBase {
    private const string sql = "UPDATE generator SET last_reserved = last_reserved + @numToGet RETURNING last_reserved";

    private readonly long _numIds;

    public long LastReserved { get; set; }

    public ReserveIdRangeStmt(long numIds) : base(sql, nameof(ReserveIdRangeStmt)) {
        _numIds = numIds;
    }

    protected override void ClearResults() { }

    protected override IReadOnlyCollection<NpgsqlParameter> GetBoundParameters() =>
        [new NpgsqlParameter<long>("numToGet", _numIds)];

    protected override bool ProcessCurrentRow(NpgsqlDataReader reader) {
        LastReserved = reader.GetInt64(0);
        return false;
    }
}