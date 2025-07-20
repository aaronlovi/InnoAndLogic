namespace InnoAndLogic.Shared.Models;

/// <summary>
/// Defines error codes for various application scenarios.
/// </summary>
public enum ErrorCodes : int {
    /// <summary>
    /// Indicates no error.
    /// </summary>
    None = 0,

    /// <summary>
    /// Indicates a generic error.
    /// </summary>
    GenericError = 1,

    /// <summary>
    /// Indicates that a requested resource was not found.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Indicates that too many retries were attempted.
    /// </summary>
    TooManyRetries = 3,

    /// <summary>
    /// Indicates a duplicate resource or entry.
    /// </summary>
    Duplicate = 4,

    /// <summary>
    /// Indicates a validation error.
    /// </summary>
    ValidationError = 5,

    /// <summary>
    /// Indicates a concurrency conflict.
    /// </summary>
    ConcurrencyConflict = 6,

    /// <summary>
    /// Indicates a serialization error.
    /// </summary>
    SerializationError = 7,

    /// <summary>
    /// Indicates a parsing error.
    /// </summary>
    ParsingError = 8
}
