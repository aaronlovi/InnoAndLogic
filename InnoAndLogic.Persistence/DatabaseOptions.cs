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
    /// Gets or sets the connection string used to connect to the database.
    /// This value must be provided and cannot be empty.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the database schema to be used for operations.
    /// This value must be provided and cannot be empty.
    /// </summary>
    public string DatabaseSchema { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the placeholders for database operations.
    /// This dictionary allows dynamic replacement of placeholders in migration scripts.
    /// </summary>
    public Dictionary<string, string> Placeholders { get; set; } = new Dictionary<string, string>();
}
