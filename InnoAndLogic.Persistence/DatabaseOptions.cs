namespace InnoAndLogic.Persistence;

public class DatabaseOptions {
    public int MaxRetries { get; set; } = 5;
    public int RetryDelayMilliseconds { get; set; } = 1000;
    public int MaxConcurrentStatements { get; set; } = 20;
    public int MaxConcurrentReadStatements { get; set; } = 20;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseSchema { get; set; } = string.Empty;
}
