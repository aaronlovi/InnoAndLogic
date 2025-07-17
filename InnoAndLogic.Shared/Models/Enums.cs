namespace InnoAndLogic.Shared.Models;

public enum ErrorCodes : int {
    None = 0,
    GenericError = 1,
    NotFound = 2,
    TooManyRetries = 3,
    Duplicate = 4,
    ValidationError = 5,
    ConcurrencyConflict = 6,
    SerializationError = 7,
    ParsingError = 8,
}
