using System;
using Npgsql;
using NpgsqlTypes;

namespace InnoAndLogic.Persistence.Statements;

/// <summary>
/// Provides utility methods for working with Npgsql parameters.
/// </summary>
public static class PostgresUtils {
    /// <summary>
    /// Creates an Npgsql parameter for a nullable DateTimeOffset value.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="nullableDate">The nullable DateTimeOffset value.</param>
    /// <param name="dbType"></param>
    /// <returns>An NpgsqlParameter representing the nullable DateTimeOffset value.</returns>
    public static NpgsqlParameter CreateNullableDateTimeOffsetParam(
        string paramName, DateTimeOffset? nullableDate, NpgsqlDbType dbType = NpgsqlDbType.TimestampTz) {
        var param = new NpgsqlParameter(paramName, dbType) {
            Value = nullableDate?.UtcDateTime ?? (object)DBNull.Value
        };
        return param;
    }

    /// <summary>
    /// Creates an Npgsql parameter for a nullable DateTime value.
    /// </summary>
    /// <param name="paramName">The name of the parameter.</param>
    /// <param name="nullableDate">The nullable DateTimeOffset value.</param>
    /// <param name="dbType">The NpgsqlDbType for the parameter, default is Timestamp.</param>
    /// <returns>An NpgsqlParameter representing the nullable DateTimeOffset value.</returns>
    public static NpgsqlParameter CreateNullableDateTimeParam(
        string paramName, DateTime? nullableDate, NpgsqlDbType dbType = NpgsqlDbType.Timestamp) {
        var param = new NpgsqlParameter(paramName, dbType) {
            Value = nullableDate ?? (object)DBNull.Value
        };
        return param;
    }
}
