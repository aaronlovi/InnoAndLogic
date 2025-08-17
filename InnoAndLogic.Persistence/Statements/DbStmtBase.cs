using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InnoAndLogic.Shared.Models;
using Npgsql;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Represents the base class for bulk insert database statements executed against a PostgreSQL database.
/// </summary>
/// <typeparam name="TItemType">The type of the items to be inserted.</typeparam>
public abstract class PostgresBulkInsertDbStmtBase<TItemType>(string _className, IReadOnlyCollection<TItemType> _items)
    : StatementBase<NpgsqlConnection, NpgsqlParameter>
    where TItemType : class {
    /// <summary>
    /// Gets the SQL COPY command used for the bulk insert operation.
    /// </summary>
    /// <returns>The SQL COPY command as a string.</returns>
    protected abstract string GetCopyCommand();

    /// <summary>
    /// Writes an individual item to the binary importer.
    /// </summary>
    /// <param name="writer">The <see cref="NpgsqlBinaryImporter"/> used to write the item.</param>
    /// <param name="item">The item to write.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    protected abstract Task WriteItemAsync(NpgsqlBinaryImporter writer, TItemType item);

    /// <inheritdoc/>
    public override async Task<DbStmtResult> Execute(NpgsqlConnection conn, CancellationToken ct) {
        TItemType? failedItem = default;

        try {
            using NpgsqlBinaryImporter writer = conn.BeginBinaryImport(GetCopyCommand());

            foreach (TItemType item in _items) {
                failedItem = item;
                await writer.StartRowAsync(ct);
                await WriteItemAsync(writer, item);
            }

            _ = await writer.CompleteAsync(ct);
            return DbStmtResult.StatementSuccess(_items.Count);
        } catch (PostgresException ex) {
            string errMsg = $"{_className} failed - {ex.Message}";
            ErrorCodes failureReason = ex.SqlState == "23505"
                ? ErrorCodes.Duplicate
                : ErrorCodes.GenericError;
            return DbStmtResult.StatementFailure(failureReason, errMsg);
        } catch (Exception ex) {
            string failedItemStr = failedItem?.ToString() ?? "NULL";
            string errMsg = $"{_className} failed - {ex.Message}. Item: {failedItemStr}";
            return DbStmtResult.StatementFailure(ErrorCodes.GenericError, errMsg);
        }
    }
}
