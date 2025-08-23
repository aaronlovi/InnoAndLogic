using System.Collections.Generic;

namespace InnoAndLogic.Persistence;

/// <summary>
/// Represents configuration options for database operations.
/// </summary>
public class DatabaseOptions {
    /// <summary>
    /// Gets or sets the maximum number of retries for database operations in case of failure.
    /// Default value is 5.
    /// </summary>
    public int MaxRetries { get; set; } = 5;

    /// <summary>
    /// Gets or sets the delay, in milliseconds, between retry attempts for database operations.
    /// Default value is 1000 milliseconds (1 second).
    /// </summary>
    public int RetryDelayMilliseconds { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum number of concurrent database statements that can be executed.
    /// Default value is 20.
    /// </summary>
    public int MaxConcurrentStatements { get; set; } = 20;

    /// <summary>
    /// Gets or sets the maximum number of concurrent read-only database statements that can be executed.
    /// Default value is 20.
    /// </summary>
    public int MaxConcurrentReadStatements { get; set; } = 20;

    /// <summary>
    /// Gets or sets the database provider to be used for operations.
    /// </summary>
    /// <remarks>
    /// The database provider determines the type of database backend used by the application.
    /// Supported values include <see cref="DatabaseProvider.InMemory"/> for in-memory databases
    /// and <see cref="DatabaseProvider.Postgres"/> for PostgreSQL.
    /// </remarks>
    public DatabaseProvider Provider { get; set; } = DatabaseProvider.Postgres;

    /// <summary>
    /// Gets or sets the connection string used to connect to the database.
    /// If empty, the database is considered in-memory only.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database schema to be used for operations.
    /// </summary>
    public string DatabaseSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholders for database operations.
    /// This dictionary allows dynamic replacement of placeholders in migration scripts.
    /// </summary>
    public Dictionary<string, string> Placeholders { get; set; } = [];

    /// <summary>
    /// Indicates that the database is in-memory only (i.e., no persistent storage).
    /// </summary>
    public bool InMemoryOnly => string.IsNullOrEmpty(ConnectionString);

    /// <summary>
    /// Gets a value indicating whether a connection string is required for the current database provider.
    /// </summary>
    public bool IsConnectionStringRequired => Provider != DatabaseProvider.InMemory;
}
