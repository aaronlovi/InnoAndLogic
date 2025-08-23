namespace InnoAndLogic.Persistence;

/// <summary>
/// Specifies the database provider used by the application.
/// </summary>
public enum DatabaseProvider {
    /// <summary>
    /// Represents an invalid or uninitialized database provider.
    /// </summary>
    Invalid = 0,

    /// <summary>
    /// Represents an in-memory database provider, typically used for testing.
    /// </summary>
    InMemory = 1,

    /// <summary>
    /// Represents a PostgreSQL database provider.
    /// </summary>
    Postgres = 2
}
